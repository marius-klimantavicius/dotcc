#!/usr/bin/env python3
"""Run the pinned native SQLite oracle, in its own process, on fresh file state."""
from __future__ import annotations

import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import shutil
import struct
import sys


INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
BUILD_RECEIPT_SHA256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435"
LIBRARY_SHA256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"
WORKLOAD_SHA256 = "8f9cdc8262ac0c8b29f73fd6a375fd693abf0311dfc4335493b8e5c02ef75a8a"


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def sidecars(path: Path) -> list[Path]:
    return [candidate for suffix in ("-wal", "-shm", "-journal")
            if (candidate := Path(str(path) + suffix)).exists()]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True, help="A directory that does not yet exist.")
    parser.add_argument("--mode", choices=("stateful", "interchange-append", "interchange-verify"), default="stateful")
    parser.add_argument("--build-receipt", type=Path,
                        default=Path(__file__).resolve().parents[2] / "acceptance/sqlite/native-oracle-build.json")
    parser.add_argument("--workload", type=Path, default=Path(__file__).with_name("workload.json"))
    parser.add_argument("--input-report", type=Path, help="Closed producing process's report; mandatory for interchange.")
    args = parser.parse_args()
    if (args.mode == "stateful") != (args.input_report is None):
        parser.error("Use --input-report only, and always, for an interchange mode.")
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    output = artifacts / "stateful-report.json"
    report: dict = {"status": "incomplete", "role": "separate-native-test-oracle-only", "mode": args.mode,
                    "generatedExecution": False, "nativeApplicationFallbackPermitted": False,
                    "connectionsClosed": False, "databaseState": None, "observations": [],
                    "lockScope": "two connections in one process; explicit ordered SQL barriers, no scheduler timing",
                    "interchangeClosureRequirement": "coordinator waits for producing process exit before consumption"}
    databases: dict[str, c.c_void_p] = {}
    input_path: Path | None = None
    input_hash: str | None = None
    close_db = None
    failure = None
    frozen_files: dict[Path, str] = {}

    try:
        if any(os.environ.get(name) for name in ("LD_PRELOAD", "LD_AUDIT", "LD_LIBRARY_PATH")):
            raise RuntimeError("The native oracle requires an explicit loader environment without preload/audit/search overrides.")
        if digest(args.build_receipt) != BUILD_RECEIPT_SHA256:
            raise RuntimeError("The pinned native-oracle-build.json bytes changed.")
        build = json.loads(args.build_receipt.read_text())
        library = Path(build["library"]["path"]).resolve(strict=True)
        actual_library = receipt(library)
        if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != INPUT_SHA256
                or build["applicationNativeFallbackPermitted"] is not False
                or actual_library["sha256"] != LIBRARY_SHA256 or actual_library["sha256"] != build["library"]["sha256"]
                or actual_library["bytes"] != build["library"]["bytes"]):
            raise RuntimeError("The exact pinned native oracle library/input/role differs from its receipt.")
        if digest(args.workload) != WORKLOAD_SHA256:
            raise RuntimeError("The frozen finite workload bytes changed.")
        workload = json.loads(args.workload.read_text())
        if workload["workloadVersion"] != 1 or workload["sourceObjectSha256"] != INPUT_SHA256:
            raise RuntimeError("The workload version/input identity changed.")
        mode = workload["modes"][args.mode]
        limits = workload["limits"]
        if len(mode["actions"]) > limits["maximumActionsPerMode"]:
            raise RuntimeError("Frozen action budget exceeded.")
        report.update(library=actual_library, buildReceipt=receipt(args.build_receipt), workload=receipt(args.workload),
                      runner=receipt(Path(__file__)), remainingT4Areas=workload["remainingT4Areas"])
        frozen_files = {args.build_receipt: BUILD_RECEIPT_SHA256, args.workload: WORKLOAD_SHA256,
                        library: LIBRARY_SHA256, Path(__file__): report["runner"]["sha256"]}
        database_file = artifacts / workload["databaseFileName"]
        if database_file.name != "stateful.db" or database_file.parent != artifacts:
            raise RuntimeError("The database must remain inside the fresh artifact directory.")
        if args.input_report is not None:
            incoming = json.loads(args.input_report.read_text())
            if (incoming["status"] not in ("passed-native-stateful", "passed-generated-stateful")
                    or incoming["connectionsClosed"] is not True
                    or incoming["databaseState"] != mode["inputState"]
                    or incoming["workload"]["sha256"] != WORKLOAD_SHA256):
                raise RuntimeError("The incoming report does not describe the expected closed workload state.")
            input_path = Path(incoming["database"]["path"]).resolve(strict=True)
            input_hash = incoming["database"]["sha256"]
            actual_input = receipt(input_path)
            if (actual_input["sha256"] != input_hash or actual_input["bytes"] != incoming["database"]["bytes"]
                    or sidecars(input_path)):
                raise RuntimeError("The exact closed input changed or has untransferred journal/WAL/shared-memory sidecars.")
            shutil.copyfile(input_path, database_file)
            if digest(input_path) != input_hash or digest(database_file) != input_hash:
                raise RuntimeError("The incoming closed file changed while it was copied.")
            report["incomingTransfer"] = {"report": receipt(args.input_report), "database": actual_input,
                                           "copiedDatabase": receipt(database_file),
                                           "closedStateEvidence": "prior report and absent sidecars; producing process exit required externally"}
            frozen_files[args.input_report] = report["incomingTransfer"]["report"]["sha256"]

        # The only SQLite loader is this exact audited path in this native-only
        # process. There is no Python sqlite3 import or system-library lookup.
        api = c.CDLL(str(library), mode=os.RTLD_NOW | os.RTLD_LOCAL)
        pointer = c.c_void_p

        bound_apis: list[str] = []

        def bind(name: str, result, *arguments):
            function = getattr(api, "sqlite3_" + name)
            function.restype, function.argtypes = result, list(arguments)
            bound_apis.append("sqlite3_" + name)
            return function

        version = bind("libversion", c.c_char_p)
        version_number = bind("libversion_number", c.c_int)
        open_db = bind("open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
        close_db = bind("close", c.c_int, pointer)
        error = bind("errmsg", c.c_char_p, pointer)
        prepare = bind("prepare_v2", c.c_int, pointer, pointer, c.c_int, c.POINTER(pointer), c.POINTER(pointer))
        step = bind("step", c.c_int, pointer)
        finalize = bind("finalize", c.c_int, pointer)
        column_count = bind("column_count", c.c_int, pointer)
        column_name = bind("column_name", c.c_char_p, pointer, c.c_int)
        column_type = bind("column_type", c.c_int, pointer, c.c_int)
        column_integer = bind("column_int64", c.c_int64, pointer, c.c_int)
        column_real = bind("column_double", c.c_double, pointer, c.c_int)
        column_text = bind("column_text", pointer, pointer, c.c_int)
        column_blob = bind("column_blob", pointer, pointer, c.c_int)
        column_bytes = bind("column_bytes", c.c_int, pointer, c.c_int)
        report["apiSymbols"] = bound_apis
        report["sqliteVersion"], report["sqliteVersionNumber"] = version().decode("ascii"), version_number()
        if report["sqliteVersionNumber"] != workload["sqliteVersionNumber"]:
            raise RuntimeError("Expected the pinned SQLite 3.54.0 native oracle.")

        def value(statement: pointer, index: int) -> dict:
            kind = column_type(statement, index)
            if kind == 1:
                return {"type": "integer", "decimal": str(column_integer(statement, index))}
            if kind == 2:
                return {"type": "real", "binary64Hex": struct.pack(">d", column_real(statement, index)).hex()}
            if kind in (3, 4):
                address = (column_text if kind == 3 else column_blob)(statement, index)
                count = column_bytes(statement, index)
                if count < 0 or count > limits["maximumColumnBytes"] or count and not address:
                    raise RuntimeError("Invalid or oversized borrowed column extent.")
                return {"type": "text-utf8" if kind == 3 else "blob", "bytes": count,
                        "hex": c.string_at(address, count).hex() if count else ""}
            if kind == 5:
                return {"type": "null"}
            raise RuntimeError("Unexpected SQLite storage class: " + str(kind))

        for action in mode["actions"]:
            name = action["connection"]
            observed = {key: action[key] for key in ("id", "action", "connection")}
            report["observations"].append(observed)
            if action["action"] == "open":
                if name in databases:
                    raise RuntimeError("A connection ID was opened twice without closing.")
                database = pointer()
                code = open_db(os.fsencode(database_file), c.byref(database), action["flags"], None)
                observed["code"] = code
                if database:
                    databases[name] = database
                if code != action["expectedCode"] or not database:
                    raise RuntimeError("Unexpected open result for " + action["id"])
                continue
            if name not in databases:
                raise RuntimeError("An action uses a connection that is not open: " + name)
            database = databases[name]
            if action["action"] == "close":
                code = close_db(database)
                observed["code"] = code
                if code == 0:
                    del databases[name]
                if code != action["expectedCode"]:
                    raise RuntimeError("Unexpected close result for " + action["id"])
                continue
            if action["action"] != "sql":
                raise RuntimeError("Unsupported frozen action: " + action["action"])
            sql = action["sql"]
            if "\0" in sql:
                raise RuntimeError("A SQL action contains an embedded NUL.")
            encoded = sql.encode("utf-8")
            owner = c.create_string_buffer(encoded)
            statement, tail = pointer(), pointer()
            observed.update(sql=sql, columns=[], rows=[], prepareCode=None, stepCode=None, finalizeCode=None)
            try:
                code = prepare(database, c.cast(owner, pointer), len(encoded) + 1, c.byref(statement), c.byref(tail))
                observed["prepareCode"] = code
                if code != 0:
                    message = error(database)
                    raise RuntimeError(f"prepare {action['id']}: {code}: {message!r}")
                start = c.addressof(owner)
                if not statement or tail.value is None or not start <= tail.value <= start + len(encoded):
                    raise RuntimeError("The SQL action did not prepare one statement with a bounded tail.")
                if encoded[tail.value - start:].strip(b" \t\r\n\v\f"):
                    raise RuntimeError("Every action must contain exactly one SQL statement.")
                count = column_count(statement)
                if count < 0 or count > limits["maximumColumns"]:
                    raise RuntimeError("Frozen result-column budget exceeded.")
                observed["columns"] = [column_name(statement, index).decode("utf-8") for index in range(count)]
                while True:
                    code = step(statement)
                    if code != 100:
                        observed["stepCode"] = code
                        break
                    observed["rows"].append([value(statement, index) for index in range(count)])
                    if len(observed["rows"]) > limits["maximumRowsPerStatement"]:
                        raise RuntimeError("Frozen result-row budget exceeded.")
            finally:
                if statement:
                    # In particular, BUSY's statement is finalized before any
                    # later transaction action can run. Preserve its code5.
                    observed["finalizeCode"] = finalize(statement)
            expected = action["expected"]
            expected_rows = workload["rowSets"][expected["rowsRef"]] if "rowsRef" in expected else expected["rows"]
            if (observed["columns"] != expected["columns"] or observed["rows"] != expected_rows
                    or observed["stepCode"] != expected["stepCode"] or observed["finalizeCode"] != expected["finalizeCode"]):
                # Keep actual observations, including checkpoint and BUSY
                # tuples, in the incomplete report before rejecting a mismatch.
                raise RuntimeError("Observed SQL protocol differs at " + action["id"])
        if databases:
            raise RuntimeError("The finite action list left an open connection.")
        if not database_file.is_file() or sidecars(database_file):
            raise RuntimeError("The final database is missing or still has untransferred sidecars.")
        report["database"] = receipt(database_file)
        report["databaseState"] = mode["outputState"]
        loaded = sorted({parts[-1] for line in Path("/proc/self/maps").read_text().splitlines()
                         if len(parts := line.split()) >= 6 and parts[-1].startswith("/") and ".so" in parts[-1]})
        if [path for path in loaded if "sqlite" in Path(path).name.lower()] != [str(library)]:
            raise RuntimeError("The native oracle loaded an unexpected second SQLite provider.")
        report["loadedLibraries"] = [receipt(Path(path)) for path in loaded]
        report["observationsSha256"] = hashlib.sha256(
            json.dumps(report["observations"], sort_keys=True, separators=(",", ":")).encode()).hexdigest()
        report["status"] = "passed-native-stateful"
    except Exception as problem:
        failure = str(problem)
        report["error"] = failure
    finally:
        cleanup = []
        if close_db is not None:
            for name, database in list(databases.items()):
                code = close_db(database)
                cleanup.append({"connection": name, "closeCode": code})
                if code == 0:
                    del databases[name]
        if cleanup:
            report["cleanup"] = cleanup
        report["connectionsClosed"] = not databases
        if input_path is not None and (not input_path.is_file() or digest(input_path) != input_hash or sidecars(input_path)):
            report["status"] = "incomplete"
            report["inputFreezeFailure"] = "The incoming database or its absent-sidecar condition changed."
        changed = [str(path) for path, sha in frozen_files.items() if not path.is_file() or digest(path) != sha]
        report["pinnedFilesUnchanged"] = not changed
        if changed:
            report["status"] = "incomplete"
            report["pinnedFileFreezeFailure"] = changed
        with output.open("x") as stream:
            json.dump(report, stream, indent=2)
            stream.write("\n")
    print(json.dumps({"status": report["status"], "mode": args.mode, "actions": len(report["observations"]),
                      "report": str(output)}))
    if failure:
        print(failure, file=sys.stderr)
    return 0 if report["status"] == "passed-native-stateful" else 1


if __name__ == "__main__":
    sys.exit(main())

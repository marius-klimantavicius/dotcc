#!/usr/bin/env python3
"""Pinned separate native oracle: hard-heap NOMEM and injected pager-commit IOERR."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import sys

WORKLOAD_SHA256 = "f1cfaffcc508d27de1c7aedca10744b15a04300d06a25911e70666533a83d6bf"
BUILD_SHA256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435"
LIBRARY_SHA256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"
INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path):
    path = Path(path)
    return {"path": str(path.resolve()), "bytes": path.stat().st_size, "sha256": digest(path)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--workload", type=Path, default=Path(__file__).with_name("workload.json"))
    parser.add_argument("--build-receipt", type=Path, default=Path(__file__).resolve().parents[2] / "acceptance/sqlite/native-oracle-build.json")
    args = parser.parse_args()
    artifacts = args.artifacts.resolve(); artifacts.mkdir(parents=True, exist_ok=False)
    report = {"status": "incomplete", "role": "separate-native-test-oracle-only", "generatedExecution": False,
              "nativeApplicationFallbackPermitted": False, "cases": [], "connectionsClosed": False,
              "faultScope": "documented sqlite3FaultSim(400) pager commit injection, not an actual OS/device failure"}
    database = c.c_void_p()
    active = {}
    close = finalize = hard_limit = test_control = None
    saved_limit = None
    hook_installed = False
    armed = [False]
    frozen = {}
    failed = None
    try:
        if any(os.environ.get(key) for key in ("LD_PRELOAD", "LD_AUDIT", "LD_LIBRARY_PATH")):
            raise RuntimeError("Use a native oracle loader environment without overrides.")
        if digest(args.workload) != WORKLOAD_SHA256 or digest(args.build_receipt) != BUILD_SHA256:
            raise RuntimeError("The frozen workload or native build receipt changed.")
        work, build = json.loads(args.workload.read_text()), json.loads(args.build_receipt.read_text())
        library = Path(build["library"]["path"]).resolve(strict=True)
        if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != INPUT_SHA256
                or build["applicationNativeFallbackPermitted"] is not False or digest(library) != LIBRARY_SHA256
                or build["library"]["sha256"] != LIBRARY_SHA256 or library.stat().st_size != build["library"]["bytes"]):
            raise RuntimeError("The exact pinned native oracle identity changed.")
        report.update(library=receipt(library), buildReceipt=receipt(args.build_receipt), workload=receipt(args.workload),
                      runner=receipt(Path(__file__)), remainingT4Areas=work["remainingT4Areas"])
        frozen = {args.workload: WORKLOAD_SHA256, args.build_receipt: BUILD_SHA256, library: LIBRARY_SHA256,
                  Path(__file__): report["runner"]["sha256"]}
        api = c.CDLL(str(library), mode=os.RTLD_NOW | os.RTLD_LOCAL)
        pointer = c.c_void_p
        symbols = []

        def bind(name, result, *arguments):
            function = getattr(api, "sqlite3_" + name)
            function.restype, function.argtypes = result, list(arguments)
            symbols.append("sqlite3_" + name)
            return function

        version = bind("libversion", c.c_char_p)
        version_number = bind("libversion_number", c.c_int)
        open_db = bind("open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
        close = bind("close", c.c_int, pointer)
        prepare = bind("prepare_v2", c.c_int, pointer, pointer, c.c_int, c.POINTER(pointer), c.POINTER(pointer))
        step = bind("step", c.c_int, pointer)
        finalize = bind("finalize", c.c_int, pointer)
        errcode = bind("errcode", c.c_int, pointer)
        extended = bind("extended_errcode", c.c_int, pointer)
        errmsg = bind("errmsg", c.c_char_p, pointer)
        autocommit = bind("get_autocommit", c.c_int, pointer)
        memory_used = bind("memory_used", c.c_int64)
        hard_limit = bind("hard_heap_limit64", c.c_int64, c.c_int64)
        # True native variadic call: one fixed int and one pointer argument.
        test_control = bind("test_control", c.c_int, c.c_int)
        column_count = bind("column_count", c.c_int, pointer)
        column_name = bind("column_name", c.c_char_p, pointer, c.c_int)
        column_type = bind("column_type", c.c_int, pointer, c.c_int)
        column_int = bind("column_int64", c.c_int64, pointer, c.c_int)
        column_text = bind("column_text", pointer, pointer, c.c_int)
        column_bytes = bind("column_bytes", c.c_int, pointer, c.c_int)
        report["apiSymbols"] = symbols

        def require(value, message):
            if not value: raise RuntimeError(message)

        def errors():
            return {"errorCode": errcode(database), "extendedErrorCode": extended(database), "message": errmsg(database).decode("utf-8")}

        def opened(filename, flags):
            nonlocal database
            require(not database, "An earlier connection is still open.")
            code = open_db(os.fsencode(filename), c.byref(database), flags, None)
            require(code == 0 and database, "Open failed.")
            return code

        def closed():
            nonlocal database
            code = close(database)
            if code == 0: database = pointer()
            require(code == 0, "Close failed.")
            return code

        def prepared(sql):
            encoded = sql.encode("utf-8"); owner = c.create_string_buffer(encoded)
            statement, tail = pointer(), pointer()
            code = prepare(database, c.cast(owner, pointer), len(encoded) + 1, c.byref(statement), c.byref(tail))
            if statement: active[statement.value] = statement
            require(code == 0, "Prepare failed: " + sql)
            start = c.addressof(owner)
            require(statement and tail.value is not None and start <= tail.value <= start + len(encoded)
                    and not encoded[tail.value - start:].strip(b" \t\n\r\v\f"), "Expected exactly one prepared SQL statement.")
            return statement

        def finish(statement):
            active.pop(statement.value)  # Clear ownership before finalizing, exactly once.
            return finalize(statement)

        def read(statement):
            count = column_count(statement)
            require(0 <= count <= 8, "Column budget exceeded.")
            value = {"columns": [column_name(statement, i).decode("utf-8") for i in range(count)], "rows": [], "stepCode": None}
            while True:
                code = step(statement)
                if code != 100:
                    value["stepCode"] = code
                    if code != 101: value["error"] = errors()
                    return value
                row = []
                for index in range(count):
                    kind = column_type(statement, index)
                    if kind == 1: cell = {"type": "integer", "decimal": str(column_int(statement, index))}
                    elif kind == 3:
                        address = column_text(statement, index); size = column_bytes(statement, index)
                        require(0 <= size <= 4096 and (not size or address), "Invalid text extent.")
                        cell = {"type": "text-utf8", "bytes": size, "hex": c.string_at(address, size).hex() if size else ""}
                    else: raise RuntimeError("Unexpected storage class in this finite workload.")
                    row.append(cell)
                value["rows"].append(row); require(len(value["rows"]) <= 16, "Row budget exceeded.")

        def query(sql):
            statement = prepared(sql); value = {"sql": sql, **read(statement)}
            value["finalizeCode"] = finish(statement)
            require(value["stepCode"] == 101 and value["finalizeCode"] == 0, "Expected successful SQL action: " + sql)
            return value

        def expected_rows(values):
            return [[{"type": "integer", "decimal": str(value)} for value in row] for row in values]

        def recover_transaction(observed):
            state = autocommit(database); require(state in (0, 1), "Invalid autocommit state.")
            observed["autocommitAfterFinalize"] = state
            observed["rollback"] = query("ROLLBACK") if state == 0 else None

        report["sqliteVersion"], report["sqliteVersionNumber"] = version().decode("ascii"), version_number()
        require(work["workloadVersion"] == 1 and report["sqliteVersionNumber"] == work["sqliteVersionNumber"], "Expected pinned SQLite3.54.0.")
        allocation = work["allocation"]
        case = {"name": "controlled-hard-heap-nomem", "setup": [], "openCode": opened(":memory:", 6)}
        report["cases"].append(case)
        for name in ("schema", "seed", "begin", "tentative"): case["setup"].append(query(allocation[name]))
        demand = prepared(allocation["demand"])
        saved_limit = hard_limit(-1)
        used = memory_used(); cap = used + allocation["memoryHeadroomBytes"]
        audit = {"previousHardLimit": saved_limit, "memoryUsedBeforeCap": used, "requestedHardLimit": cap}
        report["allocationAudit"] = audit
        require(saved_limit >= 0 and used > 0 and cap > used, "Invalid live allocation accounting/limit.")
        try:
            audit["setReturnedPrevious"] = hard_limit(cap); audit["observedHardLimit"] = hard_limit(-1)
            require(audit["observedHardLimit"] == cap and audit["setReturnedPrevious"] == saved_limit, "Hard limit was not installed.")
            case["failure"] = {"sql": allocation["demand"], **read(demand)}
        finally:
            audit["restoreReturnedPrevious"] = hard_limit(saved_limit); audit["restoredHardLimit"] = hard_limit(-1)
            require(audit["restoredHardLimit"] == saved_limit, "Prior hard limit was not restored.")
            saved_limit = None
        case["failure"]["finalizeCode"] = finish(demand)
        failure = case["failure"]
        require(failure["stepCode"] == allocation["expectedFailure"] and failure["error"]["errorCode"] == 7
                and failure["error"]["extendedErrorCode"] == 7 and failure["finalizeCode"] == 7, "Actual step did not activate controlled NOMEM.")
        recover_transaction(case)
        case["unchanged"] = query(allocation["verify"])
        require(case["unchanged"]["rows"] == expected_rows(allocation["expectedBeforeRecovery"]), "NOMEM changed committed rows.")
        case["recoveryWrite"] = query(allocation["recovery"]); case["recovered"] = query(allocation["verify"])
        require(case["recovered"]["rows"] == expected_rows(allocation["expectedAfterRecovery"]), "NOMEM recovery write failed.")
        case["closeCode"] = closed()
        io = work["io"]; filename = artifacts / io["fileName"]
        require(filename.name == "io-failure.db" and filename.parent == artifacts, "I/O file escaped fresh artifacts.")
        case = {"name": "injected-pager-commit-ioerr", "setup": [], "openCode": opened(filename, 6)}
        report["cases"].append(case)
        for name in ("journalMode", "schema", "seed", "begin", "tentative"): case["setup"].append(query(io[name]))
        require(case["setup"][0]["rows"] == [[{"type": "text-utf8", "bytes": 6, "hex": "64656c657465"}]], "Genuine DELETE journal mode was not selected.")
        commit = prepared(io["commit"])
        callback_state = {"probe0Calls": 0, "activated400": 0, "callbackFailed": False}
        callback_audit = {}
        report["faultCallbackAudit"] = callback_audit

        @c.CFUNCTYPE(c.c_int, c.c_int)
        def fault(code):
            try:
                callback_audit[str(code)] = callback_audit.get(str(code), 0) + 1
                if code == 0: callback_state["probe0Calls"] += 1
                if code == io["faultSite"] and armed[0] and callback_state["activated400"] == 0:
                    armed[0] = False; callback_state["activated400"] += 1; return 1
                return 0
            except BaseException:
                callback_state["callbackFailed"] = True
                return 0

        try:
            hook_installed = True
            case["installCode"] = test_control(io["faultInstallOperation"], c.cast(fault, pointer))
            require(case["installCode"] == 0 and callback_state["probe0Calls"] == 1, "Unarmed install probe0 failed.")
            armed[0] = True
            case["failure"] = {"sql": io["commit"], **read(commit)}
        finally:
            armed[0] = False
            case["uninstallCode"] = test_control(io["faultInstallOperation"], pointer())
            hook_installed = False
        case["failure"]["finalizeCode"] = finish(commit)
        case["callback"] = callback_state.copy()
        failure = case["failure"]
        require(failure["stepCode"] == io["expectedFailure"] and failure["error"]["errorCode"] == 10
                and failure["error"]["extendedErrorCode"] == 10 and failure["finalizeCode"] == 10
                and case["uninstallCode"] == 0 and callback_state["activated400"] == 1 and not callback_state["callbackFailed"], "Actual pager IOERR activation/uninstall failed.")
        recover_transaction(case)
        case["unchanged"] = query(io["verify"])
        require(case["unchanged"]["rows"] == expected_rows(io["expectedBeforeRecovery"]), "Failed pager commit changed rows.")
        case["recovery"] = [query(io[name]) for name in ("recoveryBegin", "recoveryUpdate", "recoveryCommit")]
        case["recovered"] = query(io["verify"])
        require(case["recovered"]["rows"] == expected_rows(io["expectedAfterRecovery"]), "Pager failure recovery transaction failed.")
        case["closeCode"] = closed(); case["reopenCode"] = opened(filename, 2)
        case["persisted"] = query(io["verify"]); case["integrity"] = query("PRAGMA integrity_check")
        require(case["persisted"]["rows"] == expected_rows(io["expectedAfterRecovery"])
                and case["integrity"]["rows"] == [[{"type": "text-utf8", "bytes": 2, "hex": "6f6b"}]], "Close/reopen persistence/integrity failed.")
        case["finalCloseCode"] = closed()
        require(not any(Path(str(filename) + suffix).exists() for suffix in ("-journal", "-wal", "-shm")), "Failure artifacts retain sidecars after normal close.")
        report["database"] = receipt(filename)
        loaded = sorted({parts[-1] for line in Path("/proc/self/maps").read_text().splitlines()
                         if len(parts := line.split()) >= 6 and parts[-1].startswith("/") and ".so" in parts[-1]})
        require([path for path in loaded if "sqlite" in Path(path).name.lower()] == [str(library)], "Unexpected native SQLite provider.")
        report["loadedLibraries"] = [receipt(Path(path)) for path in loaded]
        report["status"] = "passed-native-controlled-failures"
    except Exception as problem:
        failed = str(problem); report["error"] = failed
    finally:
        cleanup = []
        armed[0] = False
        if hook_installed and test_control:
            cleanup.append({"uninstallCode": test_control(9, c.c_void_p())}); hook_installed = False
        if saved_limit is not None and hard_limit:
            cleanup.append({"restoreHardLimitReturned": hard_limit(saved_limit), "restoredHardLimit": hard_limit(-1)})
        if finalize:
            for key in list(active):
                statement = active.pop(key); cleanup.append({"finalizeCode": finalize(statement)})
        if database and close:
            code = close(database); cleanup.append({"closeCode": code})
            if code == 0: database = c.c_void_p()
        if cleanup: report["cleanup"] = cleanup
        report["connectionsClosed"] = not database
        report["faultHookUninstalled"] = not hook_installed
        report["pinnedFilesUnchanged"] = all(path.is_file() and digest(path) == sha for path, sha in frozen.items())
        if not report["pinnedFilesUnchanged"]: report["status"] = "incomplete"
        (artifacts / "failures-report.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "cases": len(report["cases"])}))
    if failed: print(failed, file=sys.stderr)
    return 0 if report["status"] == "passed-native-controlled-failures" else 1


if __name__ == "__main__":
    sys.exit(main())

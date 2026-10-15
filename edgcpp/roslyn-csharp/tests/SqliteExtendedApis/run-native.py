#!/usr/bin/env python3
"""Separate pinned native oracle for the finite carray/normalize/metadata/error slice."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct
import sys

WORKLOAD_SHA256 = "1ad62ff2001a6630e52f0fb7a9fa730209574e9c03ec6b3f219e56db5bdb72e7"
BUILD_SHA256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435"
LIBRARY_SHA256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"
INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "bytes": path.stat().st_size, "sha256": digest(path)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--workload", type=Path, default=Path(__file__).with_name("workload.json"))
    parser.add_argument("--build-receipt", type=Path, default=Path(__file__).resolve().parents[2] / "acceptance/sqlite/native-oracle-build.json")
    args = parser.parse_args()
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    report = {"status": "incomplete", "role": "separate-native-test-oracle-only", "generatedExecution": False,
              "nativeApplicationFallbackPermitted": False, "cases": [], "connectionsClosed": False}
    database = c.c_void_p()
    active: dict[int, c.c_void_p] = {}
    owners = []
    frozen: dict[Path, str] = {}
    finalize = close = None
    failed = None
    try:
        if any(os.environ.get(name) for name in ("LD_PRELOAD", "LD_AUDIT", "LD_LIBRARY_PATH")):
            raise RuntimeError("Use an explicit native oracle loader environment without overrides.")
        if digest(args.build_receipt) != BUILD_SHA256 or digest(args.workload) != WORKLOAD_SHA256:
            raise RuntimeError("Pinned native build receipt or workload changed.")
        build, work = json.loads(args.build_receipt.read_text()), json.loads(args.workload.read_text())
        library = Path(build["library"]["path"]).resolve(strict=True)
        if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != INPUT_SHA256
                or build["applicationNativeFallbackPermitted"] is not False or digest(library) != LIBRARY_SHA256
                or digest(library) != build["library"]["sha256"] or library.stat().st_size != build["library"]["bytes"]
                or work["workloadVersion"] != 1):
            raise RuntimeError("The native oracle identity/configuration differs from the pinned test role.")
        report.update(library=receipt(library), buildReceipt=receipt(args.build_receipt), workload=receipt(args.workload),
                      runner=receipt(Path(__file__)), remainingT4Areas=work["remainingT4Areas"])
        frozen = {args.build_receipt: BUILD_SHA256, args.workload: WORKLOAD_SHA256, library: LIBRARY_SHA256,
                  Path(__file__): report["runner"]["sha256"]}
        api = c.CDLL(str(library), mode=os.RTLD_NOW | os.RTLD_LOCAL)
        pointer = c.c_void_p
        symbols = []

        def bind(name, result, *arguments):
            value = getattr(api, "sqlite3_" + name)
            value.restype, value.argtypes = result, list(arguments)
            symbols.append("sqlite3_" + name)
            return value

        version = bind("libversion", c.c_char_p)
        version_number = bind("libversion_number", c.c_int)
        open_db = bind("open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
        close = bind("close", c.c_int, pointer)
        prepare = bind("prepare_v2", c.c_int, pointer, pointer, c.c_int, c.POINTER(pointer), c.POINTER(pointer))
        step = bind("step", c.c_int, pointer)
        finalize = bind("finalize", c.c_int, pointer)
        reset = bind("reset", c.c_int, pointer)
        clear = bind("clear_bindings", c.c_int, pointer)
        bind_int = bind("bind_int64", c.c_int, pointer, c.c_int, c.c_int64)
        carray = bind("carray_bind", c.c_int, pointer, c.c_int, pointer, c.c_int, c.c_int, pointer)
        carray_v2 = bind("carray_bind_v2", c.c_int, pointer, c.c_int, pointer, c.c_int, c.c_int, pointer, pointer)
        normalized = bind("normalized_sql", pointer, pointer)
        metadata = bind("table_column_metadata", c.c_int, pointer, c.c_char_p, c.c_char_p, c.c_char_p,
                        c.POINTER(pointer), c.POINTER(pointer), c.POINTER(c.c_int), c.POINTER(c.c_int), c.POINTER(c.c_int))
        errcode = bind("errcode", c.c_int, pointer)
        extended = bind("extended_errcode", c.c_int, pointer)
        error_text = bind("errmsg", c.c_char_p, pointer)
        error_mode = bind("extended_result_codes", c.c_int, pointer, c.c_int)
        column_count = bind("column_count", c.c_int, pointer)
        column_name = bind("column_name", c.c_char_p, pointer, c.c_int)
        column_type = bind("column_type", c.c_int, pointer, c.c_int)
        column_int = bind("column_int64", c.c_int64, pointer, c.c_int)
        column_double = bind("column_double", c.c_double, pointer, c.c_int)
        column_text = bind("column_text", pointer, pointer, c.c_int)
        column_blob = bind("column_blob", pointer, pointer, c.c_int)
        column_bytes = bind("column_bytes", c.c_int, pointer, c.c_int)
        report["apiSymbols"] = symbols

        def require(condition, message):
            if not condition:
                raise RuntimeError(message)

        def text_value(value: bytes) -> dict:
            return {"type": "text-utf8", "bytes": len(value), "hex": value.hex()}

        def errors() -> dict:
            # All three documented preserving APIs; copy the message now.
            return {"errorCode": errcode(database), "extendedErrorCode": extended(database),
                    "message": error_text(database).decode("utf-8")}

        def prepared(sql):
            encoded = sql.encode("utf-8")
            owner = c.create_string_buffer(encoded)
            statement, tail = pointer(), pointer()
            code = prepare(database, c.cast(owner, pointer), len(encoded) + 1, c.byref(statement), c.byref(tail))
            if statement:
                active[statement.value] = statement
            require(code == 0, "Preparation failed: " + sql)
            start = c.addressof(owner)
            require(bool(statement) and tail.value is not None and start <= tail.value <= start + len(encoded), "Invalid SQL tail.")
            require(not encoded[tail.value - start:].strip(b" \t\r\n\v\f"), "Each action must prepare exactly one statement.")
            return statement

        def finish(statement):
            active.pop(statement.value)
            return finalize(statement)

        def value(statement, index):
            kind = column_type(statement, index)
            if kind == 1: return {"type": "integer", "decimal": str(column_int(statement, index))}
            if kind == 2: return {"type": "real", "binary64Hex": struct.pack(">d", column_double(statement, index)).hex()}
            if kind in (3, 4):
                address = (column_text if kind == 3 else column_blob)(statement, index)
                count = column_bytes(statement, index)
                require(0 <= count <= 4096 and (count == 0 or address), "Invalid borrowed column extent.")
                return {"type": "text-utf8" if kind == 3 else "blob", "bytes": count,
                        "hex": c.string_at(address, count).hex() if count else ""}
            require(kind == 5, "Unknown column storage class.")
            return {"type": "null"}

        def read(statement):
            count = column_count(statement)
            require(0 <= count <= 8, "Column budget exceeded.")
            observed = {"columns": [column_name(statement, i).decode("utf-8") for i in range(count)], "rows": [], "stepCode": None}
            while True:
                code = step(statement)
                if code != 100:
                    observed["stepCode"] = code
                    if code != 101: observed["error"] = errors()
                    return observed
                observed["rows"].append([value(statement, i) for i in range(count)])
                require(len(observed["rows"]) <= 16, "Row budget exceeded.")

        def query(sql):
            statement = prepared(sql)
            observed = {"sql": sql, **read(statement)}
            observed["finalizeCode"] = finish(statement)
            return observed

        def integer_rows(values):
            return [[{"type": "integer", "decimal": str(n)}] for n in values]

        report["sqliteVersion"], report["sqliteVersionNumber"] = version().decode("ascii"), version_number()
        require(report["sqliteVersionNumber"] == work["sqliteVersionNumber"], "Expected pinned SQLite3.54.0.")
        report["openCode"] = open_db(b":memory:", c.byref(database), 6, None)
        require(report["openCode"] == 0 and database, "Open failed.")
        arrays = {"name": "carray-values-ownership", "observations": []}
        report["cases"].append(arrays)
        for label, field, scalar, flags, destroy in (("static", "staticInt32", c.c_int32, 0, None),
                                                    ("transient", "transientInt64", c.c_int64, 1, pointer(-1))):
            data = (scalar * len(work[field]))(*work[field]); owners.append(data)
            statement = prepared(work["carraySql"])
            observed = {"label": label, "bindCode": carray(statement, 1, c.cast(data, pointer), len(data), flags, destroy)}
            arrays["observations"].append(observed)
            require(observed["bindCode"] == 0, "carray bind failed.")
            if label == "transient":
                for i, n in enumerate(work["transientMutation"]): data[i] = n
            observed.update(read(statement)); observed["finalizeCode"] = finish(statement)
            require(observed["rows"] == integer_rows(work[field]) and observed["stepCode"] == 101 and observed["finalizeCode"] == 0, "carray values/copy changed.")
        contexts = [c.c_int(i) for i in (1, 2, 3)]
        context_ids = {c.addressof(item): i for i, item in enumerate(contexts, 1)}
        events, callback_error = [], [False]

        @c.CFUNCTYPE(None, pointer)
        def destroyed(context):
            try:
                identifier = context_ids.get(context)
                if identifier is None or c.c_int.from_address(context).value != identifier:
                    callback_error[0] = True
                else: events.append(identifier)
            except BaseException:
                callback_error[0] = True

        callback = c.cast(destroyed, pointer)
        statement = prepared(work["carraySql"])
        for sequence, field in ((1, "ownedFirst"), (2, "ownedSecond")):
            data = (c.c_int32 * len(work[field]))(*work[field]); owners.append(data)
            observed = {"label": "owned-" + str(sequence), "bindCode": carray_v2(statement, 1, c.cast(data, pointer), len(data), 0, callback, c.addressof(contexts[sequence - 1]))}
            arrays["observations"].append(observed)
            require(observed["bindCode"] == 0 and c.addressof(data) not in context_ids, "Owned bind/context failed.")
            observed.update(read(statement)); observed["callbacksAfterRead"] = len(events)
            require(observed["rows"] == integer_rows(work[field]) and observed["stepCode"] == 101 and len(events) == sequence - 1, "Owned values/lifetime changed.")
            if sequence == 1:
                observed["resetCode"] = reset(statement); observed["callbacksAfterReset"] = len(events)
                observed["clearCode"] = clear(statement); observed["callbacksAfterClear"] = len(events)
                require(observed["resetCode"] == 0 and observed["callbacksAfterReset"] == 0 and observed["clearCode"] == 0 and len(events) == 1, "reset/clear ownership changed.")
            else:
                observed["finalizeCode"] = finish(statement); observed["callbacksAfterFinalize"] = len(events)
                require(observed["finalizeCode"] == 0 and len(events) == 2, "Finalization ownership changed.")
        statement = prepared(work["carraySql"])
        data = (c.c_int32 * 1)(99); owners.append(data)
        bad = {"label": "owned-bind-failure", "bindCode": carray_v2(statement, 2, c.cast(data, pointer), 1, 0, callback, c.addressof(contexts[2]))}
        arrays["observations"].append(bad)
        bad["callbacksAfterBind"] = len(events); bad["finalizeCode"] = finish(statement)
        require(bad["bindCode"] == 25 and bad["callbacksAfterBind"] == 3 and bad["finalizeCode"] == 0, "Failed-bind ownership changed.")
        arrays.update(callbackEvents=events, callbackFailed=callback_error[0], callbackContextDistinctFromData=True)
        require(events == work["callbackExpectedEvents"] and not callback_error[0], "Callback protocol failed.")
        normal = {"name": "normalized-statement-execution", "observations": []}; report["cases"].append(normal)
        for index, sql in enumerate(work["normalizedSql"]):
            statement = prepared(sql)
            address = normalized(statement)
            require(bool(address), "Normalized SQL missing despite configured feature.")
            observed = {"sql": sql, "normalized": text_value(c.string_at(address)), "bindCode": bind_int(statement, 1, 9)}
            normal["observations"].append(observed)
            observed.update(read(statement)); observed["finalizeCode"] = finish(statement)
            require(observed["bindCode"] == 0 and observed["rows"] == work["normalizedExpectedRows"][index] and observed["stepCode"] == 101 and observed["finalizeCode"] == 0, "Normalized statement execution changed.")
        require(normal["observations"][0]["normalized"] == normal["observations"][1]["normalized"], "Literal-only normalization did not agree.")
        meta = {"name": "column-metadata", "schema": query(work["schemaSql"]), "columns": []}; report["cases"].append(meta)
        require(meta["schema"]["stepCode"] == 101 and meta["schema"]["finalizeCode"] == 0, "Schema creation failed.")
        for expected in work["metadata"]:
            datatype, collation, not_null, primary, increment = pointer(), pointer(), c.c_int(), c.c_int(), c.c_int()
            code = metadata(database, b"main", b"meta", expected["column"].encode(), c.byref(datatype), c.byref(collation), c.byref(not_null), c.byref(primary), c.byref(increment))
            observed = {"column": expected["column"], "code": code}
            meta["columns"].append(observed)
            require(code == 0, "Expected metadata column missing.")
            # Copy both borrowed strings before the next SQLite call.
            observed.update(declaredType=c.string_at(datatype).decode("utf-8"), collation=c.string_at(collation).decode("utf-8"),
                            notNull=not_null.value, primaryKey=primary.value, autoIncrement=increment.value)
            require(observed == expected, "Captured metadata differs from the finite schema expectation.")
        meta["missingCode"] = metadata(database, b"main", b"meta", work["missingColumn"].encode(), None, None, None, None, None)
        require(meta["missingCode"] == work["expectedMissingMetadataCode"], "Missing-column error changed.")
        errors_case = {"name": "constraint-errors-and-recovery", "modes": [], "observations": []}; report["cases"].append(errors_case)
        for enabled in (0,):
            code = error_mode(database, enabled); errors_case["modes"].append({"enabled": enabled, "code": code}); require(code == 0, "Error mode failed.")
        for action in work["errorSql"]:
            if action["label"] in ("notnull-extended", "transaction-begin"):
                enabled = int(action["label"] == "notnull-extended")
                code = error_mode(database, enabled); errors_case["modes"].append({"enabled": enabled, "code": code}); require(code == 0, "Error mode failed.")
            observed = {"label": action["label"], **query(action["sql"])}; errors_case["observations"].append(observed)
            require(observed["stepCode"] == action["stepCode"] and observed["finalizeCode"] == action["finalizeCode"], "Constraint step/finalize code changed.")
            if "errorCode" in action:
                require(observed["error"]["errorCode"] == action["errorCode"] and observed["error"]["extendedErrorCode"] == action["extendedErrorCode"], "Immediate extended-error observation changed.")
        errors_case["recovery"] = query(work["recoverySql"])
        require(errors_case["recovery"]["rows"] == work["recoveryExpectedRows"] and errors_case["recovery"]["stepCode"] == 101 and errors_case["recovery"]["finalizeCode"] == 0, "Recovery rows changed.")
        report["closeCode"] = close(database)
        require(report["closeCode"] == 0, "Close failed.")
        database = pointer()
        loaded = sorted({parts[-1] for line in Path("/proc/self/maps").read_text().splitlines()
                         if len(parts := line.split()) >= 6 and parts[-1].startswith("/") and ".so" in parts[-1]})
        require([path for path in loaded if "sqlite" in Path(path).name.lower()] == [str(library)], "Unexpected native SQLite provider.")
        report["loadedLibraries"] = [receipt(Path(path)) for path in loaded]
        report["status"] = "passed-native-extended-apis"
    except Exception as problem:
        failed = str(problem); report["error"] = failed
    finally:
        cleanup = []
        if finalize:
            for statement in list(active.values()): cleanup.append({"finalizeCode": finalize(statement)})
            active.clear()
        if database and close:
            code = close(database); cleanup.append({"closeCode": code})
            if code == 0: database = c.c_void_p()
        if cleanup: report["cleanup"] = cleanup
        report["connectionsClosed"] = not database
        report["pinnedFilesUnchanged"] = all(path.is_file() and digest(path) == sha for path, sha in frozen.items())
        if not report["pinnedFilesUnchanged"]: report["status"] = "incomplete"
        (artifacts / "extended-api-report.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "cases": len(report["cases"])}))
    if failed: print(failed, file=sys.stderr)
    return 0 if report["status"] == "passed-native-extended-apis" else 1


if __name__ == "__main__":
    sys.exit(main())

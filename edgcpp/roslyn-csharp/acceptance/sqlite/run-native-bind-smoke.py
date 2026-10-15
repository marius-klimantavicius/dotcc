#!/usr/bin/env python3
"""Observe bind/reset/clear only in the pinned, separate native oracle process."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct
import sys

INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
SQL = "SELECT ?1 AS n,?2 AS r,?3 AS t,?4 AS b,?5 AS missing"


def receipt(path: Path) -> dict:
    with path.open("rb") as stream:
        digest = hashlib.file_digest(stream, "sha256").hexdigest()
    return {"path": str(path.resolve()), "bytes": path.stat().st_size, "sha256": digest}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-receipt", type=Path, required=True)
    parser.add_argument("--workload", type=Path, default=Path(__file__).with_name("sql-smoke.json"))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists(): parser.error("Choose a fresh output receipt path.")
    build = json.loads(args.build_receipt.read_text())
    library = Path(build["library"]["path"]).resolve()
    actual = receipt(library)
    if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != INPUT_SHA256
        or actual["sha256"] != build["library"]["sha256"] or actual["bytes"] != build["library"]["bytes"]):
        raise RuntimeError("Native oracle role or exact pinned library/input identity changed.")
    workload = json.loads(args.workload.read_text())
    if workload["workloadVersion"] != 1: raise RuntimeError("Unsupported workload version.")
    report = {"status": "incomplete", "role": "separate-native-test-oracle-only", "mode": "bind-smoke",
        "generatedExecution": False, "nativeApplicationFallbackPermitted": False,
        "library": actual, "buildReceipt": receipt(args.build_receipt), "workload": receipt(args.workload),
        "runner": receipt(Path(__file__)), "cases": [], "remainingT4Areas": workload["remainingT4Areas"]}
    # Only the audited native oracle is loaded, and only by this separate process.
    api = c.CDLL(str(library), mode=os.RTLD_NOW | os.RTLD_LOCAL)
    pointer = c.c_void_p

    def bind(name: str, result, *arguments):
        function = getattr(api, "sqlite3_" + name)
        function.restype, function.argtypes = result, list(arguments)
        return function

    version = bind("libversion", c.c_char_p)
    version_number = bind("libversion_number", c.c_int)
    open_db = bind("open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
    close_db = bind("close", c.c_int, pointer)
    error = bind("errmsg", c.c_char_p, pointer)
    prepare = bind("prepare_v2", c.c_int, pointer, pointer, c.c_int, c.POINTER(pointer), c.POINTER(pointer))
    step = bind("step", c.c_int, pointer)
    finalize = bind("finalize", c.c_int, pointer)
    reset = bind("reset", c.c_int, pointer)
    clear = bind("clear_bindings", c.c_int, pointer)
    bind_integer = bind("bind_int64", c.c_int, pointer, c.c_int, c.c_int64)
    bind_real = bind("bind_double", c.c_int, pointer, c.c_int, c.c_double)
    bind_text = bind("bind_text", c.c_int, pointer, c.c_int, pointer, c.c_int, pointer)
    bind_blob = bind("bind_blob", c.c_int, pointer, c.c_int, pointer, c.c_int, pointer)
    bind_null = bind("bind_null", c.c_int, pointer, c.c_int)
    column_count = bind("column_count", c.c_int, pointer)
    column_name = bind("column_name", c.c_char_p, pointer, c.c_int)
    column_type = bind("column_type", c.c_int, pointer, c.c_int)
    column_integer = bind("column_int64", c.c_int64, pointer, c.c_int)
    column_real = bind("column_double", c.c_double, pointer, c.c_int)
    column_text = bind("column_text", pointer, pointer, c.c_int)
    column_blob = bind("column_blob", pointer, pointer, c.c_int)
    column_bytes = bind("column_bytes", c.c_int, pointer, c.c_int)
    database, statement = pointer(), pointer()
    owners = []  # SQLITE_STATIC byte buffers remain owned until after finalize.

    def check(code: int, operation: str):
        if code != 0:
            message = error(database).decode("utf-8", errors="replace") if database else "no database handle"
            raise RuntimeError(f"{operation}: SQLite {code}: {message}")

    def bytes_binding(ordinal: int, value: bytes, text: bool):
        owner = c.create_string_buffer(value, len(value) + 1)
        owners.append(owner)  # Empty BLOB uses a non-NULL address and a zero extent.
        check((bind_text if text else bind_blob)(statement, ordinal, c.cast(owner, pointer), len(value), None), "bind bytes")

    def value(index: int) -> dict:
        kind = column_type(statement, index)
        if kind == 1: return {"type": "integer", "decimal": str(column_integer(statement, index))}
        if kind == 2: return {"type": "real", "binary64Hex": struct.pack(">d", column_real(statement, index)).hex()}
        if kind in (3, 4):
            address = (column_text if kind == 3 else column_blob)(statement, index)
            count = column_bytes(statement, index)
            if count < 0 or count > 64 * 1024 * 1024 or count and not address:
                raise RuntimeError("Invalid or oversized borrowed column extent.")
            return {"type": "text-utf8" if kind == 3 else "blob", "bytes": count,
                "hex": c.string_at(address, count).hex() if count else ""}
        if kind == 5: return {"type": "null"}
        raise RuntimeError("Unexpected storage class: " + str(kind))

    def read() -> dict:
        count = column_count(statement)
        if count != 5: raise RuntimeError("The fixed bind SELECT changed its five columns.")
        observed = {"sql": SQL, "columns": [column_name(statement, index).decode("utf-8") for index in range(count)], "rows": []}
        while True:
            code = step(statement)
            if code == 101: return observed
            if code != 100:
                check(code, "step")
                raise RuntimeError("step returned neither ROW nor DONE.")
            observed["rows"].append([value(index) for index in range(count)])
            if len(observed["rows"]) > 10000: raise RuntimeError("Fixed row budget exceeded.")

    try:
        report["sqliteVersion"], report["sqliteVersionNumber"] = version().decode("ascii"), version_number()
        if report["sqliteVersionNumber"] != 3054000: raise RuntimeError("Expected pinned SQLite 3.54.0.")
        check(open_db(b":memory:", c.byref(database), 2 | 4, None), "open")
        encoded = c.create_string_buffer(SQL.encode("utf-8"))
        tail = pointer()
        check(prepare(database, c.cast(encoded, pointer), len(encoded), c.byref(statement), c.byref(tail)), "prepare")
        if not statement or tail.value != c.addressof(encoded) + len(SQL.encode("utf-8")):
            raise RuntimeError("Preparation did not consume exactly the one fixed SELECT.")
        check(bind_integer(statement, 1, -9223372036854775807), "bind int64")
        check(bind_real(statement, 2, 3.25), "bind double")
        bytes_binding(3, b"a\0z", True)
        bytes_binding(4, bytes([0, 1, 254, 255]), False)
        check(bind_null(statement, 5), "bind null")
        first = read()
        expected = [{"type": "integer", "decimal": "-9223372036854775807"}, {"type": "real", "binary64Hex": "400a000000000000"},
            {"type": "text-utf8", "bytes": 3, "hex": "61007a"}, {"type": "blob", "bytes": 4, "hex": "0001feff"}, {"type": "null"}]
        if first["rows"] != [expected]: raise RuntimeError("Native bound values differ from the frozen expectation.")
        check(reset(statement), "reset")
        check(clear(statement), "clear bindings")
        cleared = read()
        if cleared["rows"] != [[{"type": "null"}] * 5]: raise RuntimeError("Native clear_bindings did not clear all values.")
        check(reset(statement), "reset")
        check(clear(statement), "clear bindings")
        check(bind_integer(statement, 1, 42), "bind int64")
        check(bind_real(statement, 2, -0.5), "bind double")
        bytes_binding(3, b"", True)
        bytes_binding(4, b"", False)
        check(bind_null(statement, 5), "bind null")
        empty = read()
        expected_empty = [{"type": "integer", "decimal": "42"}, {"type": "real", "binary64Hex": "bfe0000000000000"},
            {"type": "text-utf8", "bytes": 0, "hex": ""}, {"type": "blob", "bytes": 0, "hex": ""}, {"type": "null"}]
        if empty["rows"] != [expected_empty]: raise RuntimeError("Native empty text/BLOB binding changed storage class.")
        report["cases"] = [{"name": "bound-values-reset-clear", "statements": [first, cleared, empty]}]
        final_code = finalize(statement)
        statement = pointer()
        owners.clear()
        check(final_code, "finalize")
        close_code = close_db(database)
        check(close_code, "close")
        database = pointer()
        loaded = sorted({parts[-1] for line in Path("/proc/self/maps").read_text().splitlines()
            if len(parts := line.split()) >= 6 and parts[-1].startswith("/") and ".so" in parts[-1]})
        if [path for path in loaded if "sqlite" in Path(path).name.lower()] != [str(library)]:
            raise RuntimeError("The native oracle process loaded a second SQLite provider.")
        report["loadedLibraries"] = [receipt(Path(path)) for path in loaded]
        report["status"] = "passed-native-oracle-only"
    except Exception as problem:
        report["error"] = str(problem)
        raise
    finally:
        # Release SQLITE_STATIC owners only after the native statement is gone.
        if statement: finalize(statement)
        owners.clear()
        if database: close_db(database)
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "cases": 1, "statements": 3}))
    return 0


if __name__ == "__main__":
    sys.exit(main())

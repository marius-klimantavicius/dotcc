#!/usr/bin/env python3
"""Run only the pinned native reference in its own process; never a runtime provider."""
from __future__ import annotations

import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct
import sys


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "bytes": path.stat().st_size,
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-receipt", type=Path, required=True)
    parser.add_argument("--workload", type=Path, default=Path(__file__).with_name("sql-smoke.json"))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists():
        parser.error("Output already exists; choose an unused receipt path.")
    build = json.loads(args.build_receipt.read_text())
    library = Path(build["library"]["path"]).resolve()
    actual_library = receipt(library)
    if build["role"] != "separate-native-test-oracle-only" or actual_library["sha256"] != build["library"]["sha256"]:
        parser.error("Native oracle identity/role does not match its build receipt.")
    workload = json.loads(args.workload.read_text())
    if workload["workloadVersion"] != 1:
        parser.error("Unsupported workload version.")
    report = {"status": "incomplete", "role": "separate-native-test-oracle-only",
              "generatedExecution": False, "nativeApplicationFallbackPermitted": False,
              "library": actual_library, "buildReceipt": receipt(args.build_receipt),
              "workload": receipt(args.workload), "runner": receipt(Path(__file__)),
              "cases": [], "remainingT4Areas": workload["remainingT4Areas"]}
    # No Python sqlite3 import or system-library lookup: open the exact audited path.
    api = c.CDLL(str(library), mode=os.RTLD_LOCAL | os.RTLD_NOW)
    pointer = c.c_void_p
    def bind(name: str, result, *arguments):
        function = getattr(api, "sqlite3_" + name)
        function.restype = result
        function.argtypes = list(arguments)
        return function
    version = bind("libversion", c.c_char_p)
    version_number = bind("libversion_number", c.c_int)
    open_db = bind("open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
    close_db = bind("close", c.c_int, pointer)
    error = bind("errmsg", c.c_char_p, pointer)
    prepare = bind("prepare_v2", c.c_int, pointer, c.c_char_p, c.c_int, c.POINTER(pointer), c.POINTER(c.c_char_p))
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
    report["sqliteVersion"] = version().decode("ascii")
    report["sqliteVersionNumber"] = version_number()
    if report["sqliteVersionNumber"] != 3054000:
        raise RuntimeError("Pinned SQLite 3.54.0 was not loaded.")
    def check(code: int, database: pointer, operation: str):
        if code != 0:
            raise RuntimeError(f"{operation}: SQLite {code}: {error(database).decode('utf-8', errors='replace')}")
    def value(statement: pointer, index: int) -> dict:
        kind = column_type(statement, index)
        if kind == 1:
            return {"type": "integer", "decimal": str(column_integer(statement, index))}
        if kind == 2:
            return {"type": "real", "binary64Hex": struct.pack(">d", column_real(statement, index)).hex()}
        if kind in (3, 4):
            address = (column_text if kind == 3 else column_blob)(statement, index)
            count = column_bytes(statement, index)
            if count and not address:
                raise RuntimeError("Nonempty column has no storage.")
            return {"type": "text-utf8" if kind == 3 else "blob", "bytes": count,
                    "hex": c.string_at(address, count).hex() if count else ""}
        if kind == 5:
            return {"type": "null"}
        raise RuntimeError(f"Unknown SQLite storage class {kind}.")
    try:
        for case in workload["cases"]:
            database = pointer()
            observations = {"name": case["name"], "statements": []}
            report["cases"].append(observations)
            try:
                check(open_db(b":memory:", c.byref(database), 2 | 4, None), database, "open")
                for sql in case["statements"]:
                    statement, tail = pointer(), c.c_char_p()
                    encoded = sql.encode("utf-8")
                    check(prepare(database, encoded, len(encoded) + 1, c.byref(statement), c.byref(tail)), database, "prepare: " + sql)
                    try:
                        if not statement or tail.value and tail.value.strip():
                            raise RuntimeError("Each workload entry must contain exactly one SQL statement.")
                        count = column_count(statement)
                        observed = {"sql": sql, "columns": [column_name(statement, i).decode("utf-8") for i in range(count)], "rows": []}
                        observations["statements"].append(observed)
                        while True:
                            code = step(statement)
                            if code == 101:
                                break
                            if code != 100:
                                check(code, database, "step: " + sql)
                            observed["rows"].append([value(statement, i) for i in range(count)])
                            if len(observed["rows"]) > 10000:
                                raise RuntimeError("Frozen per-statement row budget exceeded.")
                    finally:
                        if statement:
                            final_code = finalize(statement)
                            if sys.exc_info()[0] is None:
                                check(final_code, database, "finalize")
            finally:
                if database:
                    check(close_db(database), database, "close")
        loaded = sorted({line.split()[-1] for line in Path("/proc/self/maps").read_text().splitlines()
                         if len(line.split()) >= 6 and line.split()[-1].startswith("/") and ".so" in line.split()[-1]})
        sqlite_libraries = [p for p in loaded if "sqlite" in Path(p).name.lower()]
        if sqlite_libraries != [str(library)]:
            raise RuntimeError(f"Unexpected SQLite library providers: {sqlite_libraries}")
        report["loadedLibraries"] = [receipt(Path(p)) for p in loaded]
        report["status"] = "passed-native-oracle-only"
    except Exception as problem:
        report["error"] = str(problem)
        raise
    finally:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "cases": len(report["cases"]),
                      "statements": sum(len(p["statements"]) for p in report["cases"])}))
    return 0


if __name__ == "__main__":
    sys.exit(main())

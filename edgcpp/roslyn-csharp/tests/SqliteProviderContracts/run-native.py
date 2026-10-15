#!/usr/bin/env python3
"""Pinned native oracle for three finite time/randomness/allocation contracts."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import math
import os
from pathlib import Path
import struct
import time

WORKLOAD_SHA256 = "33b6ea1f6d7dd02833c339f3d0e4a6612a75449b1c2be1b6e9b710e216f80502"
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
              "scope": "Three finite API contracts; selected provider exports are not invocation evidence."}
    frozen, calls = {}, {}
    database, statement = c.c_void_p(), c.c_void_p()
    allocation = None
    close = finalize = free = None
    failed = None
    try:
        if any(os.environ.get(key) for key in ("LD_PRELOAD", "LD_AUDIT", "LD_LIBRARY_PATH")):
            raise RuntimeError("Use an oracle loader environment without overrides.")
        if digest(args.workload) != WORKLOAD_SHA256 or digest(args.build_receipt) != BUILD_SHA256:
            raise RuntimeError("The frozen workload/native build receipt changed.")
        work, build = json.loads(args.workload.read_text()), json.loads(args.build_receipt.read_text())
        library = Path(build["library"]["path"]).resolve(strict=True)
        if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != INPUT_SHA256
                or build["applicationNativeFallbackPermitted"] is not False or digest(library) != LIBRARY_SHA256
                or build["library"]["sha256"] != LIBRARY_SHA256 or library.stat().st_size != build["library"]["bytes"]):
            raise RuntimeError("The exact pinned native oracle changed.")
        report.update(library=receipt(library), buildReceipt=receipt(args.build_receipt), workload=receipt(args.workload),
                      runner=receipt(Path(__file__)), remainingScope=work["remainingScope"])
        frozen = {args.workload: WORKLOAD_SHA256, args.build_receipt: BUILD_SHA256, library: LIBRARY_SHA256,
                  Path(__file__): report["runner"]["sha256"]}
        api = c.CDLL(str(library), mode=os.RTLD_NOW | os.RTLD_LOCAL)

        def bind(name, result, *arguments):
            function = getattr(api, name); function.restype, function.argtypes = result, list(arguments)
            def invoke(*values):
                calls[name] = calls.get(name, 0) + 1
                return function(*values)
            return invoke

        pointer = c.c_void_p
        version = bind("sqlite3_libversion_number", c.c_int)
        initialize = bind("sqlite3_initialize", c.c_int)
        open_db = bind("sqlite3_open_v2", c.c_int, c.c_char_p, c.POINTER(pointer), c.c_int, c.c_char_p)
        close = bind("sqlite3_close", c.c_int, pointer)
        prepare = bind("sqlite3_prepare_v2", c.c_int, pointer, pointer, c.c_int, c.POINTER(pointer), c.POINTER(pointer))
        step = bind("sqlite3_step", c.c_int, pointer)
        finalize = bind("sqlite3_finalize", c.c_int, pointer)
        column_count = bind("sqlite3_column_count", c.c_int, pointer)
        column_type = bind("sqlite3_column_type", c.c_int, pointer, c.c_int)
        column_integer = bind("sqlite3_column_int64", c.c_int64, pointer, c.c_int)
        column_double = bind("sqlite3_column_double", c.c_double, pointer, c.c_int)
        randomness = bind("sqlite3_randomness", None, c.c_int, pointer)
        malloc = bind("sqlite3_malloc64", pointer, c.c_uint64)
        realloc = bind("sqlite3_realloc64", pointer, pointer, c.c_uint64)
        msize = bind("sqlite3_msize", c.c_uint64, pointer)
        free = bind("sqlite3_free", None, pointer)

        def require(value, message):
            if not value: raise RuntimeError(message)

        report["sqliteVersionNumber"] = version()
        require(report["sqliteVersionNumber"] == work["sqliteVersionNumber"], "Unexpected pinned SQLite version.")
        report["initializationCode"] = initialize(); require(report["initializationCode"] == 0, "Initialization failed.")

        # Prepare before sampling: only the single ROW step is between the two
        # immediate host realtime samples. No ordering of those samples is assumed.
        clock = work["time"]
        opened = open_db(b":memory:", c.byref(database), 6, None)
        require(opened == 0 and database, "Time-query open failed.")
        encoded = clock["sql"].encode(); owner = c.create_string_buffer(encoded); tail = pointer()
        prepared = prepare(database, c.cast(owner, pointer), len(encoded) + 1, c.byref(statement), c.byref(tail))
        require(prepared == 0 and statement and tail.value == c.addressof(owner) + len(encoded), "Exact one-statement prepare failed.")
        before = time.time_ns()
        row_code = step(statement)
        after = time.time_ns()
        require(row_code == 100 and column_count(statement) == 2, "Expected exactly two time columns.")
        epoch_kind, julian_kind = column_type(statement, 0), column_type(statement, 1)
        epoch, julian = column_integer(statement, 0), column_double(statement, 1)
        tolerance = clock["toleranceSeconds"]
        low, high = min(before, after) / 1e9, max(before, after) / 1e9
        julian_epoch = (julian - clock["unixEpochJulianDay"]) * clock["secondsPerDay"]
        checks = {"unixepochInteger": epoch_kind == 1, "juliandayRealFinite": julian_kind == 2 and math.isfinite(julian),
                  "unixepochWithinHostWindow": math.floor(low - tolerance) <= epoch <= math.floor(high + tolerance),
                  "juliandayWithinHostWindow": math.isfinite(julian_epoch) and low - tolerance <= julian_epoch <= high + tolerance,
                  "statementTimeConsistent": math.isfinite(julian_epoch) and epoch - tolerance <= julian_epoch < epoch + 1 + tolerance}
        done = step(statement)
        owned = statement; statement = pointer(); finished = finalize(owned)
        closed = close(database)
        if closed == 0: database = pointer()
        case = {"name": "wall-clock-window", "openCode": opened, "prepareCode": prepared, "rowStepCode": row_code,
                "doneStepCode": done, "finalizeCode": finished, "closeCode": closed, "contracts": checks}
        report["cases"].append(case)
        report["timeAudit"] = {"hostBeforeUnixNanoseconds": str(before), "hostAfterUnixNanoseconds": str(after),
            "hostClockWentBackwards": after < before, "unixepochDecimal": str(epoch),
            "juliandayBinary64Hex": struct.pack(">d", julian).hex(),
            "juliandayAsUnixSecondsBinary64Hex": struct.pack(">d", julian_epoch).hex(),
            "toleranceSeconds": tolerance, "bounds": "minimum/maximum endpoint samples; no monotonicity assumption"}
        require(done == 101 and finished == 0 and closed == 0 and all(checks.values()), "Finite wall-clock contract failed.")

        random = work["randomness"]; count, guard = random["requestedBytes"], random["guardBytes"]
        require(count == 64 and guard == 16, "Unexpected guarded randomness extent.")
        guarded = (c.c_ubyte * (count + 2 * guard))()
        guarded[:guard] = [random["prefixByte"]] * guard
        guarded[guard:guard + count] = [random["initialInteriorByte"]] * count
        guarded[guard + count:] = [random["suffixByte"]] * guard
        randomness(count, c.c_void_p(c.addressof(guarded) + guard))
        observed = bytes(guarded[guard:guard + count])
        checks = {"callCompleted": True, "prefixGuardUnchanged": bytes(guarded[:guard]) == bytes([random["prefixByte"]]) * guard,
                  "suffixGuardUnchanged": bytes(guarded[guard + count:]) == bytes([random["suffixByte"]]) * guard,
                  "observedExtentMatchesRequest": len(observed) == count}
        report["cases"].append({"name": "guarded-randomness", "requestedBytes": count, "contracts": checks})
        report["randomnessAudit"] = {"bytes": len(observed), "hex": observed.hex(),
            "differentFromInitialInterior": observed != bytes([random["initialInteriorByte"]]) * count,
            "scope": "No output vector is forbidden; no statistical quality or security claim."}
        require(all(checks.values()), "Randomness buffer guard/extent contract failed.")

        memory = work["allocation"]; first, grown = memory["initialBytes"], memory["grownBytes"]
        require(first == 32 and grown == 96, "Unexpected allocation extents.")
        prefix = bytes((i * memory["prefixMultiplier"] + memory["prefixAddend"]) & 255 for i in range(first))
        tail_bytes = bytes((i * memory["tailMultiplier"] + memory["tailAddend"]) & 255 for i in range(grown - first))
        allocation = malloc(first); require(allocation, "malloc64 refused bounded allocation.")
        original_address = allocation; initial_size = msize(allocation)
        require(initial_size >= first, "Initial msize is below the requested extent.")
        c.memmove(allocation, prefix, first)
        initial_roundtrip = c.string_at(allocation, first) == prefix
        replacement = realloc(allocation, grown)
        require(replacement, "realloc64 refused bounded growth; original allocation retained for cleanup.")
        allocation = replacement  # Old pointer is invalid after successful growth.
        grown_size = msize(allocation)
        require(grown_size >= grown, "Grown msize is below the requested extent.")
        preserved = c.string_at(allocation, first) == prefix
        c.memmove(allocation + first, tail_bytes, grown - first)
        tail_roundtrip = c.string_at(allocation + first, grown - first) == tail_bytes
        address = allocation; allocation = None; free(address)  # Transfer ownership before free, exactly once.
        checks = {"allocated": True, "initialMsizeAtLeastRequested": initial_size >= first, "initialWriteReadEqual": initial_roundtrip,
                  "grownMsizeAtLeastRequested": grown_size >= grown, "prefixPreservedAfterGrowth": preserved,
                  "newTailWriteReadEqual": tail_roundtrip, "freedExactlyOnce": True}
        report["cases"].append({"name": "allocation-growth-preserves-bytes", "initialBytes": first, "grownBytes": grown, "contracts": checks})
        report["allocationAudit"] = {"initialAddressHex": format(original_address, "x"), "grownAddressHex": format(address, "x"),
            "addressChanged": original_address != address, "initialMsize": initial_size, "grownMsize": grown_size,
            "prefixHex": prefix.hex(), "newTailHex": tail_bytes.hex()}
        require(all(checks.values()), "Allocation preservation/size contract failed.")
        report["directApiInvocations"] = calls.copy()
        maps = sorted({parts[-1] for line in Path("/proc/self/maps").read_text().splitlines()
                       if len(parts := line.split()) >= 6 and parts[-1].startswith("/") and ".so" in parts[-1]})
        require(str(library) in maps and all("sqlite" not in Path(path).name.lower() or Path(path).resolve() == library for path in maps),
                "An unexpected SQLite application library was loaded.")
        report["loadedLibraries"] = [receipt(path) for path in maps]
        report["status"] = "passed-native-provider-contracts"
    except Exception as problem:
        failed = problem; report["error"] = repr(problem)
    finally:
        cleanup = []
        if allocation is not None and free is not None:
            owned = allocation; allocation = None; free(owned); cleanup.append({"allocationFreed": True})
        if statement and finalize is not None:
            owned = statement; statement = c.c_void_p(); cleanup.append({"finalizeCode": finalize(owned)})
        if database and close is not None:
            code = close(database); cleanup.append({"closeCode": code})
            if code == 0: database = c.c_void_p()
        if cleanup: report["cleanup"] = cleanup
        report["directApiInvocations"] = calls.copy()
        report["connectionsClosed"] = not bool(database)
        report["allocationReleased"] = allocation is None
        report["pinnedFilesUnchanged"] = all(Path(path).is_file() and digest(path) == expected for path, expected in frozen.items())
        if not report["connectionsClosed"] or not report["allocationReleased"] or not report["pinnedFilesUnchanged"]:
            report["status"] = "incomplete"
        (artifacts / "provider-contracts-report.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "cases": len(report["cases"])}))
    return 0 if failed is None and report["status"] == "passed-native-provider-contracts" else 1


if __name__ == "__main__":
    raise SystemExit(main())

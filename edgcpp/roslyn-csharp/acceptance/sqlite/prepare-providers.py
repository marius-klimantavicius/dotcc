#!/usr/bin/env python3
"""Resolve candidate host exports without enabling SQLite execution bindings."""
from __future__ import annotations

import argparse
import ctypes
import hashlib
import json
from pathlib import Path
import platform


ALIASES = {
    "__USER_LABEL_PREFIX__open64": "open64",
    "__USER_LABEL_PREFIX__stat64": "stat64",
    "__USER_LABEL_PREFIX__fstat64": "fstat64",
    "__USER_LABEL_PREFIX__ftruncate64": "ftruncate64",
    "__USER_LABEL_PREFIX__fcntl64": "fcntl64",
    "__USER_LABEL_PREFIX__pread64": "pread64",
    "__USER_LABEL_PREFIX__pwrite64": "pwrite64",
    "__USER_LABEL_PREFIX__mmap64": "mmap64",
    "__USER_LABEL_PREFIX__lstat64": "lstat64",
}


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def identity(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def contracts(symbol: str, group: str) -> dict:
    result = {
        "ownership": "Caller retains all passed storage; exact selected buffer extents and lifetime must be honored.",
        "errors": "Preserve the native result and native errno immediately in the provider adapter when relevant; a later managed __errno_location call is not a preservation proof.",
        "threads": "Execute on the calling native thread; no implicit serialization, retry or thread migration.",
        "callbacks": "No callback in this selected signature; exposed addresses still require a concrete ABI-correct entry.",
    }
    if group == "linux-libm":
        result.update(ownership="Scalar arguments and return have no transferred storage.",
            errors="Preserve exact binary64 result, native errno and required floating environment behavior; domain/range/NaN/signed-zero behavior needs differential evidence.")
    elif group == "linux-pthread":
        result.update(errors="Return the pthread error number directly; do not substitute errno.",
            threads="Use native pthread identity and actual provider mutex/attribute layout, constants, initialization and recursive behavior.")
        if symbol == "pthread_create":
            result.update(ownership="Caller owns pthread_t output and attributes; callback cookie remains valid until callback completion/join.",
                callbacks="Native foreign thread enters a concrete Cdecl managed callback; prove runtime attachment, per-thread state, nested reentry, completion and no managed exception crossing.")
        elif symbol == "pthread_join":
            result.update(ownership="Join consumes joinability; copy the returned callback pointer value to caller storage without changing its ownership.",
                callbacks="Wait for the native-created callback thread; preserve callback return pointer and completion ordering.")
    if symbol in {"malloc", "realloc", "free"}:
        result["ownership"] = "One native allocator domain: malloc transfers allocation to caller; realloc preserves old allocation on failure and invalidates it on success; free accepts only its live allocation or null. Zero-size behavior follows pinned provider."
    if symbol in {"memcpy", "memmove", "memset"}:
        result["ownership"] = "Borrow exact byte ranges and return original destination address; memcpy requires nonoverlap, memmove must support overlap; no allocation or ownership transfer."
    if symbol in {"strlen", "strchr", "strrchr", "strcmp", "strncmp", "strspn", "strcspn", "memcmp", "memchr"}:
        result["ownership"] = "Borrow caller bytes/NUL-terminated strings as selected; preserve byte comparisons and returned interior-pointer identity, with no managed string conversion."
    if symbol == "qsort":
        result.update(ownership="Borrow and mutate exactly count times element-size bytes in caller array.",
            callbacks="Synchronous native Cdecl comparator reenters translated managed body with borrowed element addresses; retain canonical comparator identity and prohibit exceptions across native boundary.")
    if symbol == "__errno_location":
        result.update(ownership="Borrow native thread-local int address, never free it; any logical shadow alternative requires a separate explicit tested provider protocol.",
            errors="The returned pointer must expose the logical error state of preceding provider calls despite managed transitions; currently unimplemented.",
            threads="Prove independent native TLS addresses/state on main and foreign callback threads, nested reentry and snapshot lifetime.")
    if symbol == "localtime":
        result["ownership"] = "Borrow provider static struct tm result (not caller-owned and not assumed thread-local); subsequent provider conversions may overwrite it. Prove captured struct layout."
        result["threads"] = "Preserve provider shared-static behavior; do not silently replace with per-thread or managed DateTime storage."
    if symbol == "getenv":
        result["ownership"] = "Borrow environment byte pointer; preserve native lifetime/invalidation rules without copying into a managed string."
    if symbol == "dlopen":
        result["ownership"] = "Return owned native loader reference or null; match dlclose and native flags; preserve path bytes and loader search contract."
    if symbol == "dlsym":
        result["ownership"] = "Borrow resolved native address under live loader handle; preserve data/function category and native address identity."
    if symbol == "dlclose":
        result["ownership"] = "Release one matching native loader reference; do not retain callable addresses after invalidation."
    if symbol in {"dlopen", "dlsym", "dlclose", "dlerror"}:
        result["errors"] = "Use native loader-error protocol separately from errno; dlerror reads/clears per-thread loader error and returns borrowed bytes. Capture before managed runtime/loader activity can replace it."
    if symbol == "dlerror":
        result["ownership"] = "Borrow per-thread loader-error text until subsequent loader operation invalidates it; a tested snapshot protocol may be required."
    if symbol in {"open64", "close"}:
        result["ownership"] = "open64 returns owned descriptor on success; close releases exactly that descriptor. Preserve selected variadic mode shape and native close/error semantics."
    if symbol in {"read", "pread64", "write", "pwrite64", "readlink"}:
        result["ownership"] = "Borrow exact caller byte range and descriptor/path; preserve partial result, offsets and output bytes (readlink adds no NUL). No automatic EINTR retry or full-transfer loop."
    if symbol in {"stat64", "fstat64", "lstat64", "utimes", "time", "gettimeofday", "nanosleep"}:
        result["ownership"] = "Borrow captured structure pointers and write only selected native fields; prove size/alignment/offsets against this host and preserve optional/null pointer roles."
    if symbol in {"mmap64", "munmap", "mremap"}:
        result["ownership"] = "Preserve native mapping address, MAP_FAILED sentinel, extent/protection/flags/offset; pair live mappings with munmap, and preserve mremap relocation/variadic-address semantics."
    if symbol == "getcwd":
        result["ownership"] = "Respect selected caller buffer/size; if supported null-buffer allocation is requested, return native allocator-owned bytes with matching free."
    if symbol == "sysconf":
        result["errors"] = "Preserve result and native errno distinction between error and indeterminate -1; snapshot error state immediately."
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--attempt", type=Path, required=True)
    args = parser.parse_args()
    if platform.system() != "Linux" or platform.machine() != "x86_64":
        raise RuntimeError("This candidate provider audit is pinned to Linux x64.")
    source = Path(__file__).resolve().with_name("import-audit.json")
    audit = json.loads(source.read_text())
    if audit["requiredImports"] != 103 or audit["builtinRequirements"] != 15:
        raise RuntimeError("Accepted required-import inventory changed; review the provider map.")
    candidate_aliases = {p["capturedLinkageName"]: p["candidateHostSymbol"] for p in audit["providerAliasCandidates"]}
    if candidate_aliases != ALIASES:
        raise RuntimeError("Explicit nine-alias contract changed.")
    libc_path = Path("/lib/x86_64-linux-gnu/libc.so.6").resolve()
    libm_path = Path("/lib/x86_64-linux-gnu/libm.so.6").resolve()
    libc, libm = ctypes.CDLL(str(libc_path)), ctypes.CDLL(str(libm_path))

    class DlInfo(ctypes.Structure):
        _fields_ = [("file", ctypes.c_char_p), ("base", ctypes.c_void_p),
                    ("symbol", ctypes.c_char_p), ("address", ctypes.c_void_p)]

    libc.dladdr.argtypes = [ctypes.c_void_p, ctypes.POINTER(DlInfo)]
    libc.dladdr.restype = ctypes.c_int
    libc.gnu_get_libc_version.restype = ctypes.c_char_p
    entries, providers = [], {libc_path, libm_path}
    for item in audit["imports"]:
        captured, group = item["import"]["linkageName"], item["candidateProvider"]
        entry = {"capturedImport": item["import"], "selectedSignature": item["selectedSignature"],
                 "status": "planned-unimplemented", "runtimeAbiExercised": False,
                 "productionBindingEnabled": False, "candidateProviderGroup": group}
        if item["recordedBuiltin"] is not None:
            builtin = item["recordedBuiltin"]
            entry.update(recordedBuiltin=builtin, nativeExportLookup=False,
                implementationRequirement=("Exact-width atomic load/store with captured memory-order operand and alignment; cross-thread ordering evidence required." if "atomic" in builtin else
                    "Full recorded synchronization fence with thread-ordering evidence." if "synchronize" in builtin else
                    "Captured operand/result widths and overflow out-parameter writes; no host-symbol import or source reanalysis." if "overflow" in builtin else
                    "Exact captured bit width/byte ordering and defined-input contract; implement as target intrinsic, not native import."))
        else:
            symbol = ALIASES.get(captured, captured)
            library = libm if group == "linux-libm" else libc
            function = getattr(library, symbol)
            info = DlInfo()
            if libc.dladdr(ctypes.cast(function, ctypes.c_void_p), ctypes.byref(info)) == 0 or not info.file:
                raise RuntimeError("Cannot resolve actual export provider: " + captured)
            resolved_image = info.file.decode()
            if resolved_image == "linux-vdso.so.1":
                actual_provider = {"kind": "kernel-vdso", "name": resolved_image,
                    "fileSha256": None, "kernelRelease": platform.release(),
                    "status": "normal-libc-resolution-host-kernel-dependency"}
            else:
                provider = Path(resolved_image).resolve()
                providers.add(provider)
                actual_provider = identity(provider) | {"kind": "file"}
            entry.update(hostEntryPoint=symbol, explicitProviderAlias=captured in ALIASES,
                exportResolved=True, requestedProviderPath=str(libm_path if group == "linux-libm" else libc_path),
                actualProvider=actual_provider, contracts=contracts(symbol, group),
                callingContract="Captured cc_default on pinned Linux x64 Cdecl/SysV; retain selected parameter order/roles, widths and variadic flag. Representation and adapter evidence remains required.")
        entries.append(entry)
    result = {"status": "candidate-exports-resolved-contracts-unimplemented", "productionBindingEnabled": False,
        "role": "T3 preparation only; export presence is not ABI or error/thread/lifetime execution proof",
        "sourceAudit": identity(source), "tool": identity(Path(__file__).resolve()),
        "acceptedInventoryReportSha256": audit["sourceReportSha256"],
        "target": "Linux x64 little-endian glibc", "glibcVersion": libc.gnu_get_libc_version().decode(),
        "requiredImports": len(entries), "resolvedNativeExports": sum(e.get("exportResolved", False) for e in entries),
        "targetIntrinsics": sum(e.get("nativeExportLookup") is False for e in entries),
        "explicitAliases": ALIASES, "providerFiles": [identity(p) for p in sorted(providers)],
        "selectedAbiTypeDescriptions": audit["selectedAbiTypeDescriptions"], "imports": entries,
        "remainingEvidence": ["all selected provider signatures/layouts exercised from generated code in JIT and NativeAOT",
            "errno and loader-error capture lifetime across allocation, native transitions, nested callbacks and foreign threads",
            "pthread layout/constants, return-code protocol, callback reentry and completion",
            "time/gettimeofday behavior through normal libc/vDSO resolution, with selected kernel provenance",
            "allocator/mapping/descriptor ownership and full SQLite differential workload",
            "all 15 required target intrinsic implementations including memory ordering"]}
    attempt = args.attempt.resolve()
    attempt.mkdir(parents=True, exist_ok=False)
    (attempt / "provider-plan.json").write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps({k: result[k] for k in ("status", "requiredImports", "resolvedNativeExports", "targetIntrinsics", "glibcVersion")}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

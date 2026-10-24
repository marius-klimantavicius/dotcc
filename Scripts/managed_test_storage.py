"""Generic guards for explicitly placed managed-test outputs and optional storage."""
import hashlib
import os
from pathlib import Path
import stat
import shutil


def require_storage_reserve(paths, minimum_bytes=2 * 2**30):
    """Check every actual allocation filesystem before creating build outputs."""
    if minimum_bytes < 2 * 2**30:
        raise ValueError("Managed builds require at least the normal 2GiB reserve")
    devices = {}
    for value in paths:
        path = Path(value)
        ancestor = path
        while not ancestor.exists():
            ancestor = ancestor.parent
        if not ancestor.is_dir():
            raise RuntimeError("Storage allocation ancestor is not a directory")
        device, free = ancestor.stat().st_dev, shutil.disk_usage(ancestor).free
        row = devices.setdefault(device, dict(device=device, paths=[], free_bytes=free,
                                             minimum_free_bytes=minimum_bytes))
        row["paths"].append(str(path))
        row["free_bytes"] = min(row["free_bytes"], free)
    rows = [dict(row, passed=row["free_bytes"] >= minimum_bytes) for _, row in sorted(devices.items())]
    if not all(row["passed"] for row in rows):
        raise RuntimeError("Managed build allocation is below the 2GiB reserve: " + str(rows))
    return rows


def validate_output_root(value, *, protected_roots=()):
    """Validate a private external allocation scope without creating anything."""
    path = Path(value)
    if not path.is_absolute() or path != path.resolve():
        raise RuntimeError("Output root must be absolute and cannot traverse a symlink")
    for protected in map(Path, protected_roots):
        protected = protected.resolve()
        if path.is_relative_to(protected) or protected.is_relative_to(path):
            raise RuntimeError("Output root overlaps protected inputs")
    ancestor = path
    while not ancestor.exists():
        ancestor = ancestor.parent
    if (not ancestor.is_dir() or ancestor.stat().st_uid != os.getuid()
            or not os.access(ancestor, os.W_OK | os.X_OK)):
        raise RuntimeError("Output root needs a writable directory owned by the current user")
    return path


def contained_output(value, root):
    """Require lexical and real containment, including all existing ancestors."""
    path, root = Path(value), Path(root)
    if (not path.is_absolute() or path != path.resolve() or root != root.resolve()
            or path == root or not path.is_relative_to(root)):
        raise RuntimeError("Output path is outside its exact scope or traverses a symlink")
    return path


def verify_files(expected):
    """Check current regular files, never historical caches or symlink aliases."""
    for value, digest in expected.items():
        path = Path(value)
        if (not path.is_absolute() or path != path.resolve()
                or not stat.S_ISREG(path.lstat().st_mode)):
            raise RuntimeError("Evidence must be a regular file without symlink traversal: " + str(path))
        with path.open("rb") as stream:
            if hashlib.file_digest(stream, "sha256").hexdigest() != digest:
                raise RuntimeError("Evidence file changed: " + str(path))


def audit_build_caches(expected, cleanup, *, required):
    """Distinguish explicitly removed build inputs from preserved execution files.

    Cleanup entries are evidence supplied by the caller's bound receipt, not an
    exception for missing runtime/source files. Existing files still verify even
    under a retired directory; a rebuilt cache never inherits a historical hash.
    """
    verify_files(required)
    retired_roots = []
    for row in cleanup:
        path = Path(row["path"])
        if ("retained" in row or "rebuildable_bytes_removed" not in row
                or row["rebuildable_bytes_removed"] < 0):
            continue
        if not path.is_absolute() or path != path.resolve():
            raise RuntimeError("Cache cleanup path traverses a symlink or is relative")
        retired_roots.append(path)
    present, retired = {}, {}
    for value, digest in expected.items():
        path = Path(value)
        if (not path.is_absolute() or path != path.resolve()):
            raise RuntimeError("Build input path traverses a symlink or is relative")
        if path.exists():
            verify_files({value: digest})
            present[value] = digest
        elif value not in required and any(path.is_relative_to(root) and path != root for root in retired_roots):
            retired[value] = digest
        else:
            raise RuntimeError("Missing build input without explicit cache retirement: " + value)
    return dict(status="verified-with-retired-caches" if retired else "verified",
                present_sha256=present, retired_sha256=retired,
                scope="Retired hashes describe historical build inputs; their bytes are absent")


def optional_storage(operation, *, verify):
    """Keep optimization errors separate only while completed evidence stays exact.

    The caller verifies receipts, execution/runtime closures and source identities
    before and after the operation. Verification failures always propagate.
    Allocation savings after an exception are unknown, including partial work.
    """
    verify()
    try:
        result = operation()
    except Exception as error:
        result = {"status": "failed", "error": str(error), "error_type": type(error).__name__,
                  "allocated_bytes_released": None,
                  "allocation_accounting": "Unknown after failure; no storage savings claimed"}
    verify()
    return dict(result, optional_optimization=True, qualification_preserved=True)

"""Generic guards for explicitly placed managed-test outputs and optional storage."""
import hashlib
import os
from pathlib import Path
import stat


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

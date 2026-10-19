"""Observe native and actual RAW creation/rename/read mode and errno exactly."""
import importlib.machinery
import importlib.util
import json
import os
import pathlib
import py_compile
import stat
import sys

root = pathlib.Path(os.environ["DOTCC_B30_PYC_ROOT"])
root.mkdir(mode=0o700)
rows = []

def snapshot(path):
    value = os.stat(path)
    return {"path": str(path), "mode": oct(value.st_mode), "permissions": oct(stat.S_IMODE(value.st_mode)),
            "uid": value.st_uid, "gid": value.st_gid, "size": value.st_size}

for mode in (0o600, 0o644, 0o666):
    temporary = root / ("open-" + oct(mode) + ".temporary")
    final = temporary.with_suffix(".renamed")
    flags = os.O_CREAT | os.O_EXCL | os.O_WRONLY
    descriptor = os.open(temporary, flags, mode)
    os.write(descriptor, b"exact bytes")
    os.close(descriptor)
    created = snapshot(temporary)
    os.replace(temporary, final)
    row = {"operation": "os.open/write/close/replace/read", "flags": flags, "requested_mode": oct(mode),
           "created": created, "renamed": snapshot(final)}
    try:
        with open(final, "rb") as stream:
            row["read"] = stream.read().decode("ascii")
    except OSError as error:
        row["read_error"] = {"type": type(error).__name__, "errno": error.errno, "message": str(error)}
    rows.append(row)

source = root / "source.py"
source.write_text("value = 42\n", encoding="utf-8")
compiled = root / "module.pyc"
row = {"operation": "unchanged py_compile.compile and SourcelessFileLoader", "source": snapshot(source)}
py_compile.compile(str(source), str(compiled), doraise=True)
row["compiled"] = snapshot(compiled)
try:
    loader = importlib.machinery.SourcelessFileLoader("r30_pyc_control", str(compiled))
    spec = importlib.util.spec_from_loader(loader.name, loader)
    module = importlib.util.module_from_spec(spec)
    loader.exec_module(module)
    row["loaded_value"] = module.value
except OSError as error:
    row["load_error"] = {"type": type(error).__name__, "errno": error.errno, "message": str(error)}
rows.append(row)
print(json.dumps({"scope": "native/RAW file-permission reduction; no accepted-product claim", "python": sys.version,
                  "root": str(root), "rows": rows}), flush=True)
assert row.get("loaded_value") == 42, row

"""Actual owning-startup deleted-cwd check, with unconditional absolute recovery."""
import errno
import json
import os
import tempfile

original = os.getcwd()
temporary = tempfile.mkdtemp(prefix="dotcc-b30-cwd-")
try:
    os.chdir(temporary)
    os.rmdir(temporary)
    try:
        os.getcwd()
    except FileNotFoundError as error:
        assert error.errno == errno.ENOENT, error
    else:
        raise AssertionError("Deleted cwd must raise FileNotFoundError/ENOENT")
finally:
    os.chdir(original)
assert os.getcwd() == original
print(json.dumps({"owning_deleted_cwd": "ENOENT", "absolute_recovery": True}), flush=True)

"""Separate focused actual-product proof; never a full-suite qualification.

Run by the unchanged RawReplayR30 owning consumer. Original upstream methods,
class fixtures, decorators and assertions are loaded without modification.
"""
import importlib
import json
import os
from pathlib import Path
import sys
import tempfile
import traceback
import unittest

report_path = Path(os.environ["DOTCC_B36_FOCUSED_REPORT"])
manifest = json.loads(Path(os.environ["DOTCC_B36_IO_HANDOFF"]).read_text())
expected = [row["case"] for row in manifest["cases"]]
assert len(expected) == len(set(expected)) == 12
assert sys.version_info[:3] == (3, 13, 15) and __debug__
checks = []
# Match every success scenario in A30's unchanged B35 native/emitted C proof.
with tempfile.TemporaryDirectory() as directory:
    descriptor = os.open(str(Path(directory) / "offsets"), os.O_RDWR | os.O_CREAT | os.O_EXCL, 0o600)
    try:
        for initial, offset, length in ((5, 5, 0), (8, 6, 3), (8, 2, 3), (8, 11, 8),
                                       (8, 11, 20), (8, 25, 20),
                                       (2147483650, 2147483650, 2147483649),
                                       (2147483649, 2147483650, 2147483651)):
            os.ftruncate(descriptor, initial)
            assert os.lseek(descriptor, offset, os.SEEK_SET) == offset
            os.ftruncate(descriptor, length)
            actual = (os.lseek(descriptor, 0, os.SEEK_CUR), os.fstat(descriptor).st_size)
            assert actual == (offset, length), (actual, offset, length)
            assert os.write(descriptor, b"\x5a") == 1
            after = (os.lseek(descriptor, 0, os.SEEK_CUR), os.fstat(descriptor).st_size)
            assert after == (offset + 1, max(length, offset + 1)), after
            assert os.lseek(descriptor, offset, os.SEEK_SET) == offset
            assert os.read(descriptor, 1) == b"\x5a"
            checks.append({"initial": initial, "offset": offset, "length": length,
                           "afterResize": list(actual), "afterWrite": list(after), "marker": 0x5a})
    finally:
        os.close(descriptor)


def flatten(suite):
    for test in suite:
        if isinstance(test, unittest.TestSuite):
            yield from flatten(test)
        else:
            yield test


loader = unittest.TestLoader()
all_tests = list(flatten(loader.loadTestsFromModule(importlib.import_module("test.test_io"))))
assert not loader.errors, loader.errors
tests = [test for test in all_tests if test.id() in set(expected)]
assert sorted(test.id() for test in tests) == sorted(expected)
rows = []


class Result(unittest.TextTestResult):
    def addSuccess(self, test):
        super().addSuccess(test)
        rows.append({"case": test.id(), "status": "passed"})

    def addFailure(self, test, error):
        super().addFailure(test, error)
        rows.append({"case": test.id(), "status": "failed", "traceback": self._exc_info_to_string(error, test)})

    def addError(self, test, error):
        super().addError(test, error)
        rows.append({"case": test.id(), "status": "error", "traceback": self._exc_info_to_string(error, test)})

    def addSkip(self, test, reason):
        super().addSkip(test, reason)
        rows.append({"case": test.id(), "status": "required-skip", "reason": reason})

    def addExpectedFailure(self, test, error):
        super().addExpectedFailure(test, error)
        rows.append({"case": test.id(), "status": "required-expected-failure", "traceback": self._exc_info_to_string(error, test)})

    def addUnexpectedSuccess(self, test):
        super().addUnexpectedSuccess(test)
        rows.append({"case": test.id(), "status": "unexpected-success"})

    def addSubTest(self, test, subtest, error):
        super().addSubTest(test, subtest, error)
        if error is not None:
            rows.append({"case": test.id(), "status": "failed-subtest", "subtest": str(subtest),
                         "traceback": self._exc_info_to_string(error, test)})


result = unittest.TextTestRunner(verbosity=2, resultclass=Result).run(unittest.TestSuite(tests))
success = (result.wasSuccessful() and result.testsRun == 12 and len(rows) == 12
           and {r["case"] for r in rows} == set(expected) and all(r["status"] == "passed" for r in rows))
report_path.write_text(json.dumps({"scope": "separate focused actual RAW JIT IO proof; not full-suite qualification",
    "python": sys.version, "offsetChecks": checks, "selectedIds": expected, "cases": rows,
    "executed": result.testsRun, "status": "passed" if success else "failed"}, indent=2) + "\n")
print(json.dumps({"scope": "focused actual IO", "executed": result.testsRun, "status": "passed" if success else "failed"}), flush=True)
assert success, "Focused actual IO proof did not pass every original ID"

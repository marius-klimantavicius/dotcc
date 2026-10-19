"""Invoke the unchanged exact-pin required case; no altered recursion limit."""
import _json
import json
import unittest
from test.test_json.test_recursion import TestCRecursion

assert json.scanner.c_make_scanner is _json.make_scanner
case = TestCRecursion("test_highly_nested_objects_decoding")
result = unittest.TextTestRunner(verbosity=2).run(unittest.TestSuite([case]))
assert result.testsRun == 1
assert not result.skipped and not result.expectedFailures
assert result.wasSuccessful(), "unchanged JSON C recursion case failed"
assert json.loads('{"after":[1,2,3]}') == {"after": [1, 2, 3]}

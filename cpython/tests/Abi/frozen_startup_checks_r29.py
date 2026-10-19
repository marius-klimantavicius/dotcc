"""Focused startup assertions; no product sys.path or sys._stdlib_dir repair."""
import _imp
import gc
import importlib.machinery
import json
import os
import sys

selected = sys.path[0]
assert sys._stdlib_dir == selected, (sys._stdlib_dir, selected)
assert os.path.isabs(selected)
assert os.path.isfile(os.path.join(selected, "encodings", "__init__.py"))
assert sys.flags.isolated == 1 and sys.flags.utf8_mode == 1
assert len(sys.path) == 2

_imp._override_frozen_modules_for_tests(1)
try:
    files = {
        "__hello__": "__hello__.py",
        "__hello_alias__": "__hello__.py",
        "__phello__.spam": "__phello__/spam.py",
        "__phello__.ham.eggs": "__phello__/ham/eggs.py",
        "_frozen_importlib": "importlib/_bootstrap.py",
        "__phello__.__init__": "__phello__/__init__.py",
        "__phello__.ham.__init__": "__phello__/ham/__init__.py",
        "__phello__": "__phello__/__init__.py",
        "__phello__.ham": "__phello__/ham/__init__.py",
        "__phello_alias__": "__hello__.py",
    }
    for name, suffix in files.items():
        spec = importlib.machinery.FrozenImporter.find_spec(name)
        expected = os.path.join(selected, *suffix.split("/"))
        assert spec.loader_state.filename == expected, (name, spec.loader_state, expected)
    gc.collect()
    assert importlib.machinery.FrozenImporter.find_spec("__hello__").loader_state.filename == os.path.join(selected, "__hello__.py")
finally:
    _imp._override_frozen_modules_for_tests(-1)

if os.environ.get("DOTCC_B29_ORIGINAL_FOUR") == "1":
    import unittest
    from test.test_importlib.frozen import test_finder
    cases = [
        cls(method)
        for cls in (test_finder.Frozen_FindSpecTests, test_finder.Source_FindSpecTests)
        for method in ("test_module", "test_package")
    ]
    results = unittest.TestResult()
    unittest.TestSuite(cases).run(results)
    print(json.dumps({"focused_original_ids": [case.id() for case in cases],
                      "testsRun": results.testsRun, "failures": len(results.failures),
                      "errors": len(results.errors), "skips": len(results.skipped)}), flush=True)
    assert results.testsRun == 4 and not results.failures and not results.errors and not results.skipped, (results.failures, results.errors)

print(json.dumps({"selected_stdlib": selected, "frozen_filename_checks": len(files),
                  "utf8_mode": sys.flags.utf8_mode, "isolated": sys.flags.isolated}), flush=True)

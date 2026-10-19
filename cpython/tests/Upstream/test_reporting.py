"""Synthetic unittest outcomes validate reporting only, never Python semantics."""

import contextlib
import importlib.util
import io
import pathlib
import sys
import tempfile
import types
import unittest
import json

spec = importlib.util.spec_from_file_location("cp_b_inventory", pathlib.Path(__file__).with_name("run_inventory.py"))
runner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(runner)


class GroupCheckpointTests(unittest.TestCase):
    def test_later_discovery_failure_keeps_completed_group_and_unexecuted_ids(self):
        class First(unittest.TestCase):
            def test_case(self): self.assertEqual(40 + 2, 42)
        class Second(unittest.TestCase):
            def test_case(self): self.assertTrue(True)
        modules = []
        for name, fixture in [('cp_b_checkpoint_first',First),('cp_b_checkpoint_second',Second)]:
            fixture.__module__ = name
            fixture.__qualname__ = fixture.__name__
            module = types.ModuleType(name)
            setattr(module,fixture.__name__,fixture)
            sys.modules[name] = module
            modules.append(module)
        self.addCleanup(lambda:[sys.modules.pop(module.__name__,None) for module in modules])
        discovery = runner.discover([module.__name__ for module in modules])
        required = [case for module in discovery for case in module['cases']]
        self.assertEqual(len(required),2)
        inventory = {'required_cases':required,'exclusions':[]}
        report = {'discovery':discovery,'executed':0,'cases':[],
                  'status':'running','qualifies_translated_interpreter':False}
        with tempfile.TemporaryDirectory() as temporary:
            destination = pathlib.Path(temporary)/'report.json'
            def checkpoint():
                runner.save_report(destination,report,inventory,[])
                def broken_load_tests(*args):
                    raise RuntimeError('later fresh loader failure')
                modules[1].load_tests = broken_load_tests
            with self.assertRaisesRegex(RuntimeError,'Fresh per-module discovery differs'):
                runner.execute_groups(inventory,discovery,report,io.StringIO(),[],
                                      stream=io.StringIO(),checkpoint=checkpoint)
            retained = json.loads(destination.read_text())
        self.assertEqual(retained['counts']['executed'],1)
        self.assertEqual(retained['counts']['passed'],1)
        self.assertEqual(retained['required_unexecuted'],[required[1]])
        self.assertEqual(retained['groups'][0]['status'],'passed')
        self.assertEqual(retained['status'],'running')
        self.assertFalse(retained['qualifies_translated_interpreter'])


class ReportingTests(unittest.TestCase):
    def run_fixture(self, body):
        class Fixture(unittest.TestCase):
            def test_case(self):
                body(self)
        Fixture.__module__ = "cp_b_reporting_fixture"
        Fixture.__qualname__ = "Fixture"
        module = types.ModuleType(Fixture.__module__)
        module.Fixture = Fixture
        sys.modules[module.__name__] = module
        case_id = module.__name__ + ".Fixture.test_case"
        inventory = {"required_cases": [case_id], "exclusions": []}
        discovery = [{"cases": [case_id]}]
        try:
            with contextlib.redirect_stderr(io.StringIO()):
                return runner.run_selected(inventory, discovery)
        finally:
            del sys.modules[module.__name__]

    def test_assertion_executes_and_passes(self):
        calls = []
        result, success = self.run_fixture(lambda test: (calls.append(1), test.assertEqual(40 + 2, 42)))
        self.assertTrue(success)
        self.assertEqual(calls, [1])
        self.assertEqual([row["status"] for row in result.cases], ["passed"])

    def test_failed_assertion_cannot_pass(self):
        result, success = self.run_fixture(lambda test: test.assertEqual(1, 2))
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "failed")
        self.assertIn("1 != 2", result.cases[0]["traceback"])

    def test_required_skip_cannot_pass(self):
        result, success = self.run_fixture(lambda test: test.skipTest("missing helper"))
        self.assertTrue(result.wasSuccessful())  # unittest alone would allow this.
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "required-skip")

    def test_failed_subtest_cannot_pass(self):
        def body(test):
            with test.subTest(value=7):
                test.assertEqual(7, 8)
        result, success = self.run_fixture(body)
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "failed-subtest")

    def test_unclassified_case_rejected_before_execution(self):
        with self.assertRaisesRegex(ValueError, "lack selection/exclusion"):
            runner.run_selected({"required_cases": ["a"], "exclusions": []}, [{"cases": ["a", "b"]}])

    def test_empty_required_set_cannot_qualify(self):
        with self.assertRaisesRegex(ValueError, "empty"):
            runner.run_selected({"required_cases": [], "exclusions": []}, [])

    def test_exclusion_requires_path_reason_and_contract(self):
        with self.assertRaisesRegex(ValueError, "upstream_path"):
            runner.run_selected({"required_cases": ["a"], "exclusions": [{"case": "b"}]}, [{"cases": ["a", "b"]}])

    def test_error_cannot_pass(self):
        def body(test): raise RuntimeError("reporter control error")
        result, success = self.run_fixture(body)
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "error")
        self.assertIn("reporter control error", result.cases[0]["traceback"])

    def test_keyboard_interrupt_retains_partial_evidence(self):
        def body(test): raise KeyboardInterrupt("reporter interruption")
        result, success = self.run_fixture(body)
        self.assertFalse(success)
        self.assertEqual(result.testsRun, 1)
        self.assertEqual(result.cases[0]["status"], "interrupted")
        self.assertIn("reporter interruption", result.interruption)

    def test_expected_failure_is_required_failure(self):
        class Fixture(unittest.TestCase):
            @unittest.expectedFailure
            def test_case(self): self.fail("known upstream failure remains blocker")
        result, success = self.run_defined(Fixture)
        self.assertTrue(result.wasSuccessful())
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "required-expected-failure")

    def test_unexpected_success_is_required_failure(self):
        class Fixture(unittest.TestCase):
            @unittest.expectedFailure
            def test_case(self): self.assertEqual(1, 1)
        result, success = self.run_defined(Fixture)
        self.assertFalse(success)
        self.assertEqual(result.cases[0]["status"], "unexpected-success")

    def test_class_setup_skip_cannot_qualify_unexecuted_cases(self):
        class Fixture(unittest.TestCase):
            @classmethod
            def setUpClass(cls): raise unittest.SkipTest("required setup helper unavailable")
            def test_case(self): self.fail("must not execute")
        result, success = self.run_defined(Fixture)
        self.assertFalse(success)
        self.assertEqual(result.testsRun, 0)
        self.assertEqual(result.cases[0]["status"], "required-skip")

    def test_class_teardown_error_cannot_qualify(self):
        class Fixture(unittest.TestCase):
            @classmethod
            def tearDownClass(cls): raise RuntimeError("teardown error retained")
            def test_case(self): self.assertTrue(True)
        result, success = self.run_defined(Fixture)
        self.assertFalse(success)
        self.assertEqual([c["status"] for c in result.cases], ["passed", "error"])

    def run_defined(self, fixture):
        fixture.__module__ = "cp_b_reporting_fixture"
        fixture.__qualname__ = "Fixture"
        module = types.ModuleType(fixture.__module__)
        module.Fixture = fixture
        sys.modules[module.__name__] = module
        case_id = module.__name__ + ".Fixture.test_case"
        try:
            with contextlib.redirect_stderr(io.StringIO()):
                return runner.run_selected({"required_cases": [case_id], "exclusions": []}, [{"cases": [case_id]}])
        finally:
            del sys.modules[module.__name__]

    def test_helper_hidden_by_module_skip_blocks_discovery(self):
        def skipped(self): pass
        skipped = unittest.skip("No module named '_required_helper'")(skipped)
        fixture = type("ModuleSkipped", (unittest.TestCase,), {"__module__": "unittest.loader", "json": skipped})
        module = types.ModuleType("cp_b_hidden_helper")
        module.load_tests = lambda *args: unittest.TestSuite([fixture("json")])
        sys.modules[module.__name__] = module
        try:
            discovery = runner.discover([module.__name__])
            self.assertEqual(discovery[0]["status"], "blocked")
            self.assertIn("helper hidden by module SkipTest", discovery[0]["traceback"])
        finally:
            del sys.modules[module.__name__]

    def test_doctest_instance_runs_with_exact_discovered_id(self):
        import doctest
        module = types.ModuleType("cp_b_doctest")
        module.__doc__ = ">>> 40 + 2\n42\n"
        case = next(iter(doctest.DocTestSuite(module)))
        key = case.id()
        with contextlib.redirect_stderr(io.StringIO()):
            result, success = runner.run_selected({"required_cases": [key], "exclusions": []}, [{"cases": [key]}], case_objects={key: case})
        self.assertTrue(success)
        self.assertEqual(result.cases[0]["case"], key)

    def test_duplicate_loader_occurrences_all_execute(self):
        calls = []
        class Fixture(unittest.TestCase):
            def test_case(self): calls.append(1)
        case1, case2 = Fixture('test_case'), Fixture('test_case')
        key = case1.id()
        live = runner.DiscoveredCases()
        live[key] = [case1, case2]
        live.ordered = [case1, case2]
        with contextlib.redirect_stderr(io.StringIO()):
            result, success = runner.run_selected({'required_cases': [key], 'exclusions': []}, [{'cases': [key]}], case_objects=live)
        self.assertTrue(success)
        self.assertEqual(calls, [1, 1])
        self.assertEqual(result.testsRun, 2)
        self.assertEqual([row['case'] for row in result.cases], [key, key])

    def test_partial_embedded_action_rejected(self):
        with self.assertRaisesRegex(ValueError, 'qualify/discover only'):
            runner.run_embedded('unused', 'unused', 'native-control')

    def test_inventory_discovery_drift_cannot_qualify(self):
        with self.assertRaisesRegex(ValueError, "Discovery changed"):
            runner.validate_inventory({"required_cases": ["a"], "exclusions": [], "discovered_cases": ["old"]}, [{"cases": ["a"]}])


if __name__ == "__main__":
    unittest.main(verbosity=2)

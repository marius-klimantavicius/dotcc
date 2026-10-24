"""Synthetic controller argument/storage tests; no controller or workload runs."""
import copy
import importlib.util
import io
import json
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import Mock, patch


REPO = Path(__file__).resolve().parents[2]
PROVIDER = REPO / "s2n-tls/tests/Provider"
sys.path.insert(0, str(PROVIDER))
spec = importlib.util.spec_from_file_location("synthetic_required_c_scopes", PROVIDER / "run-required-c.py")
runner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(runner)


class PlannedCommand(RuntimeError):
    """Stop at a captured command; never launch it."""


class RequiredCOutputScopeTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="required-c-scopes-")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name).resolve()
        self.subprocess = patch.object(runner.subprocess, "run", side_effect=AssertionError("No subprocess allowed"))
        self.process = self.subprocess.start()
        self.addCleanup(self.subprocess.stop)
        constructor = patch.object(runner.Pipeline, "__init__", side_effect=AssertionError("No controller initialization"))
        constructor.start()
        self.addCleanup(constructor.stop)

    def tearDown(self):
        self.process.assert_not_called()

    def args(self, *options):
        return runner.parse_args([
            "--campaign-run", str(self.root / "synthetic-campaign"),
            "--emission-manifest", str(self.root / "synthetic-emission.json"),
            "--native-report", str(self.root / "synthetic-native.json"),
            "--output", str(self.root / "controller-output"), *map(str, options)])

    def pipeline(self, roots=None, *, recorded=None, options=()):
        argv = list(options)
        for root in roots or ():
            argv += ["--dependency-output-root", root]
        instance = runner.Pipeline.__new__(runner.Pipeline)
        instance.args = self.args(*argv)
        instance.progress = copy.deepcopy(recorded) if recorded is not None else {}
        instance.configure_dependency_output_roots(resume=recorded is not None)
        instance.output = instance.args.output
        instance.work_root = instance.args.work_root
        instance.provider_freeze = None
        instance.command = Mock(side_effect=PlannedCommand("Captured only"))
        return instance

    def plan(self, instance, *, row=None, built=None):
        return list(map(str, instance.expansion_argv(
            self.root / "raw.json", self.root / "expanded.json", row or {},
            built or ["tests/unit/synthetic_scope.c"])))

    def test_parser_retains_repeatable_exact_roots_and_no_option_sentinel(self):
        self.assertIsNone(self.args().dependency_output_root)
        paths = [self.root / "first", self.root / "second", self.root / "first"]
        args = self.args(*[part for p in paths for part in ("--dependency-output-root", p)])
        self.assertEqual(args.dependency_output_root, paths)
        self.assertTrue(all(not p.exists() for p in paths))

    def test_new_scopes_are_recorded_and_passed_in_caller_order(self):
        roots = [self.root / "raw-outputs", self.root / "separate-outputs"]
        instance = self.pipeline(roots)
        self.assertEqual(instance.progress, {"dependency_output_roots": list(map(str, roots))})
        command = self.plan(instance)
        observed = [command[i + 1] for i, value in enumerate(command) if value == "--dependency-output-root"]
        self.assertEqual(observed, list(map(str, roots)))
        self.assertNotIn(str(self.root), observed)
        self.assertFalse(instance.output.exists())
        self.assertTrue(all(not p.exists() for p in roots))

    def test_no_option_preserves_legacy_progress_and_exact_child_arguments(self):
        instance = self.pipeline()
        self.assertEqual(instance.progress, {})
        self.assertEqual(instance.dependency_output_roots, ())
        self.assertEqual(self.plan(instance), [sys.executable, "-B",
            str(PROVIDER / "expand-original-controls.py"), "--executables", str(self.root / "raw.json"),
            "--output", str(self.root / "expanded.json"), "--native-report", str(self.root / "synthetic-native.json"),
            "--case-set", "required-c", "--jobs", "2", "--case", "tests/unit/synthetic_scope.c"])

    def test_resume_omission_inherits_recorded_scopes_without_mutating_input(self):
        roots = [str(self.root / "first"), str(self.root / "second")]
        recorded = {"dependency_output_roots": roots, "cases": {"synthetic": {"status": "failed"}}}
        before = copy.deepcopy(recorded)
        instance = self.pipeline(recorded=recorded)
        self.assertEqual(instance.dependency_output_roots, tuple(map(Path, roots)))
        self.assertEqual(instance.progress, before)
        self.assertEqual(recorded, before)
        self.assertEqual(self.plan(instance).count("--dependency-output-root"), 2)

    def test_resume_accepts_only_identical_explicit_list(self):
        roots = [self.root / "first", self.root / "second"]
        recorded = {"dependency_output_roots": list(map(str, roots))}
        instance = self.pipeline(roots, recorded=recorded)
        self.assertEqual(instance.dependency_output_roots, tuple(roots))
        for changed in (roots[:1], roots[::-1], roots + [self.root / "extra"], [self.root]):
            with self.subTest(changed=changed), self.assertRaisesRegex(RuntimeError, "cannot change"):
                self.pipeline(changed, recorded=recorded)
        self.assertEqual(recorded, {"dependency_output_roots": list(map(str, roots))})

    def test_legacy_resume_stays_empty_and_rejects_new_scope(self):
        recorded = {"cases": {"synthetic": {"status": "unrun"}}}
        instance = self.pipeline(recorded=recorded)
        self.assertEqual(instance.progress, recorded)
        self.assertNotIn("--dependency-output-root", self.plan(instance))
        with self.assertRaisesRegex(RuntimeError, "cannot change"):
            self.pipeline([self.root / "new"], recorded=recorded)

    def test_explicit_recorded_empty_list_remains_empty(self):
        instance = self.pipeline(recorded={"dependency_output_roots": []})
        self.assertEqual(instance.progress, {"dependency_output_roots": []})
        self.assertNotIn("--dependency-output-root", self.plan(instance))

    def test_malformed_recorded_scopes_fail_without_allocation(self):
        for value in (None, str(self.root), {"scope": str(self.root)}):
            with self.subTest(value=value), self.assertRaisesRegex(RuntimeError, "explicit list"):
                self.pipeline(recorded={"dependency_output_roots": value})
        self.assertEqual(list(self.root.iterdir()), [])

    def test_relative_and_parent_traversal_are_rejected_without_allocation(self):
        for value in (Path("relative-output"), self.root / "new" / ".." / "output"):
            for recorded in (False, True):
                with self.subTest(value=value, recorded=recorded), self.assertRaisesRegex(RuntimeError, "absolute"):
                    if recorded:
                        self.pipeline(recorded={"dependency_output_roots": [str(value)]})
                    else:
                        self.pipeline([value])
        self.assertEqual(list(self.root.iterdir()), [])

    def test_protected_roots_and_ancestors_are_rejected(self):
        for value in (REPO, REPO / "s2n-tls/build", REPO / "new-output", REPO.parent, Path("/")):
            for recorded in (False, True):
                with self.subTest(value=value, recorded=recorded), self.assertRaisesRegex(RuntimeError, "protected"):
                    if recorded:
                        self.pipeline(recorded={"dependency_output_roots": [str(value)]})
                    else:
                        self.pipeline([value])

    def test_symlink_and_dangling_symlink_roots_are_rejected(self):
        target = self.root / "target"
        target.mkdir()
        for name, destination in (("alias", target), ("dangling", self.root / "absent")):
            alias = self.root / name
            try:
                alias.symlink_to(destination, target_is_directory=True)
            except (OSError, NotImplementedError) as error:
                self.skipTest("Symlink creation is unavailable: " + str(error))
            for value in (alias, alias / "nested"):
                for recorded in (False, True):
                    with self.subTest(value=value, recorded=recorded), self.assertRaisesRegex(RuntimeError, "symlink"):
                        if recorded:
                            self.pipeline(recorded={"dependency_output_roots": [str(value)]})
                        else:
                            self.pipeline([value])
        self.assertEqual(list(target.iterdir()), [])

    def test_new_symlink_after_admission_fails_before_reserve_or_command(self):
        root = self.root / "future-output"
        instance = self.pipeline([root])
        destination = self.root / "destination"
        destination.mkdir()
        try:
            root.symlink_to(destination, target_is_directory=True)
        except (OSError, NotImplementedError) as error:
            self.skipTest("Symlink creation is unavailable: " + str(error))
        with patch.object(runner, "storage_reserves") as reserves:
            with self.assertRaisesRegex(RuntimeError, "symlink"):
                instance.batch_storage_reserves({})
            with self.assertRaisesRegex(RuntimeError, "symlink"):
                self.plan(instance)
        reserves.assert_not_called()
        instance.command.assert_not_called()
        self.assertEqual(list(destination.iterdir()), [])

    def test_resumed_batch_retains_scopes_and_existing_options(self):
        root = self.root / "raw-outputs"
        work = self.root / "work"
        equivalence = self.root / "equivalence.json"
        instance = self.pipeline([root], options=("--work-root", work, "--min-free-gib", "3",
                                                 "--native-equivalence", equivalence))
        freeze = self.root / "freeze.json"
        instance.provider_freeze = {"receipt": str(freeze)}
        (self.root / "expanded.json").write_text(json.dumps({"dependency_output_roots": [str(root)]}))
        command = self.plan(instance, row={"provider_refresh_from_raw_manifest": "synthetic-prior"},
                            built=["tests/unit/synthetic_second.c", "tests/unit/synthetic_first.c"])
        self.assertEqual(command[command.index("--work-root"):], ["--work-root", str(work), "--min-free-gib", "3.0",
            "--dependency-output-root", str(root), "--native-equivalence", str(equivalence),
            "--provider-freeze", str(freeze), "--refresh-provider", "--resume",
            "--case", "tests/unit/synthetic_second.c", "--case", "tests/unit/synthetic_first.c"])

    def test_existing_batch_scope_conflicts_fail_before_child_or_reserve(self):
        root = self.root / "current-scope"
        instance = self.pipeline([root])
        for recorded in ({}, {"dependency_output_roots": [str(self.root / "other-scope")]}):
            path = self.root / "expanded.json"
            path.write_text(json.dumps(dict(recorded, work=str(self.root / "old-work"))))
            before = path.read_bytes()
            with patch.object(runner, "storage_reserves") as reserves:
                with self.assertRaisesRegex(RuntimeError, "cannot change"):
                    self.plan(instance)
                with self.assertRaisesRegex(RuntimeError, "cannot change"):
                    instance.batch_storage_reserves({"expanded_manifest": str(path)})
            reserves.assert_not_called()
            instance.command.assert_not_called()
            self.assertEqual(path.read_bytes(), before)

    def test_legacy_controller_cannot_silently_inherit_batch_only_scopes(self):
        instance = self.pipeline()
        (self.root / "expanded.json").write_text(json.dumps({"dependency_output_roots": [str(self.root / "unknown")]}))
        with self.assertRaisesRegex(RuntimeError, "cannot change"):
            self.plan(instance)
        instance.command.assert_not_called()

    def test_legacy_existing_batch_keeps_resume_without_new_scope_arguments(self):
        instance = self.pipeline(recorded={})
        report = self.root / "expanded.json"
        report.write_text(json.dumps({"work": str(self.root / "recorded-work")}))
        before = report.read_bytes()
        command = self.plan(instance)
        self.assertEqual(command[-3:], ["--resume", "--case", "tests/unit/synthetic_scope.c"])
        self.assertNotIn("--dependency-output-root", command)
        self.assertEqual(report.read_bytes(), before)

    def test_batch_reserve_covers_exact_metadata_work_output_and_recorded_paths(self):
        roots = [self.root / "first-outputs", self.root / "second-outputs"]
        work = self.root / "future-work"
        instance = self.pipeline(roots, options=("--work-root", work))
        report = self.root / "expanded.json"
        previous_work = self.root / "recorded-work"
        report.write_text(json.dumps({"dependency_output_roots": list(map(str, roots)), "work": str(previous_work)}))
        failed = [{"passed": False, "minimum_free_bytes": 2 * 2**30}]
        with patch.object(runner, "storage_reserves", return_value=failed) as reserves:
            self.assertEqual(instance.batch_storage_reserves({"expanded_manifest": str(report)}), failed)
        reserves.assert_called_once_with([instance.output, *roots, work, previous_work], 2 * 2**30)
        instance.command.assert_not_called()
        self.assertTrue(all(not p.exists() for p in [instance.output, *roots, work, previous_work]))

    def test_legacy_batch_reserve_arguments_stay_unchanged(self):
        instance = self.pipeline()
        with patch.object(runner, "storage_reserves", return_value=[]) as reserves:
            self.assertEqual(instance.batch_storage_reserves({}), [])
        reserves.assert_called_once_with([instance.output], 2 * 2**30)

    def test_scope_drift_in_memory_is_rejected_before_command_or_reserve(self):
        instance = self.pipeline([self.root / "original"])
        instance.dependency_output_roots = (self.root / "changed",)
        with patch.object(runner, "storage_reserves") as reserves:
            with self.assertRaisesRegex(RuntimeError, "cannot change"):
                instance.batch_storage_reserves({})
            with self.assertRaisesRegex(RuntimeError, "cannot change"):
                self.plan(instance)
        reserves.assert_not_called()
        instance.command.assert_not_called()

    def test_parser_preserves_2gib_floor_and_rejects_nonfinite_reserves(self):
        self.assertEqual(self.args().min_free_gib, 2)
        self.assertEqual(self.args("--min-free-gib", "2.5").min_free_gib, 2.5)
        for value in ("0", "1.99", "nan", "inf", "-inf"):
            with self.subTest(value=value), patch("sys.stderr", new_callable=io.StringIO), self.assertRaises(SystemExit) as error:
                self.args("--min-free-gib=" + value)
            self.assertEqual(error.exception.code, 2)

    def test_higher_reserve_reaches_expander_without_work_root(self):
        instance = self.pipeline(options=("--min-free-gib", "2.5"))
        command = self.plan(instance)
        self.assertNotIn("--work-root", command)
        self.assertEqual(command[command.index("--min-free-gib") + 1], "2.5")
        with patch.object(runner, "storage_reserves", return_value=[]) as reserves:
            instance.batch_storage_reserves({})
        reserves.assert_called_once_with([instance.output], 2.5 * 2**30)

    def test_actual_batch_wiring_captures_roots_without_running_child(self):
        roots = [self.root / "raw-outputs", self.root / "additional-outputs"]
        instance = self.pipeline(roots)
        instance.output.mkdir()
        instance.campaign = instance.args.campaign_run
        instance.save = Mock()
        instance.add_support = Mock()
        case = "tests/unit/synthetic_scope_plumbing.c"
        instance.progress["cases"] = {case: {"status": "unrun"}}
        row = {"index": 0, "status": "unrun", "cases": [case]}
        batch = instance.output / "batch-000"
        batch.mkdir()
        raw = batch / "raw-executables.json"
        raw.write_text(json.dumps({"campaign_run": str(instance.campaign), "executables": [
            {"case": case, "form": "raw", "mode": "jit", "status": "built"}]}))
        before = raw.read_bytes()
        with self.assertRaisesRegex(PlannedCommand, "Captured only"):
            instance.batch(row)
        instance.command.assert_called_once()
        argv, directory, label = instance.command.call_args.args
        command = list(map(str, argv))
        observed = [command[i + 1] for i, value in enumerate(command) if value == "--dependency-output-root"]
        self.assertEqual(observed, list(map(str, roots)))
        self.assertEqual(command[-2:], ["--case", case])
        self.assertEqual((directory, label), (batch, "all4"))
        self.assertEqual(raw.read_bytes(), before)
        self.assertEqual(instance.progress["cases"][case], {"status": "unrun"})
        self.assertFalse((instance.output / "progress.json").exists())
        self.assertFalse((batch / "expanded-executables.json").exists())
        self.assertTrue(all(not p.exists() for p in roots))


if __name__ == "__main__":
    unittest.main()

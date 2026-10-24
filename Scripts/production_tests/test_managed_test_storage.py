"""Synthetic output-layout and optional-storage controls; no compiler or network."""
import importlib.util
import json
import os
from pathlib import Path
import sys
import tempfile
import threading
import unittest
from unittest.mock import Mock, patch

SCRIPTS = Path(__file__).resolve().parents[1]
REPO = SCRIPTS.parent
PROVIDER = REPO / "s2n-tls/tests/Provider"
sys.path.insert(0, str(SCRIPTS))
sys.path.insert(0, str(PROVIDER))
import qualified_dependencies as dependencies
from managed_test_storage import contained_output, optional_storage, validate_output_root, verify_files

spec = importlib.util.spec_from_file_location("synthetic_storage_expander", PROVIDER / "expand-original-controls.py")
expander = importlib.util.module_from_spec(spec)
spec.loader.exec_module(expander)


class StorageTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="managed-storage-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name).resolve()

    def fixture(self):
        """Four closed synthetic records, with distinct source/output/runtime files."""
        case = "tests/unit/synthetic_storage.c"
        rows = []
        for form in ("raw", "processed"):
            for mode in ("jit", "aot"):
                base = self.root / (form + "-" + mode)
                base.mkdir()
                project = base / "fixture.csproj"
                project.write_text("<Project />\n")
                output = base / "application.txt"
                output.write_text("synthetic output; never executed\n")
                runtime = base / "runtime.txt"
                runtime.write_text("synthetic runtime identity\n")
                closure = expander.hashes([output])
                if mode == "jit":
                    dll = base / dependencies.DLL
                    dll.write_bytes(b"synthetic pinned dependency; not an assembly")
                    closure.update(expander.hashes([dll]))
                identity = base / "identity.json"
                identity.write_text(json.dumps({"output_sha256": closure,
                    "runtime_closure_sha256": expander.hashes([runtime])}))
                rows.append(dict(case=case, form=form, mode=mode, status="built", project=str(project),
                    source_sha256=expander.tree_hashes(base), execution_sha256=closure,
                    application_execution_receipt=str(identity),
                    application_execution_receipt_sha256=expander.digest(identity)))
        qualification = self.root / "qualification.json"
        qualification.write_text(json.dumps({"status": "bounded-passed", "counts": {"passed": 4, "failed": 0},
            "cases": [dict(case=case, form=r["form"], mode=r["mode"], status="passed",
                           assertions=3, native_assertions=3) for r in rows]}))
        instance = expander.Expansion.__new__(expander.Expansion)
        instance.work = self.root / "work"
        instance.work_root = None
        instance.dependency_output_roots = (self.root,)
        instance.artifacts = self.root / "evidence"
        instance.artifacts.mkdir()
        instance.entries = {case: rows[0]}
        instance.report = {"executables": rows, "cache_cleanup": {}, "case_failures": {},
            "qualifications": {case: {"receipt": str(qualification), "sha256": expander.digest(qualification)}}}
        instance.lock = threading.RLock()
        instance.save = Mock()
        instance.verify_immutable = Mock()
        instance.command = Mock(side_effect=AssertionError("No builds or execution allowed"))
        return instance, case, rows, qualification

    def test_explicit_external_output_scope_accepts_noncanonical_raw_layout(self):
        work = self.root / "build-volume"
        raw = self.root / "separate-raw-output"
        target = raw / "artifacts/bin/Fixture/release" / dependencies.DLL
        store = dependencies.dependency_store(target, work_root=work, output_roots=[raw])
        self.assertEqual(store, raw / "immutable-dependencies")
        self.assertFalse(raw.exists())
        self.assertFalse(work.exists())

    def test_optional_storage_failure_retains_completed_qualification(self):
        instance, case, rows, qualification = self.fixture()
        expected = qualification.read_bytes()
        with patch.object(expander, "deduplicate_qualified_case", side_effect=RuntimeError("external layout unavailable")):
            instance.build_case(case)
        self.assertIn(case, instance.report["qualifications"])
        self.assertNotIn(case, instance.report["case_failures"])
        self.assertEqual(qualification.read_bytes(), expected)
        instance.command.assert_not_called()
        storage = instance.report["dependency_storage"][case][-1]
        self.assertEqual(storage["status"], "failed")
        failure = json.loads(Path(storage["receipt"]).read_text())
        self.assertIn("external layout unavailable", failure["error"])
        self.assertTrue(failure["qualification_preserved"])
        self.assertIsNone(storage["allocated_bytes_released"])

    def test_partial_storage_error_keeps_original_failed_operation_receipt(self):
        instance, case, rows, qualification = self.fixture()
        detail_bytes = b'{"status":"failed","error":"synthetic partial storage failure","replacements":[]}'
        def fail_with_receipt(*args, **kwargs):
            Path(args[2]).write_bytes(detail_bytes)
            raise RuntimeError("synthetic partial storage failure")
        with patch.object(expander, "deduplicate_qualified_case", side_effect=fail_with_receipt):
            instance.build_case(case)
        storage = instance.report["dependency_storage"][case][-1]
        report = json.loads(Path(storage["receipt"]).read_text())
        detail = Path(report["operation_receipt"])
        self.assertEqual(detail.read_bytes(), detail_bytes)
        self.assertEqual(report["operation_receipt_sha256"], expander.digest(detail))
        self.assertIn(case, instance.report["qualifications"])
        self.assertEqual(report["status"], "failed")
        self.assertIsNone(report["allocated_bytes_released"])

    def test_unspecified_external_output_is_rejected_without_creating_store(self):
        with self.assertRaisesRegex(RuntimeError, "outside owned"):
            dependencies.dependency_store(self.root / "arbitrary-output" / dependencies.DLL,
                                          work_root=self.root / "work")
        self.assertEqual(list(self.root.iterdir()), [])

    def test_output_scope_requires_exact_containment(self):
        raw = self.root / "raw"
        with self.assertRaisesRegex(RuntimeError, "outside owned"):
            dependencies.dependency_store(self.root / "raw-other" / dependencies.DLL, output_roots=[raw])
        with self.assertRaisesRegex(RuntimeError, "outside its exact scope"):
            contained_output(raw, raw)

    def test_relative_normalized_or_protected_roots_are_rejected(self):
        for root in (Path("relative"), self.root / "nonexistent/../other", REPO,
                     REPO / "DotCC", REPO.parent):
            with self.subTest(root=root), self.assertRaises(RuntimeError):
                validate_output_root(root, protected_roots=[REPO])
        self.assertEqual(list(self.root.iterdir()), [])

    def test_symlink_scope_and_output_are_rejected(self):
        raw = self.root / "raw"
        raw.mkdir()
        alias = self.root / "alias"
        alias.symlink_to(raw, target_is_directory=True)
        with self.assertRaisesRegex(RuntimeError, "symlink"):
            validate_output_root(alias)
        with self.assertRaisesRegex(RuntimeError, "symlink"):
            dependencies.dependency_store(alias / dependencies.DLL, output_roots=[self.root])

    def test_file_as_allocation_ancestor_is_rejected(self):
        file = self.root / "file"
        file.write_text("not a directory")
        with self.assertRaisesRegex(RuntimeError, "writable directory"):
            validate_output_root(file / "nested")

    def test_legacy_and_recorded_work_layouts_keep_their_original_stores(self):
        for prefix in (dependencies.ROOT / "build/provider", dependencies.ROOT / "build/managed-upstream"):
            self.assertEqual(dependencies.dependency_store(prefix / "case/bin" / dependencies.DLL), dependencies.STORE)
        work = self.root / "work"
        for prefix in (work / "provider/original16-expanded", work / "managed-upstream"):
            self.assertEqual(dependencies.dependency_store(prefix / "case/bin" / dependencies.DLL, work),
                             work / "provider/immutable-dependencies")

    def test_optional_operation_never_runs_if_verification_already_fails(self):
        operation = Mock()
        with self.assertRaisesRegex(RuntimeError, "prior evidence drift"):
            optional_storage(operation, verify=Mock(side_effect=RuntimeError("prior evidence drift")))
        operation.assert_not_called()

    def test_verification_failure_after_successful_storage_propagates(self):
        with self.assertRaisesRegex(RuntimeError, "post evidence drift"):
            optional_storage(lambda: {"status": "passed"},
                             verify=Mock(side_effect=[None, RuntimeError("post evidence drift")]))

    def test_output_runtime_identity_qualification_and_source_drift_still_invalidate(self):
        for changed in ("output", "runtime", "identity", "qualification", "source"):
            with self.subTest(changed=changed), tempfile.TemporaryDirectory(dir=self.root) as directory:
                original_root, self.root = self.root, Path(directory)
                try:
                    instance, case, rows, qualification = self.fixture()
                    identity = Path(rows[0]["application_execution_receipt"])
                    record = json.loads(identity.read_text())
                    target = {"output": Path(next(iter(record["output_sha256"]))),
                              "runtime": Path(next(iter(record["runtime_closure_sha256"]))),
                              "identity": identity, "qualification": qualification,
                              "source": Path(rows[0]["project"])}[changed]
                    def fail_after_change(*args, **kwargs):
                        target.write_bytes(b"changed during optional operation")
                        raise RuntimeError("optional failure must not hide drift")
                    with patch.object(expander, "deduplicate_qualified_case", side_effect=fail_after_change):
                        instance.build_case(case)
                    self.assertNotIn(case, instance.report["qualifications"])
                    self.assertIn(case, instance.report["case_failures"])
                    instance.command.assert_not_called()
                finally:
                    self.root = original_root

    def test_symlink_to_identical_evidence_is_not_accepted(self):
        actual = self.root / "output"
        actual.write_text("actual output")
        alias = self.root / "alias"
        alias.symlink_to(actual)
        with self.assertRaisesRegex(RuntimeError, "symlink"):
            verify_files({str(alias): expander.digest(actual)})

    def test_dependency_sharing_synthetic_bytes_uses_explicit_scopes(self):
        instance, case, rows, qualification = self.fixture()
        first = json.loads(Path(rows[0]["application_execution_receipt"]).read_text())
        source = next(Path(p) for p in first["output_sha256"] if Path(p).name == dependencies.DLL)
        source_inode = source.stat().st_ino
        expected = expander.digest(source)
        preserved = {str(qualification): expander.digest(qualification)}
        for row in rows:
            path = Path(row["application_execution_receipt"])
            preserved[str(path)] = expander.digest(path)
            identity = json.loads(path.read_text())
            preserved.update(identity["output_sha256"])
            preserved.update(identity["runtime_closure_sha256"])
        with patch.object(dependencies, "pinned_package", return_value=(source, expected, {"inputs_sha256": {}})), \
             patch.object(dependencies, "active_readers", return_value={"readers": []}):
            instance.build_case(case)
        self.assertIn(case, instance.report["qualifications"])
        self.assertEqual(expander.hashes(preserved), preserved)
        storage = instance.report["dependency_storage"][case][-1]
        self.assertEqual(storage["status"], "passed")
        report = json.loads(Path(storage["receipt"]).read_text())
        self.assertTrue(report["qualification_preserved"])
        store = self.root / "immutable-dependencies/sha256" / expected / dependencies.DLL
        self.assertEqual(expander.digest(store), expected)
        self.assertEqual(store.stat().st_mode & 0o222, 0)
        self.assertNotEqual(store.stat().st_ino, source_inode)
        self.assertEqual(len(report["replacements"]), 2)
        for row in report["replacements"]:
            self.assertIn(row["status"], ("shared", "retained"))
            self.assertEqual(expander.digest(row["path"]), expected)
        instance.command.assert_not_called()

    def test_incomplete_qualification_fails_before_optional_mutation(self):
        instance, case, rows, qualification = self.fixture()
        observed = json.loads(qualification.read_text())
        observed["counts"]["passed"] = 3
        qualification.write_text(json.dumps(observed))
        with patch.object(dependencies, "pinned_package") as package, patch.object(dependencies, "store_member") as store, \
             self.assertRaisesRegex(RuntimeError, "closed, qualified"):
            dependencies.deduplicate_qualified_case(rows, qualification, self.root / "result.json", output_roots=[self.root])
        package.assert_not_called()
        store.assert_not_called()

    def test_tampered_runtime_fails_before_optional_mutation(self):
        instance, case, rows, qualification = self.fixture()
        source = self.root / "raw-jit" / dependencies.DLL
        expected = expander.digest(source)
        identity = json.loads(Path(rows[0]["application_execution_receipt"]).read_text())
        Path(next(iter(identity["runtime_closure_sha256"]))).write_text("changed")
        with patch.object(dependencies, "pinned_package", return_value=(source, expected, {})), \
             patch.object(dependencies, "store_member") as store, self.assertRaisesRegex(RuntimeError, "evidence changed"):
            dependencies.deduplicate_qualified_case(rows, qualification, self.root / "result.json", output_roots=[self.root])
        store.assert_not_called()

    def test_standalone_options_forward_explicit_build_and_output_scopes(self):
        instance, case, rows, qualification = self.fixture()
        manifest = self.root / "manifest.json"
        manifest.write_text(json.dumps(instance.report))
        work = self.root / "build-root"
        output = self.root / "storage-report"
        argv = ["qualified_dependencies.py", "--executables", str(manifest), "--output", str(output),
                "--work-root", str(work), "--dependency-output-root", str(self.root)]
        with patch.object(sys, "argv", argv), patch.object(dependencies, "deduplicate_qualified_case",
                return_value={"status": "passed", "allocated_bytes_released": 0}) as sharing:
            def write_receipt(*args, **kwargs):
                Path(args[2]).write_text('{"status":"passed"}')
                return {"status": "passed", "allocated_bytes_released": 0}
            sharing.side_effect = write_receipt
            self.assertEqual(dependencies.main(), 0)
        self.assertEqual(sharing.call_args.kwargs, {"work_root": work, "output_roots": [self.root]})

    def test_standalone_inherits_only_explicitly_recorded_scopes(self):
        instance, case, rows, qualification = self.fixture()
        instance.report.update(work_root=str(self.root / "recorded-build"),
                               dependency_output_roots=[str(self.root / "recorded-output")])
        manifest = self.root / "manifest.json"
        manifest.write_text(json.dumps(instance.report))
        argv = ["qualified_dependencies.py", "--executables", str(manifest),
                "--output", str(self.root / "storage-report")]
        with patch.object(sys, "argv", argv), patch.object(dependencies, "deduplicate_qualified_case") as sharing:
            def write_receipt(*args, **kwargs):
                Path(args[2]).write_text('{"status":"passed"}')
                return {"status": "passed", "allocated_bytes_released": 0}
            sharing.side_effect = write_receipt
            self.assertEqual(dependencies.main(), 0)
        self.assertEqual(sharing.call_args.kwargs, {"work_root": self.root / "recorded-build",
                                                  "output_roots": [self.root / "recorded-output"]})

    def test_busy_outputs_are_retained_without_storage_mutation(self):
        instance, case, rows, qualification = self.fixture()
        source = self.root / "raw-jit" / dependencies.DLL
        expected = expander.digest(source)
        with patch.object(dependencies, "pinned_package", return_value=(source, expected, {"inputs_sha256": {}})), \
             patch.object(dependencies, "active_readers", return_value={"readers": [{"pid": 123, "kind": "synthetic"}]}), \
             patch.object(dependencies, "store_member") as store:
            instance.build_case(case)
        store.assert_not_called()
        self.assertIn(case, instance.report["qualifications"])
        storage = instance.report["dependency_storage"][case][-1]
        self.assertEqual(storage["status"], "retained-busy")
        self.assertEqual(storage["allocated_bytes_released"], 0)

    def test_store_lock_and_nested_symlinks_fail_before_external_write(self):
        for component in ("store.lock", "sha256"):
            with self.subTest(component=component), tempfile.TemporaryDirectory(dir=self.root) as directory:
                base = Path(directory)
                store, outside = base / "store", base / "outside"
                store.mkdir()
                outside.mkdir()
                source = base / "dependency"
                source.write_bytes(b"synthetic dependency")
                (store / component).symlink_to(outside, target_is_directory=True)
                with self.assertRaisesRegex(RuntimeError, "symlink"):
                    dependencies.store_member(source, expander.digest(source), store=store)
                self.assertEqual(list(outside.iterdir()), [])

    def test_dependency_store_checks_root_and_store_reserves_before_allocation(self):
        source = self.root / "dependency"
        source.write_bytes(b"synthetic dependency")
        store = self.root / "unallocated-store"
        with patch.object(dependencies, "storage_reserves", return_value=[{"passed": False}]) as reserves, \
             self.assertRaisesRegex(RuntimeError, "2GiB reserve"):
            dependencies.store_member(source, expander.digest(source), store=store)
        reserves.assert_called_once_with([dependencies.ROOT, store], 2 * 2**30)
        self.assertFalse(store.exists())

    def test_storage_reserve_coalesces_paths_on_same_device_without_allocation(self):
        root = self.root / "uncreated-root"
        work = self.root / "uncreated-work"
        minimum = 2 * 2**30
        with patch.object(dependencies.shutil, "disk_usage", return_value=type("Usage", (), {"free": minimum - 1})()):
            observation = dependencies.storage_reserves([root, work], minimum)
        self.assertEqual(len(observation), 1)
        self.assertFalse(observation[0]["passed"])
        self.assertEqual(set(observation[0]["paths"]), {str(root), str(work)})
        self.assertEqual(list(self.root.iterdir()), [])

    def test_storage_reserve_keeps_independent_filesystem_failures(self):
        metadata, work = self.root / "metadata", self.root / "work"
        metadata.mkdir()
        work.mkdir()
        actual_stat = Path.stat
        minimum = 2 * 2**30
        def device_stat(path, *args, **kwargs):
            values = list(actual_stat(path, *args, **kwargs))
            if path in (metadata, work):
                values[2] = 101 if path == metadata else 202
            return os.stat_result(values)
        def usage(path):
            return type("Usage", (), {"free": minimum - 1 if path == metadata else minimum + 1})()
        with patch.object(Path, "stat", device_stat), patch.object(dependencies.shutil, "disk_usage", side_effect=usage):
            observation = dependencies.storage_reserves([metadata / "future", work / "future"], minimum)
        self.assertEqual([(r["device"], r["passed"]) for r in observation], [(101, False), (202, True)])
        self.assertEqual(list(metadata.iterdir()), [])
        self.assertEqual(list(work.iterdir()), [])


if __name__ == "__main__":
    unittest.main()

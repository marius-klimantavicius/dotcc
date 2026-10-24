"""Source reconstruction and truthful cache-retirement regressions; no suites."""
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SCRIPTS))
from managed_source_build import build_environment, source_inputs, write_project
from managed_test_storage import audit_build_caches, require_storage_reserve

spec = importlib.util.spec_from_file_location("preserved_build_audit", SCRIPTS.parent / "s2n-tls/scripts/audit-preserved-build.py")
preserved = importlib.util.module_from_spec(spec)
spec.loader.exec_module(preserved)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


class ExternalSourceBuildTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="external-source-build-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name).resolve()
        self.project = self.root / "Original.csproj"
        self.project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
                                '<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>'
                                '</PropertyGroup></Project>')
        self.source = self.root / "Original.cs"
        self.source.write_text('System.Console.WriteLine("original");\n')
        (self.root / "Dotcc.SourceFiles.txt").write_text("Original.cs\n")

    def test_reconstruction_keeps_original_source_and_declares_real_reference(self):
        dependency = self.root / "RealDependency.dll"
        dependency.write_bytes(b"reference metadata only; SDK is never run in this test")
        properties, sources, refs, original = source_inputs(self.project, [dependency])
        output = self.root / "Reconstructed.csproj"
        write_project(output, properties, [(p.name, p) for p in sources], [(p.stem, p) for p in refs])
        import xml.etree.ElementTree as ET
        tree = ET.parse(output)
        self.assertEqual(tree.find("ItemGroup/Compile").get("Include"), str(self.source))
        self.assertEqual(tree.findtext("ItemGroup/Reference/HintPath"), str(dependency))
        self.assertEqual(tree.findtext("PropertyGroup/EnableDefaultCompileItems"), "false")
        self.assertEqual(tree.findtext("PropertyGroup/TargetFramework"), "net10.0")
        self.assertEqual(original[str(self.source)], sha(self.source))

    def test_ambient_build_credentials_never_enter_receipt_environment(self):
        with patch.dict('os.environ', {'NUGET_AUTH_TOKEN': 'ambient-test-marker',
                                       'DOTNET_CLI_HOME': 'ambient-home'}):
            actual, recorded = build_environment(self.root)
        self.assertEqual(actual['NUGET_AUTH_TOKEN'], 'ambient-test-marker')
        self.assertNotIn('NUGET_AUTH_TOKEN', recorded)
        self.assertNotIn('ambient-test-marker', json.dumps(recorded))
        self.assertEqual(recorded['DOTNET_CLI_HOME'], str(self.root / 'dotnet-home'))
        self.assertEqual(actual['DOTNET_CLI_HOME'], recorded['DOTNET_CLI_HOME'])

    def test_undeclared_source_cannot_disappear_during_reconstruction(self):
        (self.root / "Unlisted.cs").write_text("// must not silently drop this source")
        with self.assertRaisesRegex(RuntimeError, "Unmanifested"):
            source_inputs(self.project)

    def test_project_dependencies_are_never_silently_dropped(self):
        text = self.project.read_text().replace('</Project>', '<ItemGroup><PackageReference Include="ActualDependency" Version="1.0" /></ItemGroup></Project>')
        self.project.write_text(text)
        with self.assertRaisesRegex(RuntimeError, "explicitly declared"):
            source_inputs(self.project)

    def test_unknown_properties_fail_instead_of_changing_build_semantics(self):
        text = self.project.read_text().replace('</PropertyGroup>', '<DefineConstants>REQUIRED</DefineConstants></PropertyGroup>')
        self.project.write_text(text)
        with self.assertRaisesRegex(RuntimeError, "Unsupported"):
            source_inputs(self.project)

    def test_source_manifest_cannot_traverse_outside_project(self):
        (self.root / "Dotcc.SourceFiles.txt").write_text("../Other.cs\n")
        with self.assertRaisesRegex(RuntimeError, "exact flat"):
            source_inputs(self.project)

    def test_reserve_is_checked_before_allocation_and_cannot_be_lowered(self):
        output = self.root / "never-created"
        with patch('managed_test_storage.shutil.disk_usage') as usage:
            usage.return_value.free = 2 * 2**30 - 1
            with self.assertRaisesRegex(RuntimeError, "2GiB reserve"):
                require_storage_reserve([output])
        self.assertFalse(output.exists())
        with self.assertRaises(ValueError):
            require_storage_reserve([output], minimum_bytes=0)

    def test_removed_caches_are_historical_and_execution_inputs_still_verify(self):
        retired = self.root / "artifacts/obj/removed.dll"
        expected = {str(self.source): sha(self.source), str(retired): "historical-hash"}
        result = audit_build_caches(expected, [{"path": str(retired.parent), "rebuildable_bytes_removed": 1}],
                                    required={str(self.source): sha(self.source)})
        self.assertEqual(result['status'], 'verified-with-retired-caches')
        self.assertEqual(result['retired_sha256'], {str(retired): 'historical-hash'})
        self.assertFalse(retired.parent.exists())

    def test_missing_source_or_execution_file_is_not_covered_by_cleanup(self):
        required = {str(self.source): sha(self.source)}
        self.source.unlink()
        with self.assertRaises(FileNotFoundError):
            audit_build_caches(required, [{"path": str(self.root), "rebuildable_bytes_removed": 1}], required=required)

    def test_unrecorded_missing_cache_is_a_failure(self):
        missing = str(self.root / "artifacts/obj/missing.dll")
        for cleanup in ([], [{"path": str(self.root), "retained": "execution closure"}],
                        [{"path": str(self.root / "artifacts-other"), "rebuildable_bytes_removed": 1}]):
            with self.subTest(cleanup=cleanup), self.assertRaisesRegex(RuntimeError, "explicit cache retirement"):
                audit_build_caches({missing: 'hash'}, cleanup, required={})

    def test_rebuilt_cache_must_match_its_own_recorded_bytes(self):
        expected = {str(self.source): sha(self.source)}
        self.source.write_text('changed cache bytes')
        with self.assertRaisesRegex(RuntimeError, "changed"):
            audit_build_caches(expected, [{"path": str(self.root), "rebuildable_bytes_removed": 1}], required={})


class PreservedBuildAuditTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="preserved-build-audit-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name).resolve()
        self.original = self.root / "original"
        self.original.mkdir()
        self.output = self.root / "new-output"
        self.case = "synthetic_utility.c"
        self.rows, results, cleanup = [], [], []
        for form in ("raw", "processed"):
            for mode in ("jit", "aot"):
                base = self.original / (form + "-" + mode)
                source = base / "source"
                source.mkdir(parents=True)
                project = source / "Utility.csproj"
                project.write_text("<Project />")
                (source / "Utility.cs").write_text("// preserved source fixture")
                executable = base / "actual-output"
                executable.write_bytes(b"synthetic recorded output; never executed")
                command = ["never-run", str(executable)]
                outputs = {str(executable): sha(executable)}
                if mode == "jit":
                    dependency = base / preserved.DLL
                    dependency.write_bytes(b"synthetic dependency")
                    outputs[str(dependency)] = sha(dependency)
                identity = base / "identity.json"
                identity.write_text(json.dumps(dict(command=command, executable=str(executable),
                    executable_sha256=sha(executable), output_sha256=outputs,
                    runtime_closure_sha256={}, authored_source_sha256={},
                    managed_build_closure_sha256={str(base / "deleted-obj/cache.bin"): "historical"})))
                cleanup.append(dict(path=str(base / "deleted-obj"), rebuildable_bytes_removed=1))
                log = base / "actual.log"
                log.write_text("utility ... PASSED 3 tests\n")
                common = dict(case=self.case, form=form, mode=mode, command=command,
                    application_execution_receipt=str(identity), application_execution_receipt_sha256=sha(identity))
                self.rows.append(dict(common, status="built", project=str(project), execution_sha256=outputs,
                    source_sha256={p.name: sha(p) for p in source.iterdir()}))
                results.append(dict(common, status="passed", exit_code=0, assertions=3, native_assertions=3,
                                    log=str(log), log_sha256=sha(log)))
        self.manifest = self.original / "old-incomplete.json"
        self.manifest.write_text(json.dumps(dict(status="incomplete", executables=self.rows,
            case_failures={self.case: "old optional-storage failure"}, cache_cleanup={self.case: cleanup})))
        self.qualification = self.original / "qualification.json"
        self.qualification.write_text(json.dumps(dict(status="bounded-passed", counts=dict(passed=4, failed=0), cases=results)))
        self.prior = self.original / "prior.json"
        self.prior.write_text(json.dumps(dict(unchanged_expander_manifest=str(self.manifest),
            unchanged_expander_manifest_sha256=sha(self.manifest), actual_qualification=str(self.qualification),
            actual_qualification_sha256=sha(self.qualification), checked_sha256={})))
        self.original_hashes = {str(p): sha(p) for p in self.original.rglob("*") if p.is_file()}

    def audit(self):
        return preserved.audit(self.manifest, self.qualification, self.prior, self.case,
                               self.output, [self.output / "explicit-package-scope"])

    def test_new_caller_preserves_old_result_when_optional_package_storage_fails(self):
        with patch.object(preserved, "store_member", side_effect=RuntimeError("storage unavailable")) as store:
            report = self.audit()
        store.assert_called_once()
        self.assertEqual(report["status"], "passed-preserved-evidence")
        self.assertEqual(report["optional_storage"]["status"], "failed")
        self.assertEqual(report["historical_manifest_status"], "incomplete")
        self.assertFalse(report["canonical_adoption"])
        self.assertEqual(len(report["build_cache_inventory"]["retired_sha256"]), 4)
        self.assertEqual(report["executions_launched"], 0)
        self.assertEqual({str(p): sha(p) for p in self.original.rglob("*") if p.is_file()}, self.original_hashes)
        for row in report["staged_package_files"]:
            self.assertTrue(Path(row["store"]).is_relative_to(self.output / "explicit-package-scope"))

    def test_changed_actual_output_cannot_be_adopted(self):
        Path(next(iter(self.rows[0]["execution_sha256"]))).write_bytes(b"drift")
        with self.assertRaisesRegex(RuntimeError, "changed"):
            self.audit()
        self.assertFalse(self.output.exists())

    def test_wrong_previous_audit_is_rejected_before_staging(self):
        self.manifest.write_text(self.manifest.read_text() + "\n")
        with self.assertRaisesRegex(RuntimeError, "exact historical inputs"):
            self.audit()
        self.assertFalse(self.output.exists())

    def test_optional_storage_cannot_hide_drift_during_new_staging(self):
        def change_original(*args, **kwargs):
            Path(next(iter(self.rows[0]["execution_sha256"]))).write_bytes(b"drift during sharing")
            raise RuntimeError("optional storage failed")
        with patch.object(preserved, "store_member", side_effect=change_original):
            with self.assertRaisesRegex(RuntimeError, "changed"):
                self.audit()
        report = json.loads((self.output / "receipt.json").read_text())
        self.assertEqual(report["status"], "failed")


if __name__ == "__main__":
    unittest.main()

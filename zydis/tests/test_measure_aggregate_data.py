"""Receipt and report guards for paired measurements; never run any builds."""
import copy
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.recipes import helper


def emission(profile, optimized):
    return dict(profile=profile, aggregate_data=optimized, aggregate_data_threshold=1024,
                object_flags=["--aggregate-data"] if optimized else [])


class MeasurementTests(unittest.TestCase):
    def setUp(self):
        self.measure = helper(ROOT / "scripts/measure_aggregate_data.py")
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        self.inputs = {"zydis/src/table.inc": "current-source-digest"}
        # Current default is optimized, even when measuring historical receipts.
        self.qualification = SimpleNamespace(_inputs=Mock(side_effect=lambda ctx, selected: {
            **self.inputs, "campaign/emission": json.dumps(
                emission(ctx.profile, ctx.profile != "typed"), sort_keys=True)}))
        self.recipe = SimpleNamespace(native_configuration=Mock(return_value={}))
        helpers = patch.object(self.measure, "helper", side_effect=lambda path:
                               self.recipe if path.name == "campaign.py" else self.qualification)
        helpers.start()
        self.addCleanup(helpers.stop)

    def receipt(self, profile, optimized, *, legacy=False):
        identity = emission(profile, optimized)
        commands = [dict(label=f"{i:03d}-unit", status="passed", command=[
            "dotnet", f"/{profile}/tools/dotcc.dll", "-std=c11", *identity["object_flags"],
            "--override-report", f"/{profile}/{i}.jsonl", f"unit-{i}.c", "--emit=obj",
            "-o", f"/{profile}/{i}.cs"]) for i in range(18)]
        commands.append(dict(label="link", status="passed", command=[
            "dotnet", f"/{profile}/tools/dotcc.dll", *[f"/{profile}/{i}.cs" for i in range(18)],
            "--emit=managedlib", "-o", f"/{profile}/product"]))
        outputs = {}
        for form in ("raw", "processed"):
            product = self.root / profile / form / "product/TranslatedZydis.csproj"
            companion = product.parent.parent / "companion/TranslatedZydisQualification.csproj"
            product.parent.mkdir(parents=True)
            companion.parent.mkdir(parents=True)
            product.write_text("<Project />")
            companion.write_text("<Project />")
            (product.parent / "Zydis.cs").write_text("// measured source\n")
            self.measure.write_json(product.parent / "zydis-emission-profile.json", identity)
            inputs = self.inputs.copy()
            if not legacy:
                inputs["campaign/emission"] = json.dumps(identity, sort_keys=True)
            self.measure.write_json(companion.parent / "zydis-test-inputs.json", inputs)
            outputs[form] = dict(project=str(product), hashes={}, companions={
                "TranslatedZydisQualification": dict(project=str(companion))})
        receipt = dict(project="zydis", profile=profile, status="passed", outputs=outputs,
                       commands=commands, dotnet_sdk="10.0.100", tool_snapshots={
                           name: dict(executable="/tools/" + name, files={name + ".dll": "digest"})
                           for name in ("DotCC", "DotCC.PostProcess")},
                       zydis_selection=dict(emission=identity, native_flags=["-std=c11"], sources={}))
        path = self.root / (profile + ".json")
        self.measure.write_json(path, receipt)
        return path, receipt

    def pair(self, baseline="typed", optimized="default", *, legacy=False):
        return (self.receipt(baseline, False, legacy=legacy), self.receipt(optimized, True))

    def test_current_and_historical_pairs_follow_recorded_flags(self):
        for baseline, optimized, legacy in (("typed", "default", False),
                                            ("typed", "aggregate-data", False),
                                            ("default", "aggregate-data", False),
                                            ("default", "aggregate-data", True)):
            with self.subTest(baseline=baseline, optimized=optimized):
                # Each pairing gets independent current artifacts.
                with tempfile.TemporaryDirectory(dir=self.root) as directory:
                    previous = self.root
                    self.root = Path(directory)
                    try:
                        (left, _), (right, _) = self.pair(baseline, optimized, legacy=legacy)
                        receipts, products, identity = self.measure.paired_inputs(left, right, ("raw", "processed"))
                        self.assertEqual((baseline, optimized), tuple(receipts))
                        self.assertEqual(4, len(products))
                        self.assertNotIn("emission", identity["selection"])
                        self.assertEqual(18, len(identity["objects"]))
                    finally:
                        self.root = previous

    def test_two_optimized_profiles_cannot_be_an_unoptimized_baseline(self):
        left, _ = self.receipt("default", True)
        right, _ = self.receipt("aggregate-data", True)
        with self.assertRaisesRegex(RuntimeError, "Baseline.*unoptimized.*typed"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_optimized_input_requires_aggregate_emission(self):
        left, _ = self.receipt("default", False, legacy=True)
        right, _ = self.receipt("typed", False)
        with self.assertRaisesRegex(RuntimeError, "Optimized.*optimized emission"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_emission_metadata_must_be_explicit_and_consistent(self):
        _, receipt = self.receipt("default", True)
        for mutation in (None, {"object_flags": []}, {"aggregate_data": 1},
                         {"aggregate_data_threshold": 2048}, {"profile": "typed"}):
            with self.subTest(mutation=mutation):
                changed = copy.deepcopy(receipt)
                if mutation is None:
                    changed["zydis_selection"].pop("emission")
                else:
                    changed["zydis_selection"]["emission"].update(mutation)
                with self.assertRaisesRegex(RuntimeError, "emission selection"):
                    self.measure._recorded_emission(changed)

    def test_each_object_must_match_recorded_emission(self):
        (left, baseline), (right, optimized) = self.pair()
        for receipt in (baseline, optimized):
            identity = receipt["zydis_selection"]["emission"]
            self.assertEqual(18, len(self.measure._object_commands(receipt, identity)))
            changed = copy.deepcopy(receipt)
            command = changed["commands"][7]["command"]
            if identity["aggregate_data"]:
                command.remove("--aggregate-data")
            else:
                command.append("--aggregate-data")
            path = right if identity["aggregate_data"] else left
            self.measure.write_json(path, changed)
            with self.assertRaisesRegex(RuntimeError, "object-stage aggregate-data flag"):
                self.measure.paired_inputs(left, right, ("raw",))
            self.measure.write_json(path, receipt)
        optimized["commands"].pop(0)
        self.measure.write_json(right, optimized)
        with self.assertRaisesRegex(RuntimeError, "all 18"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_requested_object_command_is_checked(self):
        (left, _), (right, receipt) = self.pair()
        row = receipt["commands"][0]
        row["requested_command"] = [value for value in row["command"] if value != "--aggregate-data"]
        self.measure.write_json(right, receipt)
        with self.assertRaisesRegex(RuntimeError, "object-stage aggregate-data flag"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_pairs_must_still_match_beyond_emission(self):
        (left, _), (right, receipt) = self.pair()
        changes = (("sdk", lambda row: row.update(dotnet_sdk="different")),
                   ("tools", lambda row: row["tool_snapshots"]["DotCC"]["files"].update(other="digest")),
                   ("selection", lambda row: row["zydis_selection"]["native_flags"].append("-O2")),
                   ("object", lambda row: row["commands"][0]["command"].append("-O2")),
                   ("link", lambda row: row["commands"][-1]["command"].append("--literal-pool")))
        for name, mutate in changes:
            with self.subTest(name=name):
                changed = copy.deepcopy(receipt)
                mutate(changed)
                self.measure.write_json(right, changed)
                with self.assertRaisesRegex(RuntimeError, "Paired receipts differ"):
                    self.measure.paired_inputs(left, right, ("raw",))

    def test_only_historical_unoptimized_default_can_omit_companion_emission(self):
        (left, baseline), (right, optimized) = self.pair()
        for receipt in (baseline, optimized):
            stamp = Path(receipt["outputs"]["raw"]["companions"]["TranslatedZydisQualification"]["project"]).parent / "zydis-test-inputs.json"
            inputs = self.measure.read_json(stamp)
            self.measure.write_json(stamp, self.inputs)
            with self.assertRaisesRegex(RuntimeError, "Companion inputs/profile differ"):
                self.measure.paired_inputs(left, right, ("raw",))
            self.measure.write_json(stamp, inputs)

    def test_historical_companions_still_require_current_source_bytes(self):
        (left, receipt), (right, _) = self.pair("default", "aggregate-data", legacy=True)
        stamp = Path(receipt["outputs"]["raw"]["companions"]["TranslatedZydisQualification"]["project"]).parent / "zydis-test-inputs.json"
        self.measure.write_json(stamp, {"zydis/src/table.inc": "stale-source-digest"})
        with self.assertRaisesRegex(RuntimeError, "Companion inputs/profile differ"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_product_stamp_and_recorded_hashes_are_checked(self):
        (left, _), (right, receipt) = self.pair()
        product = Path(receipt["outputs"]["raw"]["project"])
        stamp = product.parent / "zydis-emission-profile.json"
        self.measure.write_json(stamp, emission("default", False))
        with self.assertRaisesRegex(RuntimeError, "profile stamp differs"):
            self.measure.paired_inputs(left, right, ("raw",))
        self.measure.write_json(stamp, emission("default", True))
        receipt["outputs"]["raw"]["hashes"] = {"Zydis.cs": "different-digest"}
        self.measure.write_json(right, receipt)
        with self.assertRaisesRegex(RuntimeError, "output differs from supplied receipt"):
            self.measure.paired_inputs(left, right, ("raw",))

    def test_cli_supports_role_options_and_historical_aliases(self):
        for baseline_option, optimized_option in (("--baseline-receipt", "--optimized-receipt"),
                                                  ("--default-receipt", "--aggregate-receipt")):
            with self.subTest(baseline_option=baseline_option), patch.object(self.measure, "compare") as compare:
                with patch.object(sys, "argv", ["measure", "compare", baseline_option, "baseline.json",
                                               optimized_option, "optimized.json", "--output", "measurement"]):
                    self.assertEqual(0, self.measure.main())
                args = compare.call_args.args[0]
                self.assertEqual(Path("baseline.json"), args.baseline_receipt)
                self.assertEqual(Path("optimized.json"), args.optimized_receipt)

    def test_report_uses_actual_profiles_and_optimized_minus_baseline(self):
        (left, _), (right, _) = self.pair()
        args = SimpleNamespace(baseline_receipt=left, optimized_receipt=right, output=self.root / "report",
                               form="raw", mode="jit", rid="linux-x64", build_samples=2, init_samples=1)
        def metrics(project):
            optimized = "default" in project.parts
            copies = [dict(offset=offset, bytes=min(4096, self.measure.TABLE_BYTES - offset))
                      for offset in range(0, self.measure.TABLE_BYTES, 4096)] if optimized else []
            return dict(source_bytes=50 if optimized else 100, encoder_field_store_lines=0 if optimized else 1,
                        encoder_field_store_source_bytes=0, payload_bytes=0, aggregate_initialized_bytes=0,
                        encoder_payload_copy_bytes=0, encoder_copies=copies)

        def sample(report, product, *args):
            optimized = "default" in product.parts
            return dict(build_seconds=5 if optimized else 10, init_seconds=[1 if optimized else 2],
                        process_seconds=[3 if optimized else 4])

        with patch.object(self.measure.sys, "platform", "linux"), \
                patch.object(self.measure.platform, "machine", return_value="x86_64"), \
                patch.object(self.measure, "run") as run, \
                patch.object(self.measure, "native_table", return_value=b"native"), \
                patch.object(self.measure, "source_metrics", side_effect=metrics), \
                patch.object(self.measure, "sample", side_effect=sample) as samples, \
                patch("builtins.print"):
            self.measure.compare(args)
        report = self.measure.read_json(args.output / "report.json")
        self.assertEqual("passed", report["status"])
        self.assertEqual(2, report["schema_version"])
        self.assertEqual("typed", report["comparison"]["baseline"])
        self.assertEqual("default", report["comparison"]["optimized"])
        self.assertEqual(emission("default", True), report["comparison"]["emissions"]["default"])
        self.assertEqual({"typed", "default"}, set(report["receipts"]))
        self.assertEqual(-50, report["source_differences"]["raw"]["source_bytes"])
        self.assertEqual(-5, report["median_differences"][0]["optimized_minus_baseline"]["build_seconds"])
        self.assertEqual(["typed", "default", "default", "typed"],
                         [call.args[1].parents[2].name for call in samples.call_args_list])
        self.assertEqual(1, run.call_count)  # dotnet-info is mocked, as are every build and native probe.


if __name__ == "__main__":
    unittest.main()

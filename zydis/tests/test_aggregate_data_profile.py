"""No managed builds: profile isolation, flag placement and measurement guards."""
import copy
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.layout import Layout
from campaigns.recipes import helper


class AggregateProfileTests(unittest.TestCase):
    def test_product_audit_retains_legacy_default_and_rejects_profile_substitution(self):
        audit = helper(ROOT / "scripts/audit.py")
        recipe = helper(ROOT / "scripts/campaign.py")
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory) / "TranslatedZydis.csproj"
            # The standalone historical preliminary audit has no recipe/profile.
            context = SimpleNamespace(root=ROOT)
            audit._check_emission_profile(context, project)
            context.profile = "aggregate-data"
            with self.assertRaises(RuntimeError):
                audit._check_emission_profile(context, project)
            (project.parent / "zydis-emission-profile.json").write_text(
                json.dumps(recipe.emission_identity("aggregate-data")))
            audit._check_emission_profile(context, project)
            context.profile = "default"
            with self.assertRaises(RuntimeError):
                audit._check_emission_profile(context, project)

    def test_complete_product_selects_option_at_object_emission_only(self):
        recipe = helper(ROOT / "scripts/campaign.py")
        source_paths = {source.name: source.directory for source in recipe.sources(ROOT)}
        baseline = None
        for profile in ("default", "aggregate-data"):
            ctx = SimpleNamespace(root=ROOT, sources=source_paths, receipt={}, profile=profile)
            spec = recipe.prepare(ctx)
            self.assertEqual(18, len(spec.units))
            self.assertEqual(profile == "aggregate-data", "--aggregate-data" in spec.flags)
            self.assertNotIn("--aggregate-data", spec.link_flags)
            self.assertNotIn("--aggregate-data", ctx.receipt["zydis_selection"]["native_flags"])
            if baseline is None:
                baseline = spec
            else:
                self.assertEqual(baseline.units, spec.units)
                self.assertEqual(baseline.flags, [flag for flag in spec.flags if flag != "--aggregate-data"])
                self.assertEqual(baseline.link_flags, spec.link_flags)
        self.assertEqual("default", recipe.recipe.default_profile)
        layout = Layout(ROOT, "TranslatedZydisQualification", "default")
        self.assertNotEqual(layout.project("default"), layout.project("aggregate-data"))

    def test_wrong_or_partial_object_emission_cannot_be_measured(self):
        measure = helper(ROOT / "scripts/measure_aggregate_data.py")
        receipt = {"commands": [dict(label=f"{i:03d}-unit", status="passed", command=[
            "dotnet", "/tools/dotcc.dll", "--aggregate-data", f"unit-{i}.c", "--emit=obj", "-o", f"{i}.cs"
        ]) for i in range(18)]}
        self.assertEqual(18, len(measure._object_commands(receipt, "aggregate-data")))
        with self.assertRaises(RuntimeError):
            measure._object_commands(receipt, "default")
        missing_flag = copy.deepcopy(receipt)
        missing_flag["commands"][7]["command"].remove("--aggregate-data")
        with self.assertRaises(RuntimeError):
            measure._object_commands(missing_flag, "aggregate-data")
        receipt["commands"].pop()
        with self.assertRaises(RuntimeError):
            measure._object_commands(receipt, "aggregate-data")

    def test_source_metrics_separate_field_stores_from_payload_bytes(self):
        measure = helper(ROOT / "scripts/measure_aggregate_data.py")
        source = (b"encoder_instructions_destination[0].field = 2;\n"
                  b"other_destination[0].field = 3;\n"
                  b"internal static global::System.ReadOnlySpan<byte> B123 => [0x01, 0x02];\n"
                  b"internal static global::System.ReadOnlySpan<byte> B456 => [0x03, 0x04, 0x05, 0x06];\n"
                  b"internal static global::System.ReadOnlySpan<uint> Puint_123 => [0x01020304U];\n"
                  b"// aggregate-data: 6 bytes\n"
                  b"DotCcLiterals.B123.CopyTo(new global::System.Span<byte>((byte*)(encoder_instructions_destination), 2));\n"
                  b"DotCcLiterals.B456.CopyTo(new global::System.Span<byte>((byte*)(encoder_instructions_destination) + 2, 4));\n")
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Zydis.cs").write_bytes(source)
            (root / "Dotcc.SourceFiles.txt").write_text("Zydis.cs\n")
            project = root / "TranslatedZydis.csproj"
            project.write_text('<Project><PropertyGroup><OutputType>Library</OutputType></PropertyGroup></Project>')
            metrics = measure.source_metrics(project)
            self.assertEqual(len(source), metrics["source_bytes"])
            self.assertEqual(1, metrics["encoder_field_store_lines"])
            self.assertEqual(len(source.splitlines(keepends=True)[0]), metrics["encoder_field_store_source_bytes"])
            self.assertEqual(10, metrics["payload_bytes"])
            self.assertEqual(3, metrics["payload_declarations"])
            self.assertEqual(1, metrics["aggregate_initializers"])
            self.assertEqual(6, metrics["aggregate_initialized_bytes"])
            self.assertEqual(2, metrics["encoder_payload_copy_calls"])
            self.assertEqual(6, metrics["encoder_payload_copy_bytes"])
            self.assertEqual(6, metrics["encoder_payload_bytes"])


if __name__ == "__main__":
    unittest.main()

"""Qualification identity must include data consumed by upstream C includes."""
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.recipes import helper


class InputIdentityTests(unittest.TestCase):
    def test_generated_table_edit_invalidates_companion_identity(self):
        runner = helper(ROOT / "scripts/qualification.py")
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name in ("tests/qualification.c", "tests/harness.py", "config/translation.json"):
                path = root / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("fixture\n")
            source = root / "upstream"
            table = source / "src/Generated/DecoderTables.inc"
            table.parent.mkdir(parents=True)
            table.write_text("const int table[] = { 1 };\n")
            selected = {"sources": {"zydis": source, "zycore": root / "zycore"}}
            context = SimpleNamespace(root=root)
            original = runner._inputs(context, selected)
            table.write_text("const int table[] = { 2 };\n")
            changed = runner._inputs(context, selected)
            self.assertNotEqual(original, changed)
            self.assertIn("zydis/src/Generated/DecoderTables.inc", changed)

    def test_every_profile_records_current_emission_identity(self):
        runner = helper(ROOT / "scripts/qualification.py")
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name in ("tests/qualification.c", "tests/harness.py", "config/translation.json"):
                path = root / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("fixture\n")
            selected = {"sources": {}}
            default = runner._inputs(SimpleNamespace(root=root, profile="default"), selected)
            optimized = runner._inputs(SimpleNamespace(root=root, profile="aggregate-data"), selected)
            self.assertNotEqual(default, optimized)
            typed = runner._inputs(SimpleNamespace(root=root, profile="typed"), selected)
            for identity in (default, optimized, typed):
                self.assertIn("campaign/emission", identity)
            self.assertTrue(json.loads(default["campaign/emission"])["aggregate_data"])
            self.assertTrue(json.loads(optimized["campaign/emission"])["aggregate_data"])
            self.assertFalse(json.loads(typed["campaign/emission"])["aggregate_data"])
            self.assertEqual({k: v for k, v in default.items() if k != "campaign/emission"},
                             {k: v for k, v in typed.items() if k != "campaign/emission"})

    def test_stale_default_companion_rejected_with_hashes_off(self):
        runner = helper(ROOT / "scripts/qualification.py")
        recipe = helper(ROOT / "scripts/campaign.py")
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            product = root / "generated/TranslatedZydisQualification/TranslatedZydisQualification.csproj"
            product.parent.mkdir(parents=True)
            product.touch()
            context = SimpleNamespace(root=root, profile="default", recipe=recipe.recipe,
                                      options=SimpleNamespace(hashes="off"))
            current = {"campaign/emission": json.dumps(recipe.emission_identity("default"), sort_keys=True)}
            old = recipe.emission_identity("default")
            old.update(aggregate_data=False, object_flags=[])
            for stale in ({}, {"campaign/emission": json.dumps(old, sort_keys=True)}):
                (product.parent / "zydis-test-inputs.json").write_text(json.dumps(stale))
                with (patch.object(runner, "check_product"),
                      patch.object(runner, "_configuration", return_value={}),
                      patch.object(runner, "_inputs", return_value=current)):
                    with self.assertRaisesRegex(RuntimeError, "regenerate"):
                        runner._managed(context, "processed", "jit")


if __name__ == "__main__":
    unittest.main()

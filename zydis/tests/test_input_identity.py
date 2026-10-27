"""Qualification identity must include data consumed by upstream C includes."""
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest

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

    def test_optional_emission_profile_cannot_reuse_default_companion(self):
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
            self.assertNotIn("campaign/emission", default)
            self.assertIn("campaign/emission", optimized)
            self.assertEqual(default, {key: value for key, value in optimized.items() if key != "campaign/emission"})


if __name__ == "__main__":
    unittest.main()

"""Authored-source isolation and portable project publication contracts."""
import importlib.util
from pathlib import Path
import shutil
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
spec = importlib.util.spec_from_file_location("cui_recipe_contract", ROOT / "scripts/campaign.py")
recipe = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recipe)


class RecipeContracts(unittest.TestCase):
    def test_processing_copies_authored_sources_and_publication_retargets_all_links(self):
        with tempfile.TemporaryDirectory() as temporary:
            base = Path(temporary)
            root, work, source = base / "campaign", base / "work", base / "upstream"
            host = root / "src/Host/Host.cs"
            host.parent.mkdir(parents=True)
            host.write_text("// authored host\n")
            glfw = root / "src/Glfw/Glfw.csproj"
            glfw.parent.mkdir(parents=True)
            glfw.write_text('<Project Sdk="Microsoft.NET.Sdk" />')
            source.mkdir()
            (source / "assets/fonts").mkdir(parents=True)
            for name in ("LICENSE", "THIRD_PARTY.md"):
                (source / name).write_text("notice\n")
            (glfw.parent / "LICENSE.md").write_text("GLFW notice\n")
            project = work / "processed/TranslatedCui/TranslatedCui.csproj"
            project.parent.mkdir(parents=True)
            project.write_text('<Project Sdk="Microsoft.NET.Sdk" />')
            ctx = SimpleNamespace(root=root, work=work, sources={"product": source})
            # Model relpath's different-Windows-drive failure for private links.
            with patch.object(recipe.os.path, "relpath", side_effect=ValueError("different drives")):
                recipe.configure(ctx, project, "processed")
            private = ET.parse(project).find(".//Compile").get("Include")
            self.assertNotEqual(Path(private), host)
            Path(private).write_text("// processed private host\n")
            self.assertEqual(host.read_text(), "// authored host\n")
            final = root / "generated/TranslatedCui/TranslatedCui.csproj"
            final.parent.mkdir(parents=True)
            shutil.copyfile(project, final)
            recipe.finish(ctx, final, "processed")
            tree = ET.parse(final)
            compile_link = tree.find(".//Compile").get("Include")
            project_link = tree.find(".//ProjectReference").get("Include")
            self.assertFalse(Path(compile_link).is_absolute())
            self.assertFalse(Path(project_link).is_absolute())
            self.assertEqual((final.parent / compile_link).resolve(), host)
            self.assertEqual((final.parent / project_link).resolve(), glfw)
            self.assertEqual(host.read_text(), "// authored host\n")


if __name__ == "__main__":
    unittest.main()

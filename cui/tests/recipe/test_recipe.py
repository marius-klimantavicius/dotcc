"""Authored-source isolation and portable project publication contracts."""
import importlib.util
import json
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
    def test_core_gallery_and_qualification_share_cui_only_deduplication_policy(self):
        codegen = recipe.helper(ROOT / "scripts/codegen.py")
        from campaigns.recipes import link_flags
        # The shared default remains inline-only for unrelated campaigns.
        self.assertNotIn("--deduplicate-static", link_flags("Other", "Other"))
        with tempfile.TemporaryDirectory() as temporary:
            work = Path(temporary)
            ctx = SimpleNamespace(root=ROOT, work=work, sources={"product": work}, receipt={}, recipe=recipe.recipe)
            original = recipe.helper
            with patch.object(recipe, "helper", side_effect=lambda path:
                              SimpleNamespace(stage=lambda *_: []) if path.name == "stage.py" else original(path)):
                core = recipe.prepare(ctx)
            for flag in ("--deduplicate-inline", "--deduplicate-static"):
                self.assertEqual(core.link_flags.count(flag), 1)
            self.assertIn("--export-managed-symbols", core.link_flags)
            config = json.loads((ROOT / "config/gallery.json").read_text())
            for name in [*config["sources"], *config["headers"], "LICENSE", "THIRD_PARTY.md"]:
                path = work / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("// upstream fixture\n")
            commands = []

            def run(command, label, **_):
                commands.append((label, command))
                if label == "gallery-link":
                    output = Path(command[-1])
                    output.mkdir(parents=True)
                    (output / "TranslatedCuiGallery.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk" />')

            ctx.run, ctx.compiler, ctx.options = run, work / "dotcc.dll", SimpleNamespace(timeout=300)
            core.companions(ctx)
            command = next(command for label, command in commands if label == "gallery-link")
            for flag in ("--deduplicate-inline", "--deduplicate-static"):
                self.assertEqual(command.count(flag), 1)
            self.assertIn("--managed-reference", command)
            self.assertEqual(command[command.index("--managed-reference") + 1],
                             work / "raw/TranslatedCui/Dotcc.ManagedSymbols.cs")
            qualification = recipe.helper(ROOT / "scripts/qualify.py")
            runner = object.__new__(qualification.Run)
            runner.work, runner.report = work, {}
            runner.args = SimpleNamespace(compiler=ctx.compiler)
            runner.emit = lambda *_: work / "main.cs"

            class Linked(Exception):
                pass

            def link(command, label):
                commands.append((label, command))
                raise Linked()

            runner.run = link
            with self.assertRaises(Linked):
                runner.build("probe", work, [], ("examples/gallery_ui.c", "inflate"))
            command = next(command for label, command in commands if label == "probe/link")
            for flag in ("--deduplicate-inline", "--deduplicate-static"):
                self.assertEqual(command.count(flag), 1)
            # A private profile exercises the option instead of hardcoding it.
            (work / "config").mkdir()
            (work / "config/profile.json").write_text(json.dumps({"deduplication": {"inline": True, "static": False}}))
            self.assertNotIn("--deduplicate-static", codegen.link_flags(work, "Cui", "Managed.Cui"))

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

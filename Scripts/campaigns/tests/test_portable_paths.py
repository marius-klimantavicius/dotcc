"""Portable solution and ownership metadata, without running translation tools."""
import json
from pathlib import Path, PureWindowsPath
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.delivery import promote, source_files
from campaigns.identity import HashPolicy, write_json
from campaigns.layout import Layout, check, scaffold
from campaigns.model import Recipe
from campaigns.translation import deliver


class WindowsRelativePath(type(Path())):
    """Use real filesystem I/O, but calculate Windows-style relative paths."""
    def relative_to(self, *args, **kwargs):
        return PureWindowsPath(super().relative_to(*args, **kwargs).as_posix())


def windows_source_files(directory):
    return [WindowsRelativePath(path) for path in source_files(directory)]


class PortablePathTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="portable campaign ")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.recipe = Recipe("fixture", "TranslatedFixture", ("default",),
                             ("samples/ManagedConsumer/ManagedConsumer.csproj",), lambda root: ())
        self.layout = Layout(self.root, self.recipe.product, "default")

    def write(self, name, contents):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(contents)
        return path

    def windows_project(self):
        project = Layout.project
        return patch.object(Layout, "project", lambda layout, *args, **kwargs:
                            WindowsRelativePath(project(layout, *args, **kwargs)))

    def solution_paths(self):
        return [node.get("Path") for node in ET.parse(self.layout.solution).iter("Project")]

    def test_forward_slash_solution_matches_calculated_windows_project(self):
        self.write(self.recipe.projects[0], "<Project />")
        scaffold(self.layout, self.recipe)
        with self.windows_project():
            self.assertEqual(check(self.layout, self.recipe), [])

    def test_scaffold_writes_portable_paths_from_windows_project(self):
        with self.windows_project():
            scaffold(self.layout, self.recipe)
        self.assertEqual(self.solution_paths(),
                         ["generated/TranslatedFixture/TranslatedFixture.csproj", *self.recipe.projects])

    def test_scaffold_is_idempotent_for_either_solution_separator(self):
        self.write(self.recipe.projects[0], "<Project />")
        expected = ["generated/TranslatedFixture/TranslatedFixture.csproj", *self.recipe.projects]
        for separator in ("/", "\\"):
            with self.subTest(separator=separator):
                solution = ET.Element("Solution")
                for name in expected:
                    ET.SubElement(solution, "Project", Path=name.replace("/", separator))
                ET.ElementTree(solution).write(self.layout.solution, encoding="unicode")
                with self.windows_project():
                    scaffold(self.layout, self.recipe)
                    first = self.layout.solution.read_bytes()
                    scaffold(self.layout, self.recipe)
                    self.assertEqual(check(self.layout, self.recipe), [])
                self.assertEqual(self.layout.solution.read_bytes(), first)
                self.assertEqual(self.solution_paths(), [name.replace("/", separator) for name in expected])

    def test_owned_nested_files_replace_and_remove_across_separator_styles(self):
        for separator in ("/", "\\"):
            with self.subTest(separator=separator):
                base = self.root / ("forward" if separator == "/" else "backward")
                candidate, target = base / "candidate", base / "target"
                for directory, value in ((candidate, "new"), (target, "old")):
                    self.write(directory / "upstream/provenance.json", value)
                    write_json(directory / ".campaign-files.json",
                               [name.replace("/", separator) for name in
                                ("upstream/provenance.json", "upstream/obsolete.json")])
                self.write(target / "upstream/obsolete.json", "old generated data")
                self.write(target / "caller/notes.txt", "caller data")
                with patch("campaigns.delivery.source_files", windows_source_files):
                    promote([(candidate, target)], base / "journal.json", lambda: None)
                self.assertEqual((target / "upstream/provenance.json").read_text(), "new")
                self.assertFalse((target / "upstream/obsolete.json").exists())
                self.assertEqual((target / "caller/notes.txt").read_text(), "caller data")

    def test_legacy_manifest_and_globs_own_nested_windows_paths(self):
        for separator in ("/", "\\"):
            with self.subTest(separator=separator):
                base = self.root / ("forward" if separator == "/" else "backward")
                candidate, target = base / "candidate", base / "target"
                for directory, value in ((candidate, "new"), (target, "old")):
                    for name in ("Sources/Generated.cs", "Sources/Legacy.cs", "Product.csproj"):
                        self.write(directory / name, value)
                self.write(target / "Sources/obsolete.cs", "old generated data")
                self.write(target / "Dotcc.SourceFiles.txt", "Sources" + separator + "Generated.cs\n")
                self.write(target / "caller/notes.txt", "caller data")
                with patch("campaigns.delivery.source_files", windows_source_files):
                    promote([(candidate, target)], base / "journal.json", lambda: None,
                            legacy_owned=("Sources/Legacy.cs", "Sources/obsolete.cs"))
                for name in ("Sources/Generated.cs", "Sources/Legacy.cs", "Product.csproj"):
                    self.assertEqual((target / name).read_text(), "new")
                self.assertFalse((target / "Sources/obsolete.cs").exists())
                self.assertEqual((target / "caller/notes.txt").read_text(), "caller data")

    def test_differing_unowned_nested_file_still_blocks_publication(self):
        candidate, target = self.root / "candidate", self.root / "target"
        self.write(candidate / "upstream/provenance.json", "new generated data")
        self.write(target / "upstream/provenance.json", "caller data")
        write_json(target / ".campaign-files.json", ["upstream/other.json"])
        journal = self.root / "journal.json"
        with patch("campaigns.delivery.source_files", windows_source_files):
            with self.assertRaisesRegex(RuntimeError, "Unowned generated-file conflict"):
                promote([(candidate, target)], journal, lambda: self.fail("Must not validate"))
        self.assertEqual((target / "upstream/provenance.json").read_text(), "caller data")
        self.assertEqual((candidate / "upstream/provenance.json").read_text(), "new generated data")
        self.assertFalse(journal.exists())

    def test_identical_unowned_nested_file_can_be_published(self):
        candidate, target = self.root / "candidate", self.root / "target"
        for directory in (candidate, target):
            self.write(directory / "caller/notes.txt", "shared data")
        with patch("campaigns.delivery.source_files", windows_source_files):
            promote([(candidate, target)], self.root / "journal.json", lambda: None)
        self.assertEqual((target / "caller/notes.txt").read_text(), "shared data")

    def product(self, name):
        directory = self.root / "work/raw" / name
        self.write(directory / (name + ".csproj"), "<Project />")
        self.write(directory / "Generated.cs", "// generated")
        self.write(directory / "Dotcc.SourceFiles.txt", "Generated.cs\n")
        self.write(directory / "upstream/provenance.json", '{"kind": "fixture"}')
        return directory

    def assert_portable_delivery(self, with_companion):
        main = self.recipe.product
        raw = self.product(main)
        companions = {"TranslatedCompanion": self.product("TranslatedCompanion")} if with_companion else {}
        ctx = SimpleNamespace(
            root=self.root, work=self.root / "work", artifacts=self.root / "artifacts/run",
            recipe=self.recipe, profile="default", layout=self.layout,
            options=SimpleNamespace(form="all"), policy=HashPolicy("off"), receipt={"outputs": {}},
            postprocessor=self.root / "postprocessor.dll", managed=lambda *args: None,
            restore=lambda *args: None, run=lambda *args: None, save=lambda: None)
        with patch("campaigns.translation.source_files", windows_source_files):
            deliver(ctx, raw, companions=companions)
        for name in (main, *companions):
            for form in ("raw", "processed"):
                with self.subTest(product=name, form=form):
                    directory = Layout(self.root, name, "default").directory(form=form)
                    owned = json.loads((directory / ".campaign-files.json").read_text())
                    self.assertEqual(set(owned), {name + ".csproj", "Generated.cs", "Dotcc.SourceFiles.txt",
                                                  "upstream/provenance.json"})
                    self.assertTrue(all("\\" not in path for path in owned))
                    self.assertEqual(ctx.receipt["outputs"][form]["project"], str(self.layout.project(form=form)))

    def test_main_product_manifests_use_forward_slashes(self):
        self.assert_portable_delivery(with_companion=False)

    def test_main_and_companion_manifests_use_forward_slashes(self):
        self.assert_portable_delivery(with_companion=True)


if __name__ == "__main__":
    unittest.main()

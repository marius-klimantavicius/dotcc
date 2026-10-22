"""Related-library delivery contracts; dotnet is an isolated runner fixture."""
from contextlib import redirect_stderr
import io
import json
from pathlib import Path
import shutil
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.delivery import source_files
from campaigns.identity import HashPolicy, digest, write_json
from campaigns.layout import Layout
from campaigns.model import Recipe
from campaigns.translation import check_product, deliver


class CompanionDeliveryTests(unittest.TestCase):
    main = "TranslatedBackend"
    companion = "TranslatedFrontend"

    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="related campaign ")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.authored = self.write("src/Host.cs", "// original authored source\n")
        recipe = Recipe("fixture", self.main, ("default", "alternate"), (), lambda root: ())
        self.ctx = SimpleNamespace(
            root=self.root, work=self.root / "work", artifacts=self.root / "artifacts/run",
            profile="default", recipe=recipe, layout=Layout(self.root, self.main, "default"),
            options=SimpleNamespace(form="all"), policy=HashPolicy("strict"),
            postprocessor=self.root / "fixture-semantic-runner.dll", receipt={"outputs": {}},
            managed=self.managed, restore=self.restore, run=self.semantic_run, save=self.save)
        self.calls = []
        self.hooks = []
        self.fail_build = None
        self.raw = self.output(self.root / "work/raw" / self.main, self.main, "new backend", authored=True)
        self.frontend = self.output(self.root / "work/raw" / self.companion, self.companion, "new frontend")

    def write(self, name, value):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(value, encoding="utf-8")
        return path

    def output(self, directory, name, value, authored=False):
        directory.mkdir(parents=True, exist_ok=True)
        (directory / "Generated.cs").write_text(value, encoding="utf-8")
        (directory / "Dotcc.SourceFiles.txt").write_text("Generated.cs\n", encoding="utf-8")
        project = ET.Element("Project")
        if authored:
            ET.SubElement(ET.SubElement(project, "ItemGroup"), "Compile",
                          Include=str(self.authored), Link="Host.cs")
        ET.ElementTree(project).write(directory / (name + ".csproj"), encoding="unicode")
        write_json(directory / ".campaign-files.json",
                   [str(path.relative_to(directory)) for path in source_files(directory)])
        return directory

    def managed(self, action, project, label):
        self.calls.append((label, Path(project)))
        self.assertEqual(action, "build")
        self.assertTrue(Path(project).is_file())
        if label == self.fail_build:
            raise RuntimeError("last companion build failed")
        # Build caches must not become delivered inputs or output hashes.
        for directory in ("bin", "obj"):
            self.write(str(Path(project).parent.relative_to(self.root) / directory / "cache.txt"), label)

    def restore(self, project, label):
        self.calls.append((label, Path(project)))

    def semantic_run(self, command, label, timeout):
        self.calls.append((label, Path(command[2])))
        self.assertEqual(command[:2], ["dotnet", self.ctx.postprocessor])
        self.assertEqual(command[3:], ["--in-place"])
        self.assertEqual(timeout, 900)
        project = Path(command[2])
        self.assertTrue(project.is_relative_to(self.ctx.work / "processed"))
        for node in ET.parse(project).iter("Compile"):
            source = (project.parent / node.get("Include")).resolve()
            # A semantic runner may rewrite every Compile input. Only private
            # authored copies may be passed to that runner.
            self.assertTrue(source.is_relative_to(self.ctx.work / "authored"))
            self.assertNotEqual(source, self.authored)
            source.write_text("// fixture semantic rewrite\n", encoding="utf-8")
        with (project.parent / "Generated.cs").open("a", encoding="utf-8") as stream:
            stream.write("\n// processed")

    def save(self):
        self.calls.append(("save", None))
        write_json(self.ctx.artifacts / "receipt.json", self.ctx.receipt)

    def configure(self, ctx, project, form):
        self.hooks.append(("configure", project, form))
        self.assertEqual(project.name, self.main + ".csproj")
        destination = ctx.work / "authored" / form / self.authored.name
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(self.authored, destination)
        tree = ET.parse(project)
        next(tree.iter("Compile")).set("Include", str(destination))
        tree.write(project, encoding="unicode")

    def finish(self, ctx, project, form):
        self.hooks.append(("finish", project, form))
        self.assertEqual(project.name, self.main + ".csproj")
        tree = ET.parse(project)
        next(tree.iter("Compile")).set("Include", str(self.authored))
        tree.write(project, encoding="unicode")

    def deliver(self, companions=None):
        deliver(self.ctx, self.raw, self.configure, self.finish,
                {self.companion: self.frontend} if companions is None else companions)

    def layout(self, name):
        return Layout(self.root, name, "default")

    @property
    def pointer(self):
        return self.root / "artifacts/campaign" / ("current-" + self.ctx.profile + ".json")

    def snapshot(self, directory):
        return {str(path.relative_to(directory)): path.read_bytes()
                for path in directory.rglob("*") if path.is_file()}

    def test_four_products_processed_and_published_with_current_hashes(self):
        self.deliver()
        current = json.loads(self.pointer.read_text())
        self.assertEqual(current["outputs"], self.ctx.receipt["outputs"])
        self.assertEqual(current["receipt"], str(self.ctx.artifacts / "receipt.json"))
        self.assertEqual(json.loads((self.ctx.artifacts / "receipt.json").read_text()), self.ctx.receipt)
        for form in ("raw", "processed"):
            records = current["outputs"][form]
            for name, record in ((self.main, records), (self.companion, records["companions"][self.companion])):
                directory = self.layout(name).directory(self.ctx.profile, form)
                self.assertEqual(record["project"], str(directory / (name + ".csproj")))
                self.assertTrue(record["hashes"])
                for relative, expected in record["hashes"].items():
                    self.assertEqual(digest(directory / relative), expected)
                self.assertEqual("// processed" in (directory / "Generated.cs").read_text(), form == "processed")
                owned = json.loads((directory / ".campaign-files.json").read_text())
                for relative in (*record["hashes"], *owned):
                    self.assertFalse({"bin", "obj"}.intersection(Path(relative).parts))
            self.assertEqual(check_product(self.ctx, form), self.ctx.layout.project(self.ctx.profile, form))
        self.assertFalse((self.root / "build/campaign/promotion.json").exists())

    def test_last_companion_validation_rolls_back_all_four_and_prior_receipt(self):
        previous = {}
        for name in (self.main, self.companion):
            for form in ("raw", "processed"):
                directory = self.output(self.layout(name).directory(self.ctx.profile, form), name, "old " + name + form)
                self.write(str(directory.relative_to(self.root) / "caller-notes.txt"), "preserve caller file")
                previous[directory] = self.snapshot(directory)
        write_json(self.pointer, {"receipt": "prior successful receipt", "outputs": {"old": True}})
        pointer = self.pointer.read_bytes()
        prior_receipt = self.write("artifacts/prior/receipt.json", '{"status": "passed"}\n')
        receipt_bytes = prior_receipt.read_bytes()
        self.fail_build = "final-" + self.companion + "-processed-build"
        with self.assertRaisesRegex(RuntimeError, "last companion build failed"):
            self.deliver()
        self.assertEqual(self.calls[-1][0], self.fail_build)
        for directory, before in previous.items():
            self.assertEqual(self.snapshot(directory), before)
        self.assertEqual(self.pointer.read_bytes(), pointer)
        self.assertEqual(prior_receipt.read_bytes(), receipt_bytes)
        self.assertFalse((self.ctx.artifacts / "receipt.json").exists())
        self.assertFalse((self.root / "build/campaign/promotion.json").exists())

    def test_bad_companion_name_rejected_before_any_mutation(self):
        before = self.snapshot(self.root)
        for name in ("", "../escape", "nested/name", "nested\\name", "/absolute", ".", self.main):
            with self.subTest(name=name):
                with self.assertRaises(ValueError):
                    self.deliver({self.companion: self.frontend, name: self.frontend})
                self.assertEqual(self.calls, [])
                self.assertEqual(self.hooks, [])
                self.assertEqual(self.snapshot(self.root), before)

    def test_invalid_last_manifest_rejected_before_any_mutation(self):
        (self.frontend / "Dotcc.SourceFiles.txt").write_text("missing.cs\n")
        before = self.snapshot(self.root)
        with self.assertRaisesRegex(RuntimeError, "Invalid emitted source"):
            self.deliver()
        self.assertEqual(self.calls, [])
        self.assertEqual(self.hooks, [])
        self.assertEqual(self.snapshot(self.root), before)

    def test_shared_source_directory_rejected_before_any_mutation(self):
        # Give the shared directory both expected project names so validating
        # manifests alone cannot conceal duplicate directory ownership.
        shutil.copy2(self.raw / (self.main + ".csproj"), self.raw / (self.companion + ".csproj"))
        before = self.snapshot(self.root)
        with self.assertRaises(ValueError):
            self.deliver({self.companion: self.raw / ".." / self.main})
        self.assertEqual(self.calls, [])
        self.assertEqual(self.hooks, [])
        self.assertEqual(self.snapshot(self.root), before)

    def test_main_only_hooks_protect_authored_sources_during_semantic_processing(self):
        before = self.authored.read_bytes()
        self.deliver()
        self.assertEqual([(action, form) for action, _, form in self.hooks],
                         [("configure", "raw"), ("configure", "processed"),
                          ("finish", "processed"), ("finish", "raw"),
                          ("finish", "raw"), ("finish", "processed")])
        self.assertEqual(self.authored.read_bytes(), before)
        self.assertEqual((self.ctx.work / "authored/raw/Host.cs").read_bytes(), before)
        self.assertEqual((self.ctx.work / "authored/processed/Host.cs").read_text(), "// fixture semantic rewrite\n")
        for form in ("raw", "processed"):
            project = self.ctx.layout.project(self.ctx.profile, form)
            self.assertEqual(Path(next(ET.parse(project).iter("Compile")).get("Include")), self.authored)
        self.assertEqual([label for label, _ in self.calls if label.endswith("postprocess")],
                         ["postprocess", self.companion + "-postprocess"])

    def test_raw_only_skips_semantic_runner_and_preserves_processed_outputs(self):
        self.ctx.options.form = "raw"
        old = self.output(self.layout(self.companion).directory(form="processed"), self.companion, "existing processed")
        before = self.snapshot(old)
        self.deliver()
        self.assertEqual(set(self.ctx.receipt["outputs"]), {"raw"})
        self.assertFalse(any("postprocess" in label or "semantic" in label for label, _ in self.calls))
        self.assertEqual(self.snapshot(old), before)

    def test_alternate_profile_keeps_default_paths_untouched(self):
        self.ctx.profile = "alternate"
        self.deliver()
        self.assertFalse(self.ctx.layout.directory().exists())
        for form in ("raw", "processed"):
            self.assertTrue(self.layout(self.companion).project("alternate", form).is_file())
            self.assertEqual(check_product(self.ctx, form), self.ctx.layout.project("alternate", form))

    def test_companion_content_tamper_warns_or_fails_strict_off_skips_provenance(self):
        self.deliver()
        source = self.layout(self.companion).directory() / "Generated.cs"
        source.write_text("edited frontend")
        self.ctx.policy = HashPolicy("warn")
        with redirect_stderr(io.StringIO()):
            check_product(self.ctx, "processed")
        self.assertEqual(self.ctx.policy.warnings, ["Hash differs: " + str(source)])
        self.ctx.policy = HashPolicy("strict")
        with self.assertRaisesRegex(RuntimeError, "Hash differs"):
            check_product(self.ctx, "processed")
        self.ctx.policy = HashPolicy("off")
        with patch.object(Path, "read_text", side_effect=AssertionError("off must not read provenance")):
            self.assertEqual(check_product(self.ctx, "processed"), self.ctx.layout.project())
        self.assertEqual(self.ctx.policy.warnings, [])

    def test_off_publication_records_all_projects_without_output_hashes(self):
        self.ctx.policy = HashPolicy("off")
        self.deliver()
        for record in self.ctx.receipt["outputs"].values():
            self.assertEqual(record["hashes"], {})
            self.assertEqual(record["companions"][self.companion]["hashes"], {})

    def test_missing_companion_project_fails_even_in_warn_mode(self):
        self.deliver()
        self.layout(self.companion).project().unlink()
        self.ctx.policy = HashPolicy("warn")
        with self.assertRaisesRegex(RuntimeError, "Missing related library"):
            check_product(self.ctx, "processed")

    def test_companion_receipt_rejects_path_or_identity_tamper(self):
        self.deliver()
        original = self.pointer.read_text()
        mutations = (
            lambda companions: companions.update({"../escape": companions.pop(self.companion)}),
            lambda companions: companions[self.companion].update(project=str(self.root / "unrelated.csproj")),
            lambda companions: companions[self.companion].update(hashes={"../outside.cs": "digest"}),
        )
        for mutation in mutations:
            for mode in ("strict", "warn"):
                with self.subTest(mode=mode, mutation=mutation):
                    current = json.loads(original)
                    mutation(current["outputs"]["processed"]["companions"])
                    write_json(self.pointer, current)
                    self.ctx.policy = HashPolicy(mode)
                    with self.assertRaises(RuntimeError):
                        check_product(self.ctx, "processed")


if __name__ == "__main__":
    unittest.main()

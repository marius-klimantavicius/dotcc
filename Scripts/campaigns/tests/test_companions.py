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
from campaigns.delivery import prune_build_caches, source_files
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
        projects = [Path(project), *(Path(project).parent / "managed").glob("*/*proj")]
        for project in projects:
            for directory in ("bin", "obj"):
                self.write(str(project.parent.relative_to(self.root) / directory / "cache.txt"), label)

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

    def packaged_assets(self, directory, value="new"):
        assets = {
            "bootstrap-prefix/bin/postgres": json.dumps({"kind": "managed-entrypoint", "product": self.main, "value": value}),
            "bootstrap-prefix/bin/initdb": json.dumps({"kind": "managed-entrypoint", "product": self.companion, "value": value}),
            "bootstrap-prefix/obj/catalog.json": json.dumps({"catalog": value}),
            "assets/bin/obj/data.txt": value,
            "assets/obj/bin/data.txt": value,
            "assets/not-project.csproj/bin/data.txt": value,
            "managed/csharp/Helper.csproj": "<Project />",
            "managed/fsharp/Helper.fsproj": "<Project />",
            "managed/visualbasic/Helper.vbproj": "<Project />",
            "managed/csharp/resources/bin/data.txt": value,
            "managed/fsharp/resources/obj/data.txt": value,
        }
        for relative, content in assets.items():
            self.write(str(directory.relative_to(self.root) / relative), content)
        return assets

    def assert_no_build_caches(self, directory):
        for relative in (".", "managed/csharp", "managed/fsharp", "managed/visualbasic"):
            for cache in ("bin", "obj"):
                self.assertFalse((directory / relative / cache).exists(), str(directory / relative / cache))

    def check_packaged_delivery(self, companions):
        assets = self.packaged_assets(self.raw)
        if companions:
            self.packaged_assets(self.frontend)

        def restore(project, label):
            self.restore(project, label)
            self.assert_no_build_caches(project.parent)
            for relative, content in assets.items():
                self.assertEqual((project.parent / relative).read_text(), content)

        def build(action, project, label):
            if label.startswith("final-"):
                # Final validation recreates caches, so inspect pruning before
                # that build and then verify its new caches stay out of hashes.
                self.assert_no_build_caches(project.parent)
            self.managed(action, project, label)

        with patch.object(self.ctx, "restore", restore), patch.object(self.ctx, "managed", build):
            self.deliver(None if companions else {})
        current = json.loads(self.pointer.read_text())
        for form in ("raw", "processed"):
            record = current["outputs"][form]
            products = [(self.main, record)]
            if companions:
                products.append((self.companion, record["companions"][self.companion]))
            else:
                self.assertNotIn("companions", record)
            for name, record in products:
                directory = self.layout(name).directory(self.ctx.profile, form)
                owned = json.loads((directory / ".campaign-files.json").read_text())
                delivered = {str(path.relative_to(directory)) for path in source_files(directory)}
                self.assertEqual(set(record["hashes"]), delivered)
                for relative, content in assets.items():
                    key = str(Path(relative))
                    self.assertEqual((directory / relative).read_text(), content)
                    self.assertIn(key, owned)
                    self.assertEqual(record["hashes"][key], digest(directory / relative))
                for relative in (".", "managed/csharp", "managed/fsharp", "managed/visualbasic"):
                    for cache in ("bin", "obj"):
                        path = directory / relative / cache / "cache.txt"
                        self.assertTrue(path.is_file())
                        self.assertNotIn(str(path.relative_to(directory)), owned)
                        self.assertNotIn(str(path.relative_to(directory)), record["hashes"])
            self.assertEqual(check_product(self.ctx, form), self.ctx.layout.project(self.ctx.profile, form))
        self.assertFalse((self.root / "build/campaign/promotion.json").exists())

    def test_packaged_assets_and_nested_caches_in_companion_publication(self):
        self.check_packaged_delivery(companions=True)

    def test_packaged_assets_and_nested_caches_in_single_publication(self):
        self.check_packaged_delivery(companions=False)

    def test_pruning_does_not_follow_asset_or_cache_symlinks_and_provenance_rejects_them(self):
        outside = self.write("outside/Helper.csproj", "<Project />").parent
        sentinel = self.write("outside/bin/keep.txt", "outside asset")
        for relative in ("bootstrap-prefix/bin", "bootstrap-prefix/obj", "bin", "obj"):
            with self.subTest(relative=relative):
                link = self.raw / relative
                link.parent.mkdir(parents=True, exist_ok=True)
                try:
                    link.symlink_to(outside, target_is_directory=True)
                except (OSError, NotImplementedError) as error:
                    self.skipTest(f"Directory symlinks unavailable: {error}")
                try:
                    prune_build_caches(self.raw)
                    self.assertTrue(link.is_symlink())
                    self.assertEqual(sentinel.read_text(), "outside asset")
                    with self.assertRaisesRegex(RuntimeError, "Symlink in generated project"):
                        source_files(self.raw)
                finally:
                    link.unlink()

    def test_packaged_descriptor_tamper_fails_strict_for_both_products_and_forms(self):
        self.packaged_assets(self.raw)
        self.packaged_assets(self.frontend)
        self.deliver()
        for name in (self.main, self.companion):
            for form in ("raw", "processed"):
                for entrypoint in ("postgres", "initdb"):
                    with self.subTest(name=name, form=form, entrypoint=entrypoint):
                        descriptor = self.layout(name).directory(self.ctx.profile, form) / "bootstrap-prefix/bin" / entrypoint
                        before = descriptor.read_bytes()
                        descriptor.write_text('{"kind": "managed-entrypoint", "product": "tampered"}')
                        with self.assertRaisesRegex(RuntimeError, "Hash differs"):
                            check_product(self.ctx, form)
                        descriptor.write_bytes(before)
                        check_product(self.ctx, form)

    def test_prior_unowned_assets_preserved_and_obsolete_owned_assets_removed(self):
        for name in (self.main, self.companion):
            for form in ("raw", "processed"):
                directory = self.output(self.layout(name).directory(form=form), name, "old")
                self.write(str(directory.relative_to(self.root) / "bootstrap-prefix/bin/obsolete"), "old descriptor")
                write_json(directory / ".campaign-files.json",
                           [str(path.relative_to(directory)) for path in source_files(directory)])
                for cache in ("bin", "obj"):
                    self.write(str(directory.relative_to(self.root) / "caller" / cache / "notes.txt"), "caller asset")
        self.deliver()
        for form, record in self.ctx.receipt["outputs"].items():
            for name, output in ((self.main, record), (self.companion, record["companions"][self.companion])):
                directory = self.layout(name).directory(form=form)
                self.assertFalse((directory / "bootstrap-prefix/bin/obsolete").exists())
                owned = json.loads((directory / ".campaign-files.json").read_text())
                for cache in ("bin", "obj"):
                    relative = str(Path("caller") / cache / "notes.txt")
                    self.assertEqual((directory / relative).read_text(), "caller asset")
                    self.assertEqual(output["hashes"][relative], digest(directory / relative))
                    self.assertNotIn(relative, owned)

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
        self.packaged_assets(self.raw)
        self.packaged_assets(self.frontend)
        previous = {}
        for name in (self.main, self.companion):
            for form in ("raw", "processed"):
                directory = self.output(self.layout(name).directory(self.ctx.profile, form), name, "old " + name + form)
                self.packaged_assets(directory, "old")
                write_json(directory / ".campaign-files.json",
                           [str(path.relative_to(directory)) for path in source_files(directory)])
                self.write(str(directory.relative_to(self.root) / "caller-notes.txt"), "preserve caller file")
                self.write(str(directory.relative_to(self.root) / "caller/bin/notes.txt"), "preserve caller asset")
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

    def test_missing_companion_hash_evidence_obeys_provenance_policy(self):
        self.deliver()
        current = json.loads(self.pointer.read_text())
        current["outputs"]["processed"]["companions"][self.companion]["hashes"] = {}
        write_json(self.pointer, current)
        self.ctx.policy = HashPolicy("strict")
        with self.assertRaisesRegex(RuntimeError, "No recorded.*hashes"):
            check_product(self.ctx, "processed")
        self.ctx.policy = HashPolicy("warn")
        with redirect_stderr(io.StringIO()):
            self.assertEqual(check_product(self.ctx, "processed"), self.ctx.layout.project())
        self.assertEqual(len(self.ctx.policy.warnings), 1)
        self.assertIn(str(self.layout(self.companion).project()), self.ctx.policy.warnings[0])
        self.ctx.policy = HashPolicy("off")
        self.assertEqual(check_product(self.ctx, "processed"), self.ctx.layout.project())
        self.assertEqual(self.ctx.policy.warnings, [])

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

"""Actual recipe/hook contracts with private files and command-recording runners.

These checks do not execute an engine or supply database responses.
"""
import json
from pathlib import Path
import shutil
import sys
import tempfile
from types import SimpleNamespace
import unittest
import xml.etree.ElementTree as ET

REPOSITORY = Path(__file__).resolve().parents[3]
PROJECT = REPOSITORY / "pglite"
sys.path.insert(0, str(REPOSITORY / "Scripts"))
from campaigns.identity import HashPolicy, digest
from campaigns.layout import Layout
from campaigns.recipes import helper


class ProductDeliveryTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="pglite product hooks ")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.translation = helper(PROJECT / "scripts/translation.py")
        self.recipe_module = helper(PROJECT / "scripts/campaign.py")
        self.ctx = SimpleNamespace(root=self.root, work=self.root / "work", profile="embedding-native",
                                   options=SimpleNamespace(action="translate", form="all", mode="all", rid="linux-x64"),
                                   policy=HashPolicy("off"), receipt={})

    def authored_inputs(self):
        for path in (PROJECT / "src/Host").glob("*.cs"):
            self.copy_source(path)
        for name in ("InitializationBackend.cs", "InitializationBackend.Signals.cs",
                     "InitializationArguments.cs", "InitializationProcessTransport.cs"):
            self.copy_source(PROJECT / "src/Initialization" / name)
        policy = self.root / "config/translation.json"
        policy.parent.mkdir(parents=True)
        policy.write_text(json.dumps({"complete_product_manifest": True}))
        return {path: digest(path) for path in (self.root / "src").rglob("*.cs")}

    def copy_source(self, path):
        destination = self.root / path.relative_to(PROJECT)
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, destination)

    def test_real_hooks_isolate_each_semantic_form_and_restore_original_links(self):
        before = self.authored_inputs()
        projects = {}
        for form in ("raw", "processed"):
            project = self.ctx.work / form / "TranslatedPGlite/TranslatedPGlite.csproj"
            project.parent.mkdir(parents=True)
            if form == "raw":
                project.write_text("<Project />")
            else:
                shutil.copy2(projects["raw"], project)
            self.translation.configure(self.ctx, project, form)
            links = list(ET.parse(project).iter("Compile"))
            self.assertEqual(len(links), len(before))
            self.assertEqual(len({node.get("Link") for node in links}), len(before))
            for node in links:
                copy = Path(node.get("Include"))
                original = self.root / "src" / node.get("Link")
                self.assertTrue(copy.is_relative_to(self.ctx.work / "authored-host" / form))
                self.assertEqual(digest(copy), before[original])
                if form == "processed":
                    copy.write_text("// semantic-runner fixture rewrite\n")
            projects[form] = project
        for form, project in projects.items():
            self.translation.finish(self.ctx, project, form)
            for node in ET.parse(project).iter("Compile"):
                self.assertEqual(Path(node.get("Include")), self.root / "src" / node.get("Link"))
        self.assertEqual({path: digest(path) for path in before}, before)
        for copy in (self.ctx.work / "authored-host/raw").rglob("*.cs"):
            self.assertEqual(digest(copy), before[self.root / "src" / copy.relative_to(self.ctx.work / "authored-host/raw")])

    def test_incomplete_product_finish_rejects_before_project_mutation(self):
        self.authored_inputs()
        (self.root / "config/translation.json").write_text(json.dumps(
            {"complete_product_manifest": False, "publication_blocker": "incomplete actual backend"}))
        project = self.root / "candidate.csproj"
        project.write_text('<Project><ItemGroup><Compile Include="private.cs" Link="Host/Owner.cs" /></ItemGroup></Project>')
        before = project.read_bytes()
        with self.assertRaisesRegex(RuntimeError, "incomplete actual backend"):
            self.translation.finish(self.ctx, project, "raw")
        self.assertEqual(project.read_bytes(), before)

    def test_complete_embedding_manifest_never_publishes_diagnostic_subset(self):
        self.authored_inputs()
        (self.root / "config/translation.json").write_text(json.dumps(
            {"complete_product_manifest": True, "publication_blocker": "diagnostic subsets are probe-only"}))
        self.ctx.profile = "native-diagnostic"
        project = self.root / "diagnostic.csproj"
        project.write_text("<Project />")
        before = project.read_bytes()
        with self.assertRaisesRegex(RuntimeError, "diagnostic subsets are probe-only"):
            self.translation.finish(self.ctx, project, "raw")
        self.assertEqual(project.read_bytes(), before)

    def command_context(self, fail=False):
        self.ctx.recipe = self.recipe_module.recipe
        self.ctx.layout = Layout(self.root, "TranslatedPGlite", self.ctx.recipe.default_profile)
        for name in ("TranslatedPGlite", "TranslatedInitdb"):
            for form in ("raw", "processed"):
                project = Layout(self.root, name, self.ctx.recipe.default_profile).project(self.ctx.profile, form)
                project.parent.mkdir(parents=True)
                project.write_text("<Project />")
        self.copy_source(PROJECT / self.ctx.recipe.consumer.project)
        builds, runs = [], []
        def managed(action, project, label, *arguments, **kwargs):
            builds.append((action, project, label, arguments, kwargs))
            if fail:
                raise RuntimeError("fixture build failed")
        self.ctx.managed = managed
        self.ctx.run = lambda command, label, **kwargs: runs.append((command, label, kwargs))
        return builds, runs

    def test_real_consumer_hook_selects_matching_backend_frontend_prefix_for_all_rows(self):
        builds, runs = self.command_context()
        recipe, form = self.ctx.recipe, self.ctx.options.form
        self.recipe_module.consumer(self.ctx, None)
        self.assertIs(self.ctx.recipe, recipe)
        self.assertEqual(self.ctx.options.form, form)
        self.assertEqual(len(builds), 4)
        self.assertEqual(len(runs), 4)
        for index, selected in enumerate(("raw", "processed")):
            backend = self.ctx.layout.project(self.ctx.profile, selected)
            frontend = Layout(self.root, "TranslatedInitdb", recipe.default_profile).project(self.ctx.profile, selected)
            for mode_index, mode in enumerate(("jit", "aot")):
                action, _, label, arguments, _ = builds[index * 2 + mode_index]
                self.assertEqual(action, "build" if mode == "jit" else "publish")
                self.assertEqual(label, "consumer-" + selected + "-" + mode + ("-build" if mode == "jit" else "-publish"))
                self.assertIn("-p:TranslatedProject=" + str(backend), arguments)
                self.assertIn("-p:InitdbTranslatedProject=" + str(frontend), arguments)
                command, _, kwargs = runs[index * 2 + mode_index]
                self.assertEqual(command[-4:-2], ["--bootstrap-prefix", str(backend.parent / "bootstrap-prefix")])
                self.assertTrue(command[-1].endswith("consumer-" + selected + "-" + mode + "-database"))
                self.assertTrue(kwargs["separate"])
                if mode == "aot":
                    self.assertIn("-p:PublishAot=true", arguments)

    def test_real_consumer_hook_restores_recipe_and_form_when_a_build_fails(self):
        _, runs = self.command_context(fail=True)
        recipe, form = self.ctx.recipe, self.ctx.options.form
        with self.assertRaisesRegex(RuntimeError, "fixture build failed"):
            self.recipe_module.consumer(self.ctx, None)
        self.assertIs(self.ctx.recipe, recipe)
        self.assertEqual(self.ctx.options.form, form)
        self.assertEqual(runs, [])


if __name__ == "__main__":
    unittest.main()

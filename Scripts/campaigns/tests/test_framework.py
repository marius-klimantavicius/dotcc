"""Infrastructure contracts exercised with tiny fixtures, never upstream builds."""
from contextlib import redirect_stderr
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import tempfile
import threading
from types import SimpleNamespace
import unittest
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.delivery import lock, promote, recover
from campaigns.identity import HashPolicy, digest, exact_replace, apply_unified_patch, write_json
from campaigns.inputs import acquire, extract
from campaigns.layout import Layout, check, scaffold
from campaigns.model import Recipe, Source, Task, ordered_tasks
from campaigns.process import CommandError, execute
from campaigns.translation import check_product
from campaigns.recipes import discover, load


class DiscoveryTests(unittest.TestCase):
    def test_new_recipes_are_discovered_without_registration(self):
        with tempfile.TemporaryDirectory() as directory:
            repo = Path(directory)
            (repo / "ordinary-directory").mkdir()
            (repo / "nested/example/scripts").mkdir(parents=True)
            (repo / "nested/example/scripts/campaign.py").write_text("raise AssertionError('not a root project')")
            for name in ("z-new-library", "a-future-library"):
                path = repo / name / "scripts/campaign.py"
                path.parent.mkdir(parents=True)
                path.write_text('from campaigns.model import Recipe\n'
                                f'recipe = Recipe({name!r}, "TranslatedFuture", ("default",), (), lambda root: ())\n')
            self.assertEqual(discover(repo), ("a-future-library", "z-new-library"))
            self.assertEqual(load(repo, "z-new-library").consumer.property, "TranslatedProject")
            for name in ("../z-new-library", "ordinary-directory", "nested/example"):
                with self.assertRaisesRegex(ValueError, "Unknown project"):
                    load(repo, name)
            path.write_text('from campaigns.model import Recipe\n'
                            'recipe = Recipe("wrong-name", "TranslatedFuture", ("default",), (), lambda root: ())\n')
            with self.assertRaisesRegex(ValueError, "does not match"):
                load(repo, "a-future-library")

    def test_campaign_helpers_keep_hash_literals_in_metadata(self):
        import re
        repo = Path(__file__).resolve().parents[3]
        digest = re.compile(r"\b[0-9a-fA-F]{40,64}\b")
        ignored = {"ref", "generated", "build", "artifacts", "bin", "obj", "__pycache__"}
        for name in discover(repo):
            for path in (repo / name).rglob("*"):
                if path.suffix in (".py", ".sh") and not ignored.intersection(path.relative_to(repo / name).parts):
                    self.assertIsNone(digest.search(path.read_text()), str(path))

    @unittest.skipUnless(shutil.which("bash"), "Bash required")
    def test_new_project_helpers_fetch_and_test_both_consumer_forms(self):
        with tempfile.TemporaryDirectory(prefix="future campaign ") as directory:
            repo = Path(directory)
            source_scripts = Path(__file__).resolve().parents[2]
            shutil.copytree(source_scripts / "campaigns", repo / "Scripts/campaigns",
                            ignore=shutil.ignore_patterns("__pycache__", "tests"))
            for name in ("campaign.py", "campaign.sh", "campaign-common.sh", "campaign-reference.py"):
                shutil.copy2(source_scripts / name, repo / "Scripts" / name)
            root = repo / "future-library"
            (root / "scripts").mkdir(parents=True)
            (root / "scripts/campaign.py").write_text('''from campaigns.model import Consumer, Recipe, Source, Suite
def sources(root):
    yield Source("product", "unused", root / "ref/source.tar", root / "ref/source", "source", required=("library.c",))
recipe = Recipe("future-library", "TranslatedFuture", ("default",),
                ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources,
                consumer=Consumer(property="FutureLibrary", arguments=("argument with spaces",)),
                suites=(Suite("consumer", ("consumer",)),), default_suites=("consumer",))
''')
            for form in ("TranslatedFuture", "TranslatedFuture.Raw", "../samples/ManagedConsumer"):
                path = root / "generated" / form
                path.mkdir(parents=True, exist_ok=True)
                (path / ("ManagedConsumer.csproj" if form.startswith("..") else "TranslatedFuture.csproj")).write_text("<Project />")
            (root / "ref/source").mkdir(parents=True)
            (root / "ref/source/library.c").write_text("int value(void) { return 1; }")
            binary = repo / "bin/dotnet"
            binary.parent.mkdir()
            binary.write_text('#!' + sys.executable + '\nimport sys; print(repr(sys.argv[1:]))\n')
            binary.chmod(0o755)
            env = {**os.environ, "PATH": str(binary.parent) + os.pathsep + os.environ["PATH"]}
            def run(*args):
                result = subprocess.run(["bash", repo / "Scripts/campaign.sh", *args],
                                        cwd="/", env=env, capture_output=True, text=True)
                self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
                return result.stdout
            self.assertIn("future-library", run("list", "all"))
            run("layout", "future-library", "--write")
            result = subprocess.run(["bash", root / "scripts/fetch.sh", "--fetch", "never"],
                                    cwd="/", env=env, capture_output=True, text=True)
            self.assertEqual(result.returncode, 0, result.stderr)
            run("test", "future-library", "--form", "all", "--hashes", "off")
            latest = json.loads((root / "artifacts/campaign/latest-attempt.json").read_text())
            receipt = json.loads(Path(latest["receipt"]).read_text())
            builds = [row for row in receipt["commands"] if row["label"].endswith("-build")]
            self.assertEqual(len(builds), 2)
            for row in builds:
                self.assertTrue(any(arg.startswith("-p:FutureLibrary=") for arg in row["command"]))
            runs = [row for row in receipt["commands"] if row["label"].endswith("-run")]
            self.assertEqual([row["command"][-1] for row in runs], ["argument with spaces"] * 2)


class Fixture(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="campaign test ")
        self.root = Path(self.temp.name)

    def tearDown(self):
        self.temp.cleanup()

    def file(self, name, data="input"):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(data)
        return path


class PolicyTests(Fixture):
    def test_existing_edited_product_warns_but_build_remains_allowed(self):
        layout = Layout(self.root, "TranslatedFixture", "default")
        self.file("generated/TranslatedFixture/TranslatedFixture.csproj", "<Project />")
        self.file("generated/TranslatedFixture/Generated.cs", "edited")
        write_json(self.root / "artifacts/campaign/current-default.json",
                   {"outputs": {"processed": {"hashes": {"Generated.cs": "historical"}}}})
        ctx = SimpleNamespace(layout=layout, root=self.root, profile="default", receipt={}, policy=HashPolicy("warn"))
        with redirect_stderr(io.StringIO()):
            self.assertEqual(check_product(ctx, "processed"), layout.project())
        self.assertEqual(len(ctx.policy.warnings), 1)
        ctx.policy = HashPolicy("strict")
        with self.assertRaisesRegex(RuntimeError, "Hash differs"):
            check_product(ctx, "processed")
        self.file("artifacts/campaign/current-default.json", "not JSON")
        ctx.policy = HashPolicy("off")
        self.assertEqual(check_product(ctx, "processed"), layout.project())

    def test_patch_allows_offsets_but_rejects_ambiguous_context(self):
        patch = '--- a/file\n+++ b/file\n@@ -1,2 +1,2 @@\n context\n-old\n+new\n'
        self.assertEqual(apply_unified_patch('unrelated\ncontext\nold\n', patch),
                         'unrelated\ncontext\nnew\n')
        for text in ('context\nold\ncontext\nold\n', 'context\nchanged\n'):
            with self.assertRaisesRegex(RuntimeError, 'exactly 1'):
                apply_unified_patch(text, patch)

    def test_warn_continues(self):
        path = self.file("source.c")
        warnings = []
        with redirect_stderr(io.StringIO()):
            self.assertEqual(HashPolicy("warn", warnings).check(path, "wrong"), digest(path))
        self.assertEqual(len(warnings), 1)

    def test_off_does_not_read(self):
        self.assertIsNone(HashPolicy("off").check(self.root / "missing", "wrong"))
        self.assertEqual(HashPolicy("off").manifest([self.root / "missing"], self.root), {})

    def test_strict_rejects_drift_and_missing_evidence(self):
        for expected in ("wrong", None):
            with self.assertRaises(RuntimeError):
                HashPolicy("strict").check(self.file("source"), expected)

    def test_exact_replacement_ignores_unrelated_edits(self):
        self.assertEqual(exact_replace("new comment\nold\n", "old", "new"), "new comment\nnew\n")
        for text in ("missing", "old old"):
            with self.assertRaises(RuntimeError):
                exact_replace(text, "old", "new")


class InputTests(Fixture):
    def source(self):
        return Source("fixture", "https://invalid.example/unused", self.root / "source.zip",
                      self.root / "tree", "upstream", required=("source.c",))

    def test_existing_tree_needs_no_archive_or_hash_manifest(self):
        self.file("tree/source.c", "edited input")
        self.assertEqual(acquire(self.source(), HashPolicy("warn"), "never"), self.root / "tree")

    def test_missing_offline_fails(self):
        with self.assertRaisesRegex(RuntimeError, "Missing source"):
            acquire(self.source(), HashPolicy("off"), "never")

    def test_cached_archive_extracts_without_network(self):
        source = self.source()
        with zipfile.ZipFile(source.archive, "w") as bundle:
            bundle.writestr("upstream/source.c", "int main() {}")
        acquire(source, HashPolicy("off"), "never")
        self.assertEqual((source.directory / "source.c").read_text(), "int main() {}")

    def test_zip_traversal_and_foreign_root_rejected(self):
        for name in ("upstream/../escape", "/upstream/absolute", "foreign/source.c", "upstream/C:/escape"):
            with self.subTest(name=name):
                archive = self.root / "bad.zip"
                with zipfile.ZipFile(archive, "w") as bundle:
                    bundle.writestr(name, "bad")
                with self.assertRaises(RuntimeError):
                    extract(archive, self.root / "out", "upstream")

    def test_tar_symlink_rejected(self):
        archive = self.root / "bad.tar"
        with tarfile.open(archive, "w") as bundle:
            member = tarfile.TarInfo("upstream/link")
            member.type, member.linkname = tarfile.SYMTYPE, "/etc/passwd"
            bundle.addfile(member)
        with self.assertRaises(RuntimeError):
            extract(archive, self.root / "out", "upstream")

    def test_duplicate_files_rejected(self):
        archive = self.root / "bad.tar"
        with tarfile.open(archive, "w") as bundle:
            for _ in range(2):
                member = tarfile.TarInfo("upstream/file")
                member.size = 1
                bundle.addfile(member, io.BytesIO(b"x"))
        with self.assertRaises(RuntimeError):
            extract(archive, self.root / "out", "upstream")


class LayoutTests(Fixture):
    def recipe(self):
        return Recipe("fixture", "TranslatedFixture", ("default", "debug"),
                      ("samples/ManagedConsumer/ManagedConsumer.csproj",), lambda root: ())

    def test_scaffold_and_check_without_generated_library(self):
        recipe = self.recipe()
        layout = Layout(self.root, recipe.product, recipe.default_profile)
        self.file("samples/ManagedConsumer/ManagedConsumer.csproj", "<Project />")
        scaffold(layout, recipe)
        self.assertEqual(check(layout, recipe), [])
        self.assertIn('campaign_exec translate "$@"', (self.root / "scripts/translate.sh").read_text())

    def test_scaffold_preserves_authored_files_and_solution_entries(self):
        recipe = self.recipe()
        layout = Layout(self.root, recipe.product, recipe.default_profile)
        self.file("scripts/build.sh", "authored")
        self.file("ManagedConsumer.slnx", '<Solution><Project Path="extra.csproj" /></Solution>')
        scaffold(layout, recipe)
        self.assertEqual((self.root / "scripts/build.sh").read_text(), "authored")
        self.assertIn('Path="extra.csproj"', layout.solution.read_text())

    def test_profiles_and_forms_have_separate_paths(self):
        layout = Layout(self.root, "TranslatedFixture", "default")
        self.assertEqual(layout.project(), self.root / "generated/TranslatedFixture/TranslatedFixture.csproj")
        self.assertEqual(layout.project("debug", "raw"), self.root / "generated/profiles/debug/TranslatedFixture.Raw/TranslatedFixture.csproj")
        with self.assertRaises(ValueError):
            layout.directory("../../escape")


class GraphTests(unittest.TestCase):
    def test_dependencies_run_once_in_order(self):
        tasks = [Task("a", lambda: None, ("b",)), Task("b", lambda: None), Task("c", lambda: None, ("b",))]
        self.assertEqual([t.name for t in ordered_tasks(tasks)], ["b", "a", "c"])

    def test_invalid_graphs_fail_before_execution(self):
        for tasks in ([Task("a", None, ("missing",))], [Task("a", None, ("b",)), Task("b", None, ("a",))],
                      [Task("a", None), Task("a", None)]):
            with self.assertRaises(ValueError):
                ordered_tasks(tasks)


class DeliveryTests(Fixture):
    def test_metadata_failure_rolls_back_products_and_current_pointer(self):
        candidate, target = self.output("candidate", "new"), self.output("target", "old")
        pointer = self.root / "current.json"
        write_json(pointer, {"run": "old"})
        def fail():
            raise RuntimeError("metadata failure")
        with self.assertRaisesRegex(RuntimeError, "metadata failure"):
            promote([(candidate, target)], self.root / "journal.json", lambda: None,
                    {pointer: lambda: {"run": "new"}, self.root / "second.json": fail})
        self.assertEqual(json.loads(pointer.read_text()), {"run": "old"})
        self.assertEqual((target / "Generated.cs").read_text(), "old")

    def output(self, name, value):
        path = self.root / name
        self.file(name + "/Generated.cs", value)
        write_json(path / ".campaign-files.json", ["Generated.cs"])
        return path

    def test_all_outputs_roll_back_on_final_build_failure(self):
        raw, product = self.output("raw", "new raw"), self.output("product", "new product")
        old_raw, old_product = self.output("stable-raw", "old raw"), self.output("stable-product", "old product")
        def fail():
            raise RuntimeError("build failed")
        with self.assertRaisesRegex(RuntimeError, "build failed"):
            promote([(raw, old_raw), (product, old_product)], self.root / "journal.json", fail)
        self.assertEqual((old_raw / "Generated.cs").read_text(), "old raw")
        self.assertEqual((old_product / "Generated.cs").read_text(), "old product")
        self.assertEqual((product / "Generated.cs").read_text(), "new product")

    def test_unowned_files_preserved(self):
        candidate, target = self.output("candidate", "new"), self.output("target", "old")
        self.file("target/notes.txt", "user data")
        promote([(candidate, target)], self.root / "journal.json", lambda: None)
        self.assertEqual((target / "notes.txt").read_text(), "user data")

    def test_conflicting_unowned_file_is_not_overwritten(self):
        candidate, target = self.output("candidate", "new"), self.output("target", "old")
        self.file("candidate/notes.txt", "generated")
        self.file("target/notes.txt", "authored")
        with self.assertRaisesRegex(RuntimeError, "Unowned"):
            promote([(candidate, target)], self.root / "journal.json", lambda: None)
        self.assertEqual((target / "notes.txt").read_text(), "authored")

    def test_interrupted_rename_recovered(self):
        candidate, target = self.output("candidate", "new"), self.output("target", "old")
        backup = self.root / "backup"
        journal = self.root / "journal.json"
        write_json(journal, {"outputs": [dict(candidate=str(candidate), target=str(target), backup=str(backup), previous=True)]})
        target.rename(backup)
        candidate.rename(target)
        recover(journal)
        self.assertEqual((target / "Generated.cs").read_text(), "old")

    @unittest.skipIf(os.name == "nt", "POSIX duplicate-descriptor locking contract")
    def test_concurrent_lock_rejected(self):
        with lock(self.root / "lock"):
            with self.assertRaises(RuntimeError):
                with lock(self.root / "lock"):
                    pass


class ProcessTests(Fixture):
    @unittest.skipUnless(Path("/proc/self/stat").exists(), "Linux descendant cleanup")
    def test_timeout_stops_detached_grandchild(self):
        code = ("import subprocess,sys,time,pathlib; "
                "p=subprocess.Popen([sys.executable,'-c','import time;time.sleep(60)'],start_new_session=True); "
                "pathlib.Path('child.pid').write_text(str(p.pid));time.sleep(60)")
        with self.assertRaises(CommandError):
            execute([sys.executable, "-c", code], label="tree", cwd=self.root,
                    output=self.root / "log", timeout=0.3)
        pid = int((self.root / "child.pid").read_text())
        path = Path(f"/proc/{pid}/stat")
        if path.exists():
            self.assertEqual(path.read_text().rsplit(")", 1)[1].split()[0], "Z")

    def test_worker_cancellation_has_record(self):
        cancellation = threading.Event()
        timer = threading.Timer(0.05, cancellation.set)
        timer.start()
        record = {}
        try:
            with self.assertRaises(CommandError):
                execute([sys.executable, "-c", "import time; time.sleep(60)"], label="cancel",
                        cwd=self.root, output=self.root / "log", cancellation=cancellation, record=record)
            self.assertEqual(record["status"], "cancelled")
        finally:
            timer.cancel()

    def test_stdout_and_stderr_separate(self):
        result = execute([sys.executable, "-c", "import sys; print('result'); print('diagnostic', file=sys.stderr)"],
                         label="test", cwd=self.root, output=self.root / "out", separate=True)
        self.assertEqual(result["status"], "passed")
        self.assertEqual(Path(result["stdout"]).read_text(), "result\n")
        self.assertEqual(Path(result["stderr"]).read_text(), "diagnostic\n")

    def test_startup_failure_has_record(self):
        record = {}
        with self.assertRaises(CommandError):
            execute([self.root / "missing-executable"], label="missing", cwd=self.root, output=self.root / "log", record=record)
        self.assertEqual(record["status"], "failed")
        self.assertIn("seconds", record)

    def test_timeout_has_record(self):
        record = {}
        with self.assertRaises(CommandError):
            execute([sys.executable, "-c", "import time; time.sleep(60)"], label="timeout", cwd=self.root,
                    output=self.root / "log", timeout=0.05, record=record)
        self.assertEqual(record["status"], "timed_out")


@unittest.skipUnless(shutil.which("bash"), "Bash required")
class BashTests(Fixture):
    def invoke(self, names):
        scripts = self.root / "Scripts"
        scripts.mkdir()
        repo_scripts = Path(__file__).resolve().parents[2]
        for name in ("campaign-common.sh", "campaign.sh"):
            shutil.copy2(repo_scripts / name, scripts / name)
        (scripts / "campaign.py").write_text("import os,sys,json; print(json.dumps([os.environ['PYTHON_CMD'],sys.argv[1:]])); sys.exit(7)\n")
        binaries = self.root / "bin"
        binaries.mkdir()
        for name, works in names.items():
            path = binaries / name
            path.write_text('#!/bin/sh\n' + ('exec "' + sys.executable + '" "$@"\n' if works else 'exit 1\n'))
            path.chmod(0o755)
        (binaries / "dirname").symlink_to(shutil.which("dirname"))
        return subprocess.run([shutil.which("bash"), scripts / "campaign.sh", "translate", "fixture", "a b", "$literal"],
                              cwd="/", env={**os.environ, "PATH": str(binaries)}, capture_output=True, text=True)

    def test_python_fallback_arguments_and_exit_status(self):
        result = self.invoke({"python": True})
        self.assertEqual(result.returncode, 7, result.stderr)
        self.assertEqual(json.loads(result.stdout), ["python", ["translate", "fixture", "a b", "$literal"]])

    def test_python3_preferred(self):
        result = self.invoke({"python": True, "python3": True})
        self.assertEqual(json.loads(result.stdout)[0], "python3")

    def test_broken_python3_falls_back(self):
        result = self.invoke({"python": True, "python3": False})
        self.assertEqual(json.loads(result.stdout)[0], "python")

    def test_missing_python_fails_clearly(self):
        result = self.invoke({})
        self.assertEqual(result.returncode, 1)
        self.assertIn("Python 3 is required", result.stderr)


if __name__ == "__main__":
    unittest.main()

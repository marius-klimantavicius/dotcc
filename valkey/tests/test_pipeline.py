"""Exercise real orchestration in isolated campaigns with test-only tool executables."""
import hashlib
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import tarfile
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def hashes(tree):
    return {path.relative_to(tree).as_posix(): sha(path) for path in tree.rglob("*") if path.is_file()}


class PipelineTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="valkey pipeline with spaces ")
        self.addCleanup(self.temporary.cleanup)
        self.repo = Path(self.temporary.name)
        self.root = self.repo / "valkey"
        self.root.mkdir()
        shutil.copytree(ROOT / "scripts", self.root / "scripts", ignore=shutil.ignore_patterns("__pycache__"))
        shutil.copytree(ROOT.parent / "Scripts", self.repo / "Scripts", ignore=shutil.ignore_patterns("__pycache__", "*.lock"))
        for name in ("src/Managed.Valkey/Managed.Valkey.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"):
            project = self.root / name
            project.parent.mkdir(parents=True, exist_ok=True)
            project.write_text("<Project />")
        (self.root / "tests").mkdir()
        (self.root / "ManagedConsumer.slnx").write_text('<Solution><Project Path="generated/TranslatedValkey/TranslatedValkey.csproj" /><Project Path="src/Managed.Valkey/Managed.Valkey.csproj" /><Project Path="samples/ManagedConsumer/ManagedConsumer.csproj" /></Solution>')
        (self.root / "config").mkdir()
        self.tree = self.root / "ref/valkey-fixture"
        self.archive = self.root / "ref/valkey-fixture.tar.gz"
        generator_prefix = (
            "from pathlib import Path\nimport sys\n"
            "with Path('generator-interpreters.txt').open('a') as stream:\n"
            "    stream.write(sys.executable + '\\n')\n")
        files = {
            "src/server.c": b"/* Copyright fixture author; licensed for testing. */\nint server(void) { return 42; }\n",
            "src/server.h": b"/* fixture header */\n",
            "src/second.c": b"int second(void) { return 24; }\n",
            "COPYING": b"Fixture redistribution notice.\n",
            "src/fmtargs.h": b"/* fixed preamble */\n/* Everything below this line is generated */\nstale\n",
            "src/version.h": b'#define VALKEY_VERSION "fixture"\n',
            "utils/generate-command-code.py": (generator_prefix +
                "Path('src/commands.def').write_text('/* deterministic command table */\\n')\n").encode(),
            "utils/generate-fmtargs.py": (generator_prefix +
                "print('/* Everything below this line is generated */')\nprint('#define FMTARG 42')\n").encode(),
        }
        self.reference_hashes = {name: hashlib.sha256(content).hexdigest() for name, content in files.items()}
        data = io.BytesIO()
        with tarfile.open(fileobj=data, mode="w:gz") as archive:
            for name in ("", "src", "utils"):
                member = tarfile.TarInfo("valkey-fixture" + ("/" + name if name else ""))
                member.type, member.mode = tarfile.DIRTYPE, 0o755
                archive.addfile(member)
            for name, content in files.items():
                member = tarfile.TarInfo("valkey-fixture/" + name)
                member.size, member.mode = len(content), 0o644
                archive.addfile(member, io.BytesIO(content))
        self.archive_bytes = data.getvalue()
        archive_sha = hashlib.sha256(self.archive_bytes).hexdigest()
        manifest = {"archive_sha256": archive_sha, "directories": ["src", "utils"],
                    "files": {name: {"sha256": hashlib.sha256(content).hexdigest(), "size": len(content),
                                     "executable": 0} for name, content in files.items()}}
        self.write_json(self.root / "config/source-files.json", manifest)
        self.write_json(self.root / "config/source.json", {
            "version": "fixture", "commit": "0123456789abcdef", "directory": self.tree.name,
            "archive": self.archive.name, "url": "https://invalid.example/valkey-fixture.tar.gz",
            "archive_sha256": archive_sha, "file_manifest": "source-files.json",
            "file_manifest_sha256": sha(self.root / "config/source-files.json")})
        self.write_json(self.root / "config/licenses.json", {"commit": "0123456789abcdef", "files": [
            {"path": "COPYING", "sha256": self.reference_hashes["COPYING"]}]})
        self.write_json(self.root / "config/sources.json", {"commit": "0123456789abcdef", "sources": [
            {"path": "src/server.c", "defines": ["PROFILE=1"], "include_dirs": ["src"]},
            {"path": "src/second.c", "defines": [], "include_dirs": ["src"]}]})
        self.write_json(self.root / "config/managed-adaptations.json", {
            "name": "test-managed", "commit": "0123456789abcdef", "adaptations": [],
            "injected_files": [], "extra_sources": []})
        self.write_json(self.root / "config/dotcc-overrides.json", {"functions": []})
        (self.root / "src/Host").mkdir(parents=True)
        (self.root / "src/Host/ValkeyHost.cs").write_text("// authored host fixture\n")
        self.seed_archive()
        # Real snapshot_tools verifies and copies these inert fixtures; only the
        # test dotnet executable consumes them. They never enter product sources.
        for project, dll in (("DotCC", "dotcc.dll"), ("DotCC.PostProcess", "dotcc-postprocess.dll")):
            directory = self.repo / project / "bin/Release/net10.0"
            directory.mkdir(parents=True)
            (directory / dll).write_text("test-only tool identity")
            (directory / "tool.deps.json").write_text("{}")
        self.bin = self.repo / "test tools"
        self.bin.mkdir()
        dotnet = self.bin / "dotnet"
        dotnet.write_text("#!/usr/bin/env python3\n" + '''
import json, os
from pathlib import Path
import sys
args = sys.argv[1:]
with Path(os.environ["TEST_DOTNET_LOG"]).open("a") as stream:
    stream.write(json.dumps(args) + "\\n")
fail = os.environ.get("TEST_DOTNET_FAIL", "")
if "--emit=obj" in args:
    if fail == "unit" and any(arg.endswith("server.c") for arg in args):
        raise SystemExit(17)
    Path(args[args.index("-o") + 1]).write_text("// test object fixture\\n")
elif "--emit=managedlib" in args:
    output = Path(args[args.index("-o") + 1])
    output.mkdir(parents=True)
    (output / "TranslatedValkey.csproj").write_text("<Project />\\n")
    (output / "Dotcc.SourceFiles.txt").write_text("Core.cs\\n")
    (output / "Core.cs").write_text("// test generated library fixture\\n")
elif args and args[0] == "build":
    project = args[1]
    if fail == "product-build" and "/processed/TranslatedValkey/" in project:
        raise SystemExit(18)
    if fail == "final-build" and "/generated/TranslatedValkey/" in project:
        raise SystemExit(19)
''')
        dotnet.chmod(0o755)
        self.guard = self.repo / "network guard"
        self.guard.mkdir()
        # Every real socket is blocked. Default acquisition may use one exact
        # fixture URL via controlled urllib transport, recorded independently.
        self.transport = self.repo / "download fixture.tar.gz"
        self.transport.write_bytes(self.archive_bytes)
        (self.guard / "sitecustomize.py").write_text('''
import io, os
from pathlib import Path
import socket, urllib.request

def blocked(*args, **kwargs):
    Path(os.environ["TEST_NETWORK_ATTEMPT"]).write_text("socket attempted")
    raise AssertionError("External network is blocked in pipeline tests")

def fixture_fetch(url, *args, **kwargs):
    Path(os.environ["TEST_FETCH_ATTEMPT"]).write_text(str(url))
    if os.environ.get("TEST_ALLOW_FIXTURE_FETCH") != "1":
        raise AssertionError("Acquisition attempted during --no-fetch")
    if url != "https://invalid.example/valkey-fixture.tar.gz":
        raise AssertionError("Unexpected download URL: " + str(url))
    return io.BytesIO(Path(os.environ["TEST_FETCH_FIXTURE"]).read_bytes())

socket.socket.connect = blocked
socket.create_connection = blocked
urllib.request.urlopen = fixture_fetch
''')
        self.env = dict(os.environ, PATH=str(self.bin) + os.pathsep + os.environ["PATH"],
                        PYTHONPATH=str(self.guard), TEST_DOTNET_LOG=str(self.repo / "dotnet.jsonl"),
                        TEST_NETWORK_ATTEMPT=str(self.repo / "network-attempt"),
                        TEST_FETCH_ATTEMPT=str(self.repo / "fetch-attempt"),
                        TEST_FETCH_FIXTURE=str(self.transport), TEST_DOTNET_FAIL="",
                        TEST_ALLOW_FIXTURE_FETCH="0")

    @staticmethod
    def write_json(path, value):
        path.write_text(json.dumps(value, indent=2) + "\n")

    def seed_archive(self):
        self.archive.parent.mkdir(parents=True, exist_ok=True)
        self.archive.write_bytes(self.archive_bytes)

    def translate(self, *args, fail="", allow_fetch=False):
        result = subprocess.run(["/bin/bash", str(self.root / "scripts/translate.sh"), *args],
                                env=dict(self.env, TEST_DOTNET_FAIL=fail,
                                         TEST_ALLOW_FIXTURE_FETCH="1" if allow_fetch else "0"),
                                text=True, capture_output=True, timeout=30)
        self.assertFalse(Path(self.env["TEST_NETWORK_ATTEMPT"]).exists(), result.stderr)
        if not allow_fetch:
            self.assertFalse(Path(self.env["TEST_FETCH_ATTEMPT"]).exists(), result.stderr)
        return result

    def receipt(self):
        latest = json.loads((self.root / "artifacts/campaign/latest-attempt.json").read_text())
        return json.loads(Path(latest["receipt"]).read_text())

    def dotnet_commands(self):
        path = Path(self.env["TEST_DOTNET_LOG"])
        return [json.loads(line) for line in path.read_text().splitlines()] if path.exists() else []

    def seed_product(self):
        for name in ("TranslatedValkey", "TranslatedValkey.Raw"):
            directory = self.root / "generated" / name
            directory.mkdir(parents=True)
            (directory / "previous.cs").write_text("previous " + name)
        return hashes(self.root / "generated")

    def test_default_acquisition_reaches_fetch_and_runs_complete_pipeline(self):
        self.archive.unlink()
        result = self.translate(allow_fetch=True)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertTrue(Path(self.env["TEST_FETCH_ATTEMPT"]).exists())
        receipt = self.receipt()
        self.assertEqual(receipt["status"], "passed")
        self.assertEqual(receipt["status"], "passed")
        self.assertEqual(len(receipt["units"]), 2)
        link = next(args for args in self.dotnet_commands() if "--emit=managedlib" in args)
        self.assertEqual(link[link.index("--namespace") + 1], "Managed.Database")
        self.assertIn("--literal-pool", link)
        self.assertIn("--deduplicate-inline", link)
        self.assertNotIn("--overrides-file", link)
        self.assertTrue(all("--overrides-file" in args for args in self.dotnet_commands() if "--emit=obj" in args))
        self.assertIn("managed_profile", receipt)
        for name in ("TranslatedValkey", "TranslatedValkey.Raw"):
            project = self.root / "generated" / name / "TranslatedValkey.csproj"
            import xml.etree.ElementTree as ET
            include = ET.parse(project).find(".//Compile").get("Include")
            self.assertFalse(Path(include).is_absolute())
            self.assertEqual((project.parent / include).resolve(), self.root / "src/Host/ValkeyHost.cs")
            self.assertFalse((project.parent / "ValkeyHost.cs").exists())
            notice = (project.parent / "UPSTREAM-NOTICES.txt").read_text()
            self.assertIn("Fixture redistribution notice.", notice)
            self.assertIn("Copyright fixture author", notice)
            self.assertNotIn("int server(void)", notice)
            self.assertEqual(ET.parse(project).find(".//None").get("CopyToPublishDirectory"), "PreserveNewest")
        builds = [args for args in self.dotnet_commands() if args[0] == "build"]
        self.assertTrue(any(args[1].endswith("DotCC.csproj") for args in builds))
        self.assertTrue(any(args[1].endswith("DotCC.PostProcess.csproj") for args in builds))
        self.assertTrue((self.root / "generated/TranslatedValkey/TranslatedValkey.csproj").is_file())
        self.assertTrue((self.root / "generated/TranslatedValkey.Raw/TranslatedValkey.csproj").is_file())

    def test_managed_profile_includes_bridge_and_preserves_reference_tree(self):
        (self.root / "src").mkdir(exist_ok=True)
        bridge = self.root / "src/bridge.c"
        bridge.write_text("int bridge(void) { return 12; }\n")
        self.write_json(self.root / "config/managed-adaptations.json", {
            "name": "test-managed", "commit": "0123456789abcdef", "adaptations": [],
            "injected_files": [{"source": "src/bridge.c", "path": "src/bridge.c"}],
            "extra_sources": [{"path": "src/bridge.c", "defines": [], "include_dirs": ["src"]}]})
        result = self.translate("--no-fetch", "--no-build-tools", "--managed-profile")
        self.assertEqual(result.returncode, 0, result.stderr)
        receipt = self.receipt()
        self.assertEqual(["/".join(Path(unit).parts[-2:]) for unit in receipt["units"]],
                         ["src/server.c", "src/second.c", "src/bridge.c"])
        self.assertEqual(receipt["managed_profile"]["injected_files"][0]["sha256"], sha(bridge))
        self.assertEqual(hashes(self.tree), self.reference_hashes)
        self.assertTrue((self.root / "generated/TranslatedValkey/TranslatedValkey.csproj").is_file())

    def test_offline_nested_generators_are_deterministic_and_ref_is_immutable(self):
        result = self.translate("--no-fetch", "--no-build-tools")
        self.assertEqual(result.returncode, 0, result.stderr)
        original = hashes(self.tree)
        self.assertEqual(original, self.reference_hashes)
        first = self.receipt()
        self.archive.unlink()
        result = self.translate("--no-fetch", "--no-build-tools")
        self.assertEqual(result.returncode, 0, result.stderr)
        second = self.receipt()
        self.assertEqual(hashes(self.tree), original)
        self.assertNotIn("src/release.h", original)
        self.assertNotIn("src/commands.def", original)
        self.assertEqual(first["generated_inputs"], second["generated_inputs"])
        self.assertEqual(second["status"], "passed")
        for receipt in (first, second):
            executable = receipt["python"]
            generators = [entry for entry in receipt["commands"]
                          if len(entry["command"]) > 1 and entry["command"][1].endswith(".py")]
            self.assertEqual(len(generators), 2)
            self.assertTrue(all(entry["command"][0] == executable for entry in generators))
            staged = self.root / receipt["staging"] / "source"
            self.assertEqual((staged / "generator-interpreters.txt").read_text().splitlines(),
                             [executable, executable])
            self.assertEqual(receipt["generated_inputs"], {name: sha(staged / name)
                for name in ("src/commands.def", "src/fmtargs.h", "src/release.h")})
        self.assertFalse(any(args[0] == "build" and args[1].endswith("DotCC.csproj")
                             for args in self.dotnet_commands()))
        self.assertTrue(any(args[0] == "restore" for args in self.dotnet_commands()))
        self.assertTrue(any(args[0].endswith("dotcc-postprocess.dll") for args in self.dotnet_commands()))

    def test_partial_unit_requires_explicit_probe_before_input_preparation(self):
        result = self.translate("--unit", "src/server.c", "--no-fetch")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("require probe", result.stderr)
        self.assertFalse(self.tree.exists())
        self.assertFalse((self.root / "artifacts/translation").exists())
        self.assertEqual(self.dotnet_commands(), [])

    def test_probe_emits_only_selected_unit_and_never_promotes(self):
        old = self.seed_product()
        result = self.translate("--no-fetch", "--no-build-tools", "--probe", "--unit", "src/second.c")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(hashes(self.root / "generated"), old)
        receipt = self.receipt()
        self.assertEqual(["/".join(Path(unit).parts[-2:]) for unit in receipt["units"]], ["src/second.c"])
        self.assertEqual(receipt["action"], "probe")
        self.assertIn("no product published", receipt["scope"])
        self.assertFalse(any("--emit=managedlib" in args for args in self.dotnet_commands()))

    def test_offline_missing_sources_fail_before_any_tool_execution(self):
        self.archive.unlink()
        result = self.translate("--no-fetch", "--no-build-tools")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Missing source", result.stderr)
        self.assertEqual(self.receipt()["status"], "failed")
        self.assertEqual(self.dotnet_commands(), [])
        self.assertFalse((self.root / "generated").exists())

    def test_translation_failure_preserves_both_previous_outputs(self):
        old = self.seed_product()
        result = self.translate("--no-fetch", "--no-build-tools", fail="unit")
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(hashes(self.root / "generated"), old)
        self.assertEqual(self.receipt()["status"], "failed")
        self.assertTrue(self.receipt()["previous_product_stale_for_attempt"])
        self.assertFalse(any("--emit=managedlib" in args for args in self.dotnet_commands()))

    def test_processed_build_failure_does_not_promote(self):
        old = self.seed_product()
        result = self.translate("--no-fetch", "--no-build-tools", fail="product-build")
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(hashes(self.root / "generated"), old)
        self.assertEqual(self.receipt()["status"], "failed")
        self.assertIn("semantic-context-build.log", self.receipt()["error"])

    def test_final_path_build_failure_rolls_back_both_outputs(self):
        old = self.seed_product()
        result = self.translate("--no-fetch", "--no-build-tools", fail="final-build")
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(hashes(self.root / "generated"), old)
        self.assertEqual(self.receipt()["status"], "failed")
        self.assertIn("final-processed-build.log", self.receipt()["error"])


if __name__ == "__main__":
    unittest.main()

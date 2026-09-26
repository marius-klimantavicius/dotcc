"""Input acquisition checks; fixture downloads cannot contact external services."""
import hashlib
import importlib.util
import io
import json
import os
from pathlib import Path
import shlex
import shutil
import subprocess
import sys
import tarfile
import tempfile
import unittest
from unittest.mock import patch

SCRIPTS = Path(__file__).resolve().parents[1] / "scripts"
spec = importlib.util.spec_from_file_location("valkey_inputs", SCRIPTS / "inputs.py")
inputs = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inputs)


class InputTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="valkey inputs with spaces ")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "campaign with spaces"
        (self.root / "config").mkdir(parents=True)
        shutil.copytree(SCRIPTS, self.root / "scripts", ignore=shutil.ignore_patterns("__pycache__"))
        # This suite covers the opt-in strict acquisition API. Shared command
        # dispatch and relaxed modes have their own framework integration tests.
        support = self.root.parent / "Scripts"
        support.mkdir()
        shutil.copy2(SCRIPTS.parents[1] / "Scripts/campaign-common.sh", support / "campaign-common.sh")
        (self.root / "scripts/fetch.sh").write_text(
            '#!/usr/bin/env bash\nsource "$(dirname -- "${BASH_SOURCE[0]}")/../../Scripts/campaign-common.sh"\n'
            'export DOTCC_CAMPAIGN_HASHES=strict\nexec "$PYTHON_CMD" "$(dirname -- "$0")/inputs.py" "$@"\n')
        environment = patch.dict(os.environ, DOTCC_CAMPAIGN_HASHES="strict")
        environment.start()
        self.addCleanup(environment.stop)
        self.tree = self.root / "ref/valkey-fixture"
        self.archive = self.root / "ref/valkey-fixture.tar.gz"
        self.files = {"src/server.c": b"int main(void) { return 0; }\n", "deps/lua/lua.h": b"/* bundled Lua */\n",
                      "utils/generate.py": b"print('generated')\n"}
        data = io.BytesIO()
        with tarfile.open(fileobj=data, mode="w:gz") as tar:
            for name in ["", "src", "deps", "deps/lua", "utils"]:
                entry = tarfile.TarInfo("valkey-fixture" + ("/" + name if name else ""))
                entry.type = tarfile.DIRTYPE
                entry.mode = 0o755
                tar.addfile(entry)
            for name, content in self.files.items():
                entry = tarfile.TarInfo("valkey-fixture/" + name)
                entry.size, entry.mode = len(content), 0o644
                tar.addfile(entry, io.BytesIO(content))
        self.archive_bytes = data.getvalue()
        archive_sha = hashlib.sha256(self.archive_bytes).hexdigest()
        manifest = {"archive_sha256": archive_sha, "directories": ["src", "deps", "deps/lua", "utils"],
                    "files": {name: {"sha256": hashlib.sha256(content).hexdigest(), "size": len(content),
                                     "executable": 0} for name, content in self.files.items()}}
        manifest_path = self.root / "config/source-files.json"
        inputs.write_json(manifest_path, manifest)
        self.config = {"version": "fixture", "commit": "fixture", "directory": self.tree.name,
                       "archive": self.archive.name, "url": "https://invalid.example/pinned.tar.gz",
                       "archive_sha256": archive_sha, "file_manifest": manifest_path.name,
                       "file_manifest_sha256": inputs.digest(manifest_path)}
        inputs.write_json(self.root / "config/source.json", self.config)
        self.network = self.root / "blocked network"
        self.network.mkdir()
        (self.network / "sitecustomize.py").write_text(
            "import pathlib, socket, urllib.request\n"
            "def blocked(*args, **kwargs):\n"
            "    pathlib.Path(" + repr(str(self.network / "attempted")) + ").write_text('attempted')\n"
            "    raise AssertionError('Network operation in --no-fetch')\n"
            "socket.socket.connect = blocked\nsocket.create_connection = blocked\n"
            "urllib.request.urlopen = blocked\n")

    def seed_archive(self):
        self.archive.parent.mkdir(parents=True, exist_ok=True)
        self.archive.write_bytes(self.archive_bytes)

    def cli(self, *args):
        env = dict(os.environ, PYTHONPATH=str(self.network))
        result = subprocess.run(["/bin/bash", str(self.root / "scripts/fetch.sh"), *args],
                                env=env, text=True, capture_output=True)
        self.assertFalse((self.network / "attempted").exists(), result.stderr)
        return result

    def test_default_acquisition_downloads_and_verifies(self):
        with patch.object(inputs.urllib.request, "urlopen", return_value=io.BytesIO(self.archive_bytes)) as download:
            receipt = inputs.prepare_inputs(root=self.root)
        download.assert_called_once_with(self.config["url"], timeout=120)
        self.assertEqual(receipt["fetch_mode"], "fetch")
        self.assertTrue(receipt["download_attempted"])
        self.assertEqual((self.tree / "deps/lua/lua.h").read_bytes(), self.files["deps/lua/lua.h"])

    def test_default_fetch_failure_is_not_offline_fallback(self):
        with patch.object(inputs.urllib.request, "urlopen", side_effect=OSError("fetch blocked")):
            with self.assertRaisesRegex(RuntimeError, "fetch blocked"):
                inputs.prepare_inputs(root=self.root)
        self.assertFalse(self.tree.exists())
        self.assertEqual(json.loads((self.root / "artifacts/inputs.json").read_text())["status"], "failed")

    def test_no_fetch_extracts_verified_archive_with_network_blocked(self):
        self.seed_archive()
        result = self.cli("--no-fetch", "--json")
        self.assertEqual(result.returncode, 0, result.stderr)
        receipt = json.loads(result.stdout)
        self.assertEqual(receipt["source_root"], str(self.tree))
        self.assertEqual(receipt["fetch_mode"], "no-fetch")
        self.assertFalse(receipt["download_attempted"])

    def test_archive_free_tree_uses_trusted_manifest(self):
        self.seed_archive()
        self.assertEqual(self.cli("--no-fetch").returncode, 0)
        self.archive.unlink()
        result = self.cli("--no-fetch", "--json")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(json.loads(result.stdout)["validation_method"], "trusted-file-manifest")
        # Fetch-enabled operation also reuses exact local sources without an unnecessary request.
        self.assertEqual(self.cli().returncode, 0)

    def test_absent_inputs_fail_without_network(self):
        result = self.cli("--no-fetch")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn(str(self.tree), result.stderr)
        self.assertIn(str(self.root / "scripts/fetch.sh"), result.stderr)
        self.assertFalse((self.root / "ref").exists())

    def test_corrupt_archive_is_not_replaced(self):
        self.seed_archive()
        self.archive.write_bytes(b"damaged archive")
        for flags in [("--no-fetch",), ()]:
            result = self.cli(*flags)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn(str(self.archive), result.stderr)
            self.assertEqual(self.archive.read_bytes(), b"damaged archive")

    def test_tampered_or_incomplete_tree_is_never_repaired(self):
        self.seed_archive()
        self.assertEqual(self.cli("--no-fetch").returncode, 0)
        path = self.tree / "deps/lua/lua.h"
        for flags in [("--no-fetch",), ()]:
            path.write_bytes(b"tampered")
            result = self.cli(*flags)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn(str(path), result.stderr)
            self.assertEqual(path.read_bytes(), b"tampered")
        path.unlink()
        self.assertNotEqual(self.cli("--no-fetch").returncode, 0)

    def test_manifest_tampering_rejected_without_archive(self):
        self.seed_archive()
        self.assertEqual(self.cli("--no-fetch").returncode, 0)
        self.archive.unlink()
        (self.root / "config/source-files.json").write_text("{}")
        result = self.cli("--no-fetch")
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("manifest checksum mismatch", result.stderr)

    def test_source_symlink_and_unexpected_files_rejected(self):
        self.seed_archive()
        self.assertEqual(self.cli("--no-fetch").returncode, 0)
        extra = self.tree / "extra.txt"
        extra.write_text("unexpected")
        self.assertNotEqual(self.cli("--no-fetch").returncode, 0)
        extra.unlink()
        extra.symlink_to(self.tree / "src/server.c")
        self.assertNotEqual(self.cli("--no-fetch").returncode, 0)

    def test_missing_manifest_cannot_be_manufactured_from_tree(self):
        self.seed_archive()
        self.assertEqual(self.cli("--no-fetch").returncode, 0)
        self.archive.unlink()
        (self.root / "config/source-files.json").unlink()
        self.assertNotEqual(self.cli("--no-fetch").returncode, 0)

    def test_download_checksum_failure_does_not_publish(self):
        with patch.object(inputs.urllib.request, "urlopen", return_value=io.BytesIO(b"wrong archive")):
            with self.assertRaisesRegex(RuntimeError, "checksum mismatch"):
                inputs.prepare_inputs(root=self.root)
        self.assertFalse(self.archive.exists())
        self.assertFalse(self.tree.exists())

    def test_python_resolution_and_argument_preservation(self):
        bins = self.root / "python executables"
        bins.mkdir()
        (bins / "dirname").symlink_to(shutil.which("dirname"))
        invocation_log = self.root / "python.log"
        for usable in [(True, True), (False, True), (False, False)]:
            invocation_log.write_text("")
            for name, works in zip(("python3", "python"), usable):
                script = bins / name
                script.write_text("#!/bin/bash\necho " + name + " >> " + shlex.quote(str(invocation_log)) +
                                  "\n" + ("exec " + shlex.quote(sys.executable) + ' "$@"\n' if works else "exit 1\n"))
                script.chmod(0o755)
            self.seed_archive()
            result = subprocess.run(["/bin/bash", str(self.root / "scripts/fetch.sh"), "--no-fetch", "--json"],
                                    env=dict(os.environ, PATH=str(bins), PYTHONPATH=str(self.network)),
                                    text=True, capture_output=True)
            if any(usable):
                self.assertEqual(result.returncode, 0, result.stderr)
                self.assertEqual(json.loads(result.stdout)["source_root"], str(self.tree))
                self.assertEqual(invocation_log.read_text().splitlines()[-1], "python3" if usable[0] else "python")
                if usable[0]:
                    self.assertNotIn("python", invocation_log.read_text().splitlines())
            else:
                self.assertNotEqual(result.returncode, 0)
                self.assertIn("Python 3 is required", result.stderr)
            self.assertFalse((self.network / "attempted").exists())


if __name__ == "__main__":
    unittest.main()

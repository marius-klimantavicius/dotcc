"""Validate that the host port preparation fails closed and preserves upstream."""
import hashlib
import importlib.util
from pathlib import Path
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location("host_source", ROOT / "scripts/prepare-host-source.py")
PORT = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PORT)


class HostSourceTests(unittest.TestCase):
    def test_exact_single_substitution_and_reference_unchanged(self):
        original = (PORT.SOURCE / "sqlite3.c").read_bytes()
        with tempfile.TemporaryDirectory() as directory:
            output = PORT.prepare(output=Path(directory))
            adapted = (output / "sqlite3.c").read_bytes()
            self.assertEqual(adapted.replace(PORT.AFTER, PORT.BEFORE), original)
            self.assertEqual(hashlib.sha256(adapted).hexdigest(), PORT.PATCHED_HASH)
        self.assertEqual((PORT.SOURCE / "sqlite3.c").read_bytes(), original)

    def test_unrelated_edit_is_allowed_but_strict_mode_rejects_it(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "ref"
            output = Path(directory) / "out"
            source.mkdir()
            (source / "sqlite3.c").write_bytes((PORT.SOURCE / "sqlite3.c").read_bytes() + b"\n")
            (source / "sqlite3.h").write_bytes((PORT.SOURCE / "sqlite3.h").read_bytes())
            with self.assertRaisesRegex(ValueError, "Hash differs"):
                PORT.prepare(source, output, hash_mode="strict")
            self.assertFalse(output.exists())
            PORT.prepare(source, output)
            self.assertTrue((output / "sqlite3.c").read_bytes().endswith(b"\n"))

    def test_missing_replacement_context_fails_even_with_hashes_off(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "ref"
            source.mkdir()
            (source / "sqlite3.c").write_text("unrelated implementation")
            (source / "sqlite3.h").write_text("header")
            with self.assertRaisesRegex(ValueError, "exactly one"):
                PORT.prepare(source, Path(directory) / "output", hash_mode="off")


if __name__ == "__main__":
    unittest.main()

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

    def test_changed_reference_is_rejected_before_output(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "ref"
            output = Path(directory) / "out"
            source.mkdir()
            (source / "sqlite3.c").write_bytes((PORT.SOURCE / "sqlite3.c").read_bytes() + b"\n")
            (source / "sqlite3.h").write_bytes((PORT.SOURCE / "sqlite3.h").read_bytes())
            with self.assertRaisesRegex(ValueError, "sqlite3.c checksum"):
                PORT.prepare(source, output)
            self.assertFalse(output.exists())


if __name__ == "__main__":
    unittest.main()

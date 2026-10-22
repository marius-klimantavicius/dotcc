"""Guard/preservation checks on actual configured headers, all writes private."""
import hashlib
import importlib.util
import os
from pathlib import Path
import tempfile
import unittest


spec = importlib.util.spec_from_file_location("pglite_atomics", Path(__file__).resolve().parents[2] / "scripts/atomics.py")
atomics = importlib.util.module_from_spec(spec)
spec.loader.exec_module(atomics)


class OverlayTests(unittest.TestCase):
    def setUp(self):
        self.source = Path(os.environ["PGLITE_ATOMICS_SOURCE"]).resolve()
        self.scratch = tempfile.TemporaryDirectory(prefix="overlay-", dir=os.environ["PGLITE_ATOMICS_TEST_WORK"])
        self.addCleanup(self.scratch.cleanup)
        self.work = Path(self.scratch.name)

    def minimal_source(self):
        source = self.work / "configured"
        for name in ("pg_config.h", "c.h", "port/atomics.h", "port/atomics/generic.h", "storage/s_lock.h"):
            target = source / "src/include" / name
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes((self.source / "src/include" / name).read_bytes())
        return source

    def test_preserves_real_upstream_closure_and_algorithms(self):
        original = self.source / "src/include"
        hashes = {str(p.relative_to(original)): hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in original.rglob("*") if p.is_file()}
        include = self.work / "include"
        port = atomics.prepare_overlay(self.source, include)
        self.assertEqual(port["defines"], [atomics.PORT_DEFINE, atomics.SPINLOCK_DEFINE])
        self.assertTrue(port["generic_wrappers_unchanged"])
        self.assertTrue(port["spinlock_generic_unchanged"])
        changed = set()
        for name, digest in hashes.items():
            self.assertEqual(hashlib.sha256((original / name).read_bytes()).hexdigest(), digest)
            if hashlib.sha256((include / name).read_bytes()).hexdigest() != digest:
                changed.add(name)
        self.assertEqual(changed, {"port/atomics.h", "storage/s_lock.h"})
        added = {str(p.relative_to(include)) for p in include.rglob("*") if p.is_file()} - hashes.keys()
        self.assertEqual(added, {"port/atomics/generic-dotcc-c11.h", "storage/s_lock-dotcc-c11.h"})

    def test_missing_and_duplicate_selection_contexts_fail_before_copy(self):
        for header, anchor in (
                ("port/atomics.h", '#if defined(__arm__) || defined(__arm) || defined(__aarch64__)\n#include "port/atomics/arch-arm.h"'),
                ("port/atomics.h", '#if defined(__GNUC__) || defined(__INTEL_COMPILER)\n#include "port/atomics/generic-gcc.h"'),
                ("storage/s_lock.h", '#if defined(__GNUC__) || defined(__INTEL_COMPILER)\n')):
            for duplicate in (False, True):
                with self.subTest(header=header, duplicate=duplicate):
                    source = self.minimal_source()
                    path = source / "src/include" / header
                    original = path.read_text()
                    path.write_text(original + anchor if duplicate else original.replace(anchor, "", 1))
                    destination = self.work / "rejected"
                    with self.assertRaisesRegex(RuntimeError, "selection context changed"):
                        atomics.prepare_overlay(source, destination)
                    self.assertFalse(destination.exists())

    def test_overlapping_paths_are_rejected(self):
        for destination in (self.source, self.source / "private-overlay", self.source.parent):
            with self.subTest(destination=destination), self.assertRaisesRegex(RuntimeError, "independent"):
                atomics.prepare_overlay(self.source, destination)

    def test_existing_destination_is_preserved(self):
        destination = self.work / "existing"
        destination.mkdir()
        marker = destination / "keep"
        marker.write_text("owned by an earlier run")
        with self.assertRaises(FileExistsError):
            atomics.prepare_overlay(self.source, destination)
        self.assertEqual(list(destination.iterdir()), [marker])
        self.assertEqual(marker.read_text(), "owned by an earlier run")


if __name__ == "__main__":
    unittest.main(verbosity=2)

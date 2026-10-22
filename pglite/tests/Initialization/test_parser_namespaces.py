"""Guarded parser staging against readonly configured upstream inputs."""
import json
from pathlib import Path
import re
import tempfile
from types import SimpleNamespace
import unittest

from campaigns.recipes import helper

ROOT = Path(__file__).resolve().parents[2]
PORTABILITY = helper(ROOT / "scripts/portability.py")
INITIALIZER = helper(ROOT / "scripts/initializer.py")


class ParserNamespaceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = Path(INITIALIZER.configured(SimpleNamespace(root=ROOT))["source"])
        cls.rules = json.loads((ROOT / "config/portable-codegen.json").read_text())["parser_semantic_types"]

    def test_real_parser_bodies_stay_identical_and_generated_guards_use_distinct_tags(self):
        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary) / "parser-inputs"
            report = PORTABILITY.prepare_parser_units(ROOT, self.source, destination)
            self.assertEqual(len(report["units"]), 6)
            self.assertEqual(len(report["files_sha256"]), 9)
            for rule in self.rules:
                original = (self.source / rule["generated_header"]).read_text()
                staged = (destination / rule["generated_header"]).read_text()
                self.assertEqual(staged, re.sub(r"\bYYSTYPE\b", rule["tag"], original))
                self.assertIn("! defined " + rule["tag"], staged)
                for unit in rule["units"]:
                    self.assertEqual((destination / unit).read_bytes(), (self.source / unit).read_bytes())
                    self.assertEqual(report["units"][unit]["defines"], ["YYSTYPE=" + rule["tag"]])

    def test_bad_last_generated_anchor_writes_nothing(self):
        for replacement in ("missing", "YYSTYPE YYSTYPE"):
            with self.subTest(replacement=replacement), tempfile.TemporaryDirectory() as temporary:
                source = Path(temporary) / "source"
                for rule in self.rules:
                    for name in [rule["generated_header"], *rule["units"]]:
                        target = source / name
                        target.parent.mkdir(parents=True, exist_ok=True)
                        target.write_bytes((self.source / name).read_bytes())
                last = source / self.rules[-1]["generated_header"]
                last.write_text(last.read_text().replace("union YYSTYPE", "union " + replacement))
                destination = Path(temporary) / "output"
                with self.assertRaisesRegex(RuntimeError, "exact YYSTYPE"):
                    PORTABILITY.prepare_parser_units(ROOT, source, destination)
                self.assertFalse(destination.exists())

    def test_source_overlap_and_existing_outputs_are_preserved(self):
        for destination in (self.source, self.source.parent, self.source / "private"):
            with self.subTest(destination=destination), self.assertRaisesRegex(RuntimeError, "separate"):
                PORTABILITY.prepare_parser_units(ROOT, self.source, destination)
        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary)
            marker = destination / "owned.txt"
            marker.write_text("preserve")
            with self.assertRaisesRegex(RuntimeError, "already exists"):
                PORTABILITY.prepare_parser_units(ROOT, self.source, destination)
            self.assertEqual(marker.read_text(), "preserve")

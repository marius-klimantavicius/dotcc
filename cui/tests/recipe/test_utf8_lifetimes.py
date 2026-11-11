"""Reject accidental arena fallbacks outside the reviewed retention exceptions."""
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts/managed-api"))
from utf8_overloads import generate


class Utf8LifetimeContracts(unittest.TestCase):
    def test_checked_in_overloads_reproduce_with_retention_exceptions(self):
        wrapper = ROOT / "src/ManagedCui"
        self.assertEqual(generate(wrapper), (wrapper / "Ui.Utf8.g.cs").read_bytes())

    def test_scalar_arena_regressions_are_rejected(self):
        wrapper = ROOT / "src/ManagedCui"
        source = (wrapper / "Ui.Api.cs").read_text()
        cases = [
            ("ProgressRing", "cui_progress_ring(_ctx, __labelPointer,", "cui_progress_ring(_ctx, Utf8(label),"),
            ("RangeCalendar", "cui_range_calendar(_ctx, __idPointer,", "cui_range_calendar(_ctx, Utf8(id),"),
            ("CodeBlock", "cui_code_block_ex(_ctx, __idPointer, __codePointer,", "cui_code_block_ex(_ctx, __idPointer, Utf8(code),"),
            ("ChatMessage", "cui_chat_message_ex(_ctx, __textPointer,", "cui_chat_message_ex(_ctx, Utf8(text),"),
        ]
        for name, original, regression in cases:
            with self.subTest(method=name):
                self.assertIn(original, source)
                with self.assertRaisesRegex(ValueError, name + r"\(.*scalar still uses frame-arena"):
                    generate(wrapper, {"Ui.Api.cs": source.replace(original, regression, 1).encode()})


if __name__ == "__main__":
    unittest.main()

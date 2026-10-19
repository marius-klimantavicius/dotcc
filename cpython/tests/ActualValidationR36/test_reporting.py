"""Controls for count inflation, interrupted journals and mixed subtest outcomes."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
from reporting import count_journal, validate_counts


class JournalTests(unittest.TestCase):
    def count(self, rows, required):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "journal.jsonl"
            path.write_text("".join(json.dumps({"case": c, "status": s}) + "\n" for c, s in rows))
            return count_journal(path, required)

    def test_duplicate_pass_events_are_one_unique_pass(self):
        result = self.count([("a", "started"), ("a", "passed"), ("a", "started"),
                             ("a", "passed"), ("b", "started"), ("b", "required-skip")], ["a", "b"])
        self.assertEqual(result["uniqueOutcomes"], {"passed": 1, "required-skip": 1})
        self.assertEqual(result["outcomeEvents"]["passed"], 2)
        impossible = copy.deepcopy(result)
        impossible["uniqueOutcomes"]["passed"] = 2
        with self.assertRaisesRegex(ValueError, "do not sum"):
            validate_counts(impossible)

    def test_interruption_and_unstarted_are_distinct(self):
        result = self.count([("a", "started"), ("a", "passed"), ("b", "started")], ["a", "b", "c"])
        self.assertEqual(result["fatalStartedWithoutOutcome"], ["b"])
        self.assertEqual(result["neverStarted"], 1)
        self.assertEqual(result["withoutCompletedOutcome"], 2)

    def test_mixed_subtest_and_pass_never_becomes_pass(self):
        result = self.count([("a", "started"), ("a", "failed-subtest"), ("a", "passed")], ["a"])
        self.assertEqual(result["uniqueOutcomes"], {"failed-subtest": 1})
        self.assertIn("a", result["mixedStatusIds"])

    def test_unexpected_fixture_error_is_visible(self):
        result = self.count([("setUpClass (foreign)", "error")], ["a"])
        self.assertEqual(result["unexpectedIds"], ["setUpClass (foreign)"])
        self.assertEqual(result["outcomesWithoutStart"], ["setUpClass (foreign)"])
        self.assertEqual(result["uniqueCompleted"], 0)


if __name__ == "__main__":
    unittest.main()

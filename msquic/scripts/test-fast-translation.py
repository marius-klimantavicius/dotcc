#!/usr/bin/env python3
"""Fast and qualified recipes share emission but retain distinct prerequisites."""
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Scripts"))
from campaigns.recipes import helper

MODULE = helper(Path(__file__).with_name("campaign.py"))

class FastTranslationTests(unittest.TestCase):
    def context(self, fast=False, action="translate"):
        calls = []
        context = SimpleNamespace(options=SimpleNamespace(fast=fast, action=action),
                                  artifacts=Path("attempt"), receipt={},
                                  script=lambda name, *args, **kwargs: calls.append((name, args)))
        return context, calls

    def test_fast_has_no_qualification_claim_or_freeze(self):
        context, calls = self.context(fast=True)
        MODULE.gates(context)
        MODULE.finish(context)
        self.assertEqual(calls, [])

    def test_qualified_path_runs_fresh_host_and_public_abi_gates(self):
        context, calls = self.context()
        MODULE.gates(context)
        self.assertEqual(calls, [("test-host-contract.py", ("--no-fetch",)),
                                 ("test-abi.py", ("--groups", "public", "--no-fetch"))])

    def test_qualified_delivery_tests_existing_output_before_freezing(self):
        context, calls = self.context()
        MODULE.finish(context)
        self.assertEqual(calls, [("build-product.py", ("--existing",)),
                                 ("freeze-product.py", ("--without-sqlite", "--output", Path("attempt/qualified-closure.json")))])
        self.assertEqual(context.receipt["qualification"], "attempt/qualified-closure.json")

    def test_probe_never_qualifies_or_freezes_product(self):
        context, calls = self.context(action="probe")
        MODULE.gates(context)
        MODULE.finish(context)
        self.assertEqual(calls, [])

if __name__ == "__main__":
    unittest.main()

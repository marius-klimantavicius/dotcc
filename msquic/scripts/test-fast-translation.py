#!/usr/bin/env python3
"""Fast and qualified recipes share emission but retain distinct prerequisites."""
from pathlib import Path
import hashlib
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Scripts"))
from campaigns.recipes import helper

MODULE = helper(Path(__file__).with_name("campaign.py"))

class FastTranslationTests(unittest.TestCase):
    def context(self, fast=False, action="translate"):
        calls = []
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        artifacts = Path(temporary.name)
        root = Path(__file__).resolve().parents[1]

        def script(name, *args, **kwargs):
            calls.append((name, args))
            if name == "freeze-product.py":
                Path(args[args.index("--output") + 1]).write_text('{"fresh": true}\n')

        context = SimpleNamespace(options=SimpleNamespace(fast=fast, action=action),
                                  root=root, repo=root.parent,
                                  artifacts=artifacts, receipt={}, script=script)
        return context, calls

    def test_fast_has_no_qualification_claim_or_freeze(self):
        context, calls = self.context(fast=True)
        MODULE.gates(context)
        MODULE.finish(context)
        self.assertEqual(calls, [])

    def test_qualified_path_runs_fresh_host_and_public_abi_gates(self):
        context, calls = self.context()
        provenance = SimpleNamespace(picotls_provenance=Mock(return_value={"current_binding": {"fresh": True}}))
        with patch.object(MODULE, "helper", return_value=provenance) as helper:
            MODULE.gates(context)
        helper.assert_called_once_with(context.root / "scripts/provenance.py")
        provenance.picotls_provenance.assert_called_once_with(context.repo / "picotls")
        self.assertEqual(context.receipt["picotls_current_binding"], {"fresh": True})
        self.assertEqual(calls, [("test-host-contract.py", ("--no-fetch",)),
                                 ("test-abi.py", ("--groups", "public", "--no-fetch"))])

    def test_qualified_delivery_tests_existing_output_before_freezing(self):
        context, calls = self.context()
        MODULE.finish(context)
        closure = context.artifacts / "qualified-closure.json"
        self.assertEqual(calls, [("build-product.py", ("--existing",)),
                                 ("freeze-product.py", ("--without-sqlite", "--output", closure))])
        self.assertEqual(context.receipt["qualification"], str(closure))
        self.assertEqual(context.receipt["qualification_sha256"], hashlib.sha256(closure.read_bytes()).hexdigest())

    def test_probe_never_qualifies_or_freezes_product(self):
        context, calls = self.context(action="probe")
        MODULE.gates(context)
        MODULE.finish(context)
        self.assertEqual(calls, [])

if __name__ == "__main__":
    unittest.main()

"""Managed builds retain the campaign's one-node resource budget."""
from pathlib import Path
import sys
import unittest
from unittest.mock import Mock

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.runner import Context


class ManagedCommands(unittest.TestCase):
    def test_build_and_publish_limit_nodes_and_forward_arguments(self):
        ctx = Context.__new__(Context)
        ctx.run = Mock(return_value="completed")
        for action in ("build", "publish"):
            with self.subTest(action=action):
                result = ctx.managed(action, "Consumer.csproj", "consumer", "--artifacts-path", "external", timeout=1800)
                command, label, timeout = ctx.run.call_args.args
                self.assertEqual(command[:3], ["dotnet", action, "Consumer.csproj"])
                self.assertIn("-m:1", command)
                self.assertEqual(command[-2:], ["--artifacts-path", "external"])
                self.assertEqual((label, timeout, result), ("consumer", 1800, "completed"))


if __name__ == "__main__":
    unittest.main()

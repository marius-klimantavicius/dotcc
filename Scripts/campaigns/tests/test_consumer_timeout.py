"""A consumer's declared qualification deadline must reach its actual run."""
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import Mock, patch

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from campaigns.model import Consumer, Suite
from campaigns.testing import consumer


class ConsumerTimeoutTests(unittest.TestCase):
    def test_consumer_uses_declared_suite_timeout(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "samples/ManagedConsumer/ManagedConsumer.csproj"
            project.parent.mkdir(parents=True)
            project.write_text("<Project><PropertyGroup><AssemblyName>Fixture</AssemblyName></PropertyGroup></Project>")
            ctx = SimpleNamespace(recipe=SimpleNamespace(consumer=Consumer()), root=root, work=root / "work",
                                  options=SimpleNamespace(form="processed", mode="jit"), managed=Mock(), run=Mock())
            with patch("campaigns.testing.check_product", return_value=root / "product.csproj"):
                consumer(ctx, Suite("consumer", (), timeout=321))
            self.assertEqual(ctx.run.call_count, 1)
            self.assertEqual(ctx.run.call_args.kwargs, dict(timeout=321, separate=True))


if __name__ == "__main__":
    unittest.main()

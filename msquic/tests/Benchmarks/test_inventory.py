#!/usr/bin/env python3
"""Reject generated-source drift before and after a benchmark campaign."""
import importlib.util
from pathlib import Path
import tempfile
import unittest


spec = importlib.util.spec_from_file_location('benchmark', Path(__file__).resolve().parents[2] / 'scripts/benchmark-product.py')
benchmark = importlib.util.module_from_spec(spec)
spec.loader.exec_module(benchmark)


class InventoryControls(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='dotcc-benchmark-inventory-')
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        for name, value in {'A.cs': 'class A {}\n', 'Dotcc.SourceFiles.txt': 'A.cs\n',
                            'Library.csproj': '<Project />\n'}.items():
            (self.directory / name).write_text(value)
        self.recorded = {p.name: benchmark.sha(p) for p in self.directory.iterdir()}
        benchmark.validate_generated(self.directory, self.recorded)

    def reject(self):
        with self.assertRaises(RuntimeError):
            benchmark.validate_generated(self.directory, self.recorded)

    def test_sdk_intermediates_are_not_product_sources(self):
        for name in ('bin/Release/Metadata.cs', 'obj/Release/AssemblyInfo.cs'):
            path = self.directory / name
            path.parent.mkdir(parents=True)
            path.write_text('class SdkMetadata {}')
        benchmark.validate_generated(self.directory, self.recorded)

    def test_new_top_level_source(self):
        (self.directory / 'Extra.cs').write_text('class Extra {}')
        self.reject()

    def test_new_nested_source_after_initial_validation(self):
        (self.directory / 'nested').mkdir()
        (self.directory / 'nested/Extra.cs').write_text('class Extra {}')
        self.reject()

    def test_missing_source(self):
        (self.directory / 'A.cs').unlink()
        self.reject()

    def test_changed_source(self):
        (self.directory / 'A.cs').write_text('class Changed {}')
        self.reject()

    def test_duplicate_manifest_entry(self):
        (self.directory / 'Dotcc.SourceFiles.txt').write_text('A.cs\nA.cs\n')
        self.reject()


if __name__ == '__main__':
    unittest.main(verbosity=2)

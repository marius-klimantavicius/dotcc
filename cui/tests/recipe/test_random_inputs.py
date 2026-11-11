"""Upstream additions must extend, rather than invalidate, group coverage."""
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location('cui_random_groups', Path(__file__).parents[1] / 'qualification/random_inputs.py')
random_inputs = importlib.util.module_from_spec(spec)
spec.loader.exec_module(random_inputs)


class RandomGroups(unittest.TestCase):
    def test_new_upstream_group_is_required_in_order(self):
        source = 'static void helper(void) {\n    test_not_a_group();\n}\nint main(void) {\n    test_color();\n    test_keyboard();\n}\n'
        self.assertEqual(random_inputs.expected_groups(source), ['test_color', 'test_keyboard'])
        expanded = source.replace('    test_keyboard();', '    test_copy_button_focus();\n    test_keyboard();')
        expected = random_inputs.expected_groups(expanded)
        self.assertEqual(expected, ['test_color', 'test_copy_button_focus', 'test_keyboard'])
        observed_old = random_inputs.groups('QCOUNT test_color 5\nQCOUNT test_keyboard 10\n')
        self.assertNotEqual([name for name, _ in observed_old], expected)

    def test_invalid_inventory_is_rejected(self):
        for source in ['void helper(void) {}', 'int main(void) {}', 'int main(void) {\n    test_color();\n    test_color();\n}']:
            with self.subTest(source=source), self.assertRaises(RuntimeError):
                random_inputs.expected_groups(source)

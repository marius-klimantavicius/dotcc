#!/usr/bin/env python3
"""Test-only adapter controls: compiler failures must never count as negatives."""

import unittest
from oracle import matches_expectation, oracle_command


class OracleStatusTests(unittest.TestCase):
    def test_normal_acceptance(self):
        self.assertTrue(matches_expectation("accept", 0))
        self.assertFalse(matches_expectation("reject", 0))

    def test_language_error_is_rejection(self):
        self.assertTrue(matches_expectation("reject", 2))
        self.assertFalse(matches_expectation("accept", 2))

    def test_catastrophe_is_not_an_expected_language_rejection(self):
        self.assertFalse(matches_expectation("reject", 4))

    def test_arbitrary_failure_is_not_a_pass(self):
        for exit_code in (1, 3, 127, -6, -11):
            self.assertFalse(matches_expectation("accept", exit_code))
            self.assertFalse(matches_expectation("reject", exit_code))

    def test_selected_bodies_are_instantiated_by_default(self):
        command = oracle_command("compiler", "source.cpp")
        self.assertEqual(command[command.index("--instantiate") + 1], "used")
        self.assertIn("--no_code_gen", command)
        self.assertEqual(command[-1], "source.cpp")

    def test_none_profile_is_explicit(self):
        command = oracle_command("compiler", "source.cpp", "none")
        self.assertEqual(command[command.index("--instantiate") + 1], "none")

    def test_preprocess_preserves_template_profile(self):
        command = oracle_command("compiler", "source.cpp", preprocess=True)
        self.assertIn("--preprocess", command)
        self.assertEqual(command[command.index("--instantiate") + 1], "used")

    def test_explicit_preprocessor_configuration_preserves_argument_boundaries(self):
        command = oracle_command("compiler", "source.cpp", preprocess=True,
                                 include_dirs=["quote path", "next"],
                                 system_include_dirs=["system"],
                                 definitions=["VALUE=two tokens", "F(x)=x"],
                                 undefinitions=["OLD"], no_line_commands=True)
        expected = ["--no_line_commands",
                         "--include_directory", "quote path",
                         "--include_directory", "next",
                         "--sys_include", "system",
                         "--define_macro", "VALUE=two tokens",
                         "--define_macro", "F(x)=x",
                         "--undefine_macro", "OLD", "source.cpp"]
        self.assertEqual(command[-len(expected):], expected)


if __name__ == "__main__":
    unittest.main()

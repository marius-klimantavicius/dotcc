"""Source inventory and receipt-context guards; no compiler or acquisition."""
import importlib.util
from pathlib import Path
import tempfile
import unittest


SCRIPT = Path(__file__).resolve().parents[2] / "scripts/snowball-probe.py"
SPEC = importlib.util.spec_from_file_location("snowball_probe", SCRIPT)
PROBE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PROBE)


class SourceInventoryTests(unittest.TestCase):
    def test_only_makefile_objects_are_selected_and_vpath_resolves(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            snowball = root / "src/backend/snowball"
            (snowball / "libstemmer").mkdir(parents=True)
            (snowball / "dict_snowball.c").write_text("/* original module */")
            (snowball / "libstemmer/api.c").write_text("/* original API */")
            (snowball / "unused.c").write_text("/* must not be selected */")
            (snowball / "Makefile").write_text("OBJS = $(WIN32RES) \\\n api.o\nOBJS += dict_snowball.o\n")
            self.assertEqual(PROBE.makefile_sources(root), [
                "src/backend/snowball/libstemmer/api.c", "src/backend/snowball/dict_snowball.c"])

    def test_missing_ambiguous_duplicate_and_dynamic_sources_fail(self):
        for kind in ("missing", "ambiguous", "duplicate", "dynamic"):
            with self.subTest(kind=kind), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                snowball = root / "src/backend/snowball"
                (snowball / "libstemmer").mkdir(parents=True)
                if kind != "missing":
                    (snowball / "libstemmer/api.c").write_text("")
                if kind == "ambiguous":
                    (snowball / "api.c").write_text("")
                objects = "api.o api.o" if kind == "duplicate" else "$(UNKNOWN)" if kind == "dynamic" else "api.o"
                (snowball / "Makefile").write_text("OBJS = " + objects + "\n")
                with self.assertRaises(RuntimeError):
                    PROBE.makefile_sources(root)


class ReceiptContextTests(unittest.TestCase):
    def receipt(self, root):
        include = root / "original-overlay"
        include.mkdir()
        common = ["-std=c11", "-DPGLITE_DOTCC_C11_ATOMICS", "-DPGLITE_DOTCC_C11_SPINLOCKS",
                  "-I", str(include), "-D__PGLITE__", "-Dexit=pgl_exit"]
        return {"pglite_preparation": {"source": str(root)},
                "pglite_atomics_overlay": {"include": str(include)},
                "commands": [
                    {"exit_code": 0, "command": ["dotnet", "immutable.dll", *common,
                        "-DBACKEND_SPECIFIC", str(root / "src/backend/unit.c"), "--emit=obj", "-o", "a.cs"]},
                    {"exit_code": 0, "command": ["dotnet", "immutable.dll", *common,
                        "-I", "module-only", str(root / "dotcc_plpgsql_module.c"), "--emit=obj", "-o", "b.cs"]}]}, common

    def test_original_overlay_and_common_flags_are_preserved(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            receipt, expected = self.receipt(root)
            flags, include = PROBE.backend_context(receipt)
            self.assertEqual(flags, expected)
            self.assertEqual(include, root / "original-overlay")
            self.assertNotIn("module-only", flags)
            self.assertNotIn("-DBACKEND_SPECIFIC", flags)

    def test_common_prefix_does_not_retain_an_incomplete_include_option(self):
        with tempfile.TemporaryDirectory() as directory:
            receipt, expected = self.receipt(Path(directory))
            command = receipt["commands"][0]["command"]
            position = command.index("-DBACKEND_SPECIFIC")
            command[position:position + 1] = ["-I", "backend-only"]
            flags, _ = PROBE.backend_context(receipt)
            self.assertEqual(flags, expected)

    def test_unavailable_or_mixed_overlay_context_is_rejected(self):
        for kind in ("missing-directory", "different-overlay", "failed-object"):
            with self.subTest(kind=kind), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                receipt, _ = self.receipt(root)
                if kind == "missing-directory":
                    (root / "original-overlay").rmdir()
                elif kind == "different-overlay":
                    receipt["commands"][1]["command"][7] = str(root / "fresh-overlay")
                else:
                    receipt["commands"][0]["exit_code"] = 1
                with self.assertRaises(RuntimeError):
                    PROBE.backend_context(receipt)


class CaseInputTests(unittest.TestCase):
    def test_lengths_are_utf8_bytes_and_embedded_nuls_survive(self):
        header = PROBE.case_header({"rounds": 2, "allocation_cycles": 2,
                                   "cases": [{"language": "lithuanian", "input": "ž\0a", "expected": "ž"}]})
        self.assertIn("197,190,0,97,0", header)
        self.assertIn("{1,dsb_input_0,4,dsb_expected_0,2}", header)

    def test_unreviewed_language_and_vacuous_controls_are_rejected(self):
        for selection in ({"rounds": 1, "allocation_cycles": 2, "cases": []},
                          {"rounds": 2, "allocation_cycles": 2,
                           "cases": [{"language": "invented", "input": "x", "expected": "x"}]}):
            with self.assertRaises(RuntimeError):
                PROBE.case_header(selection)


if __name__ == "__main__":
    unittest.main()

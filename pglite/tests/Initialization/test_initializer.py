"""Structural tests against an existing readonly configured source stage.

Run with PYTHONPATH=Scripts and PGLITE_INITIALIZATION_PREPARATION if selection
must be explicit. This test never acquires, prepares, or executes PostgreSQL.
"""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest


ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location("pglite_initializer", ROOT / "scripts/initializer.py")
INITIALIZER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(INITIALIZER)


class InitializerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.info = INITIALIZER.configured(SimpleNamespace(root=ROOT))
        cls.source = Path(cls.info["source"])
        cls.original = (cls.source / "src/include/pg_config.h").read_bytes()

    def test_frontend_closure_covers_both_native_libraries_and_real_option_parsers(self):
        policy = INITIALIZER.policy(ROOT)
        native = {unit["source"] for key in ("native_frontend", "native_libpq") for unit in policy[key]}
        selected = {unit["source"] for unit in policy["units"]}
        self.assertTrue(native.issubset(selected))
        self.assertTrue({"src/port/getopt.c", "src/port/getopt_long.c"}.issubset(selected))
        self.assertEqual(len(selected), len(policy["units"]))
        self.assertFalse(any(path.startswith("src/backend/") for path in selected))
        for path in selected:
            self.assertTrue((self.source / path).is_file(), path)

    def test_shared_overlay_satisfies_frontend_contract_without_mutating_the_source(self):
        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary) / "include"
            result = INITIALIZER.prepare_frontend_overlay(ROOT, self.source, destination)
            staged = (destination / "pg_config.h").read_text()
            for macro in INITIALIZER.policy(ROOT)["frontend_configuration"]:
                self.assertEqual(staged.splitlines().count("/* #undef " + macro + " */"), 1)
            self.assertTrue(result["frontend_only"])
            self.assertFalse(result["source_mutated"])
            self.assertEqual((self.source / "src/include/pg_config.h").read_bytes(), self.original)
            units = INITIALIZER.frontend_units(ROOT, self.info, destination)
            for unit in units:
                self.assertEqual(unit["includes"][0], str(destination))
                self.assertIn("FRONTEND", unit["defines"])
                self.assertIn("USE_PRIVATE_ENCODING_FUNCS", unit["defines"])
                self.assertIn("JSONAPI_USE_PQEXPBUFFER", unit["defines"])

    def test_missing_and_duplicate_config_anchors_fail_before_creating_an_overlay(self):
        macro = next(iter(INITIALIZER.policy(ROOT)["frontend_configuration"]))
        before = "#define " + macro + " 1"
        for replacement in ("/* missing */", before + "\n" + before):
            with self.subTest(replacement=replacement), tempfile.TemporaryDirectory() as temporary:
                source = Path(temporary) / "source"
                include = source / "src/include"
                include.mkdir(parents=True)
                (include / "pg_config.h").write_text(self.original.decode().replace(before, replacement))
                output = Path(temporary) / "output"
                with self.assertRaisesRegex(RuntimeError, "exactly one"):
                    INITIALIZER.prepare_frontend_overlay(ROOT, source, output)
                self.assertFalse(output.exists())

    def test_overlay_and_data_paths_cannot_overlap_readonly_inputs(self):
        for destination in (self.source, self.source.parent, self.source / "private"):
            with self.subTest(destination=destination), self.assertRaisesRegex(RuntimeError, "independent"):
                INITIALIZER.prepare_frontend_overlay(ROOT, self.source, destination)
        prefix = Path(self.info["prefix"])
        with self.assertRaisesRegex(RuntimeError, "independent"):
            INITIALIZER.stage_bootstrap_data(ROOT, self.info, prefix / "private")

    def test_data_staging_copies_actual_upstream_bytes_and_a_static_provider_manifest(self):
        with tempfile.TemporaryDirectory() as temporary:
            output = Path(temporary) / "prefix"
            report = INITIALIZER.stage_bootstrap_data(ROOT, self.info, output)
            self.assertFalse(report["native_libraries_copied"])
            self.assertFalse(report["module_execution_qualified"])
            self.assertTrue((output / "COPYRIGHT").is_file())
            self.assertGreater(len(report["files"]), 15)
            for entry in report["files"]:
                self.assertFalse(entry["path"].endswith((".so", ".dll", ".dylib", ".wasm")))
                data = (output / entry["path"]).read_bytes()
                self.assertEqual(hashlib.sha256(data).hexdigest(), entry["sha256"])
                if entry["kind"] == "upstream-bootstrap-data":
                    self.assertEqual(data, (Path(self.info["prefix"]) / entry["path"]).read_bytes())
            module = json.loads((output / "lib/postgresql/plpgsql").read_text())
            self.assertFalse(module["native_library"])
            self.assertIn("Pg_magic_func", module["symbols"])
            snowball = json.loads((output / "lib/postgresql/dict_snowball").read_text())
            self.assertFalse(snowball["native_library"])
            self.assertEqual(snowball["provider"], "dpg_snowball_module")
            self.assertIn("dsnowball_init", snowball["symbols"])
            self.assertNotIn("_PG_init", snowball["symbols"])
            for name in ("initdb", "postgres"):
                descriptor = json.loads((output / "bin" / name).read_text())
                self.assertFalse(descriptor["native_executable"])
                self.assertEqual(descriptor["kind"], "translated-entrypoint-descriptor")
            with self.assertRaisesRegex(RuntimeError, "fresh"):
                INITIALIZER.stage_bootstrap_data(ROOT, self.info, output)

    def test_backend_module_units_preserve_original_sources_and_guard_export_names(self):
        with tempfile.TemporaryDirectory() as temporary:
            ctx = SimpleNamespace(root=ROOT, work=Path(temporary))
            original = self.source / "src/pl/plpgsql/src/pl_handler.c"
            before = original.read_bytes()
            units = INITIALIZER.backend_module_units(ctx, self.info, self.source / "src/include")
            self.assertEqual(len(units), 60)
            wrapper = next(unit for unit in units if unit.path.name == "pl_handler-input.c")
            self.assertIn("#define PG_MAGIC_FUNCTION_NAME dpg_plpgsql_magic", wrapper.path.read_text())
            self.assertIn('#include "' + original.as_posix() + '"', wrapper.path.read_text())
            self.assertEqual(original.read_bytes(), before)
            self.assertEqual({unit.path.name for unit in units[-3:]},
                             {"dotcc_plpgsql_module.c", "dotcc_snowball_module.c", "dotcc_static_modules.c"})
            selection = INITIALIZER.backend_module_configuration(ROOT)
            self.assertEqual(len(selection["dfmgr_defines"]), 4)
            self.assertTrue(selection["same_backend_context"])
            self.assertTrue(all(not any("dpg_static_dl" in flag for flag in unit.flags) for unit in units))
            self.assertEqual([module["name"] for module in selection["modules"]], ["plpgsql", "dict_snowball"])
            snowball = self.source / "src/backend/snowball/dict_snowball.c"
            before_snowball = snowball.read_bytes()
            snowball_wrapper = next(unit for unit in units if unit.path.name == "dict_snowball-input.c")
            text = snowball_wrapper.path.read_text()
            self.assertIn("#define PG_MAGIC_FUNCTION_NAME dpg_snowball_magic", text)
            self.assertNotIn("#define _PG_init", text)
            self.assertIn('#include "' + snowball.as_posix() + '"', text)
            self.assertEqual(snowball.read_bytes(), before_snowball)
            original_stemmers = list((self.source / "src/backend/snowball/libstemmer").glob("*.c"))
            self.assertEqual(len(original_stemmers), 50)
            self.assertEqual({unit.path for unit in units if unit.path in original_stemmers}, set(original_stemmers))
            for unit in units:
                if unit.path in original_stemmers or unit.path == snowball_wrapper.path:
                    self.assertIn(str(self.source / "src/include/snowball/libstemmer"), unit.flags)


if __name__ == "__main__":
    unittest.main()

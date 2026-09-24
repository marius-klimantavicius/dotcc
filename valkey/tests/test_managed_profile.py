"""Exact staging edits and the real C static-module resolver contract."""
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("valkey_pipeline", ROOT / "scripts/pipeline.py")
pipeline = importlib.util.module_from_spec(spec)
spec.loader.exec_module(pipeline)


class ManagedProfileTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.stage = self.root / "stage"
        self.stage.mkdir()
        (self.root / "config").mkdir()
        (self.root / "host.c").write_text("int host(void) { return 42; }\n")
        (self.stage / "first.c").write_text("first old\n")
        (self.stage / "second.c").write_text("second old\n")
        self.profile = {
            "name": "test", "commit": "pinned",
            "adaptations": [{"path": name, "sha256": pipeline.sha(self.stage / name),
                             "purpose": "test exact staging", "replacements": [{"before": "old", "after": "new"}]}
                            for name in ("first.c", "second.c")],
            "injected_files": [{"source": "host.c", "path": "host/host.c"}],
            "extra_sources": [{"path": "host/host.c", "defines": [], "include_dirs": []}],
        }
        self.receipt = {"inputs": {"commit": "pinned"}}

    def apply(self):
        (self.root / "config/managed-adaptations.json").write_text(json.dumps(self.profile))
        pipeline.apply_managed_profile(self.stage, self.receipt, root=self.root)

    def test_edits_and_injections_are_recorded_with_input_and_output_identities(self):
        self.apply()
        self.assertEqual((self.stage / "first.c").read_text(), "first new\n")
        self.assertEqual((self.stage / "host/host.c").read_bytes(), (self.root / "host.c").read_bytes())
        receipt = self.receipt["managed_profile"]
        self.assertEqual(receipt["adaptations"][0]["after_sha256"], pipeline.sha(self.stage / "first.c"))
        self.assertEqual(receipt["injected_files"][0]["sha256"], pipeline.sha(self.root / "host.c"))
        self.assertEqual(receipt["extra_sources"], self.profile["extra_sources"])

    def test_hash_mismatch_rejects_entire_profile_before_any_edits(self):
        (self.stage / "second.c").write_text("unexpected\n")
        with self.assertRaisesRegex(RuntimeError, "hash mismatch"):
            self.apply()
        self.assertEqual((self.stage / "first.c").read_text(), "first old\n")
        self.assertFalse((self.stage / "host").exists())
        self.assertNotIn("managed_profile", self.receipt)

    def test_ambiguous_replacement_rejects_entire_profile(self):
        (self.stage / "second.c").write_text("old old\n")
        self.profile["adaptations"][1]["sha256"] = pipeline.sha(self.stage / "second.c")
        with self.assertRaisesRegex(RuntimeError, "not unique"):
            self.apply()
        self.assertEqual((self.stage / "first.c").read_text(), "first old\n")

    def test_wrong_commit_cannot_apply_a_different_profile(self):
        self.receipt["inputs"]["commit"] = "different"
        with self.assertRaisesRegex(RuntimeError, "source commit"):
            self.apply()

    def test_escaping_injection_is_rejected_before_any_edits(self):
        self.profile["injected_files"][0]["path"] = "../escape.c"
        with self.assertRaisesRegex(RuntimeError, "Unsafe"):
            self.apply()
        self.assertEqual((self.stage / "first.c").read_text(), "first old\n")

    def test_missing_injected_source_cannot_leave_partial_adaptations(self):
        (self.root / "host.c").unlink()
        with self.assertRaises(FileNotFoundError):
            self.apply()
        self.assertEqual((self.stage / "first.c").read_text(), "first old\n")

    @unittest.skipUnless(shutil.which("cc"), "native C compiler is required")
    def test_native_reference_resolver_preserves_callback_signatures_and_rejects_unknown_symbols(self):
        # These test callbacks check pointer identity and invocation only; this
        # does not substitute for running the actual translated Lua engine.
        harness = self.root / "resolver-test.c"
        harness.write_text(r'''
#include "valkey_host.h"
#include <assert.h>
#include <stddef.h>
int ValkeyModule_OnLoad_lua(ValkeyModuleCtx *ctx, ValkeyModuleString **argv, int argc) {
    return (ctx == NULL && argv == NULL) ? argc + 40 : -1;
}
int ValkeyModule_OnUnload_lua(ValkeyModuleCtx *ctx) { return ctx == NULL ? 7 : -1; }
int main(void) {
    void *symbol = (void *)1, *handle = (void *)1;
    assert(valkeyManagedResolveStaticModuleSymbol(&symbol, &handle, "ValkeyModule_OnLoad", "lua") == 0);
    assert(symbol == (void *)ValkeyModule_OnLoad_lua && handle == NULL);
    assert(((int (*)(ValkeyModuleCtx *, ValkeyModuleString **, int))symbol)(NULL, NULL, 2) == 42);
    assert(valkeyManagedResolveStaticModuleSymbol(&symbol, &handle, "ValkeyModule_OnUnload", "lua") == 0);
    assert(symbol == (void *)ValkeyModule_OnUnload_lua && handle == NULL);
    assert(((int (*)(ValkeyModuleCtx *))symbol)(NULL) == 7);
    assert(valkeyManagedResolveStaticModuleSymbol(&symbol, &handle, "missing", "lua") == -1);
    assert(symbol == NULL && handle == NULL);
    assert(valkeyManagedResolveStaticModuleSymbol(&symbol, &handle, "ValkeyModule_OnLoad", "other") == -1);
    assert(symbol == NULL && handle == NULL);
    assert(valkeyManagedResolveStaticModuleSymbol(NULL, &handle, "ValkeyModule_OnLoad", "lua") == -1);
    return 0;
}
''')
        binary = self.root / "resolver-test"
        subprocess.run(["cc", "-std=c11", "-Wall", "-Wextra", "-Werror", "-I", str(ROOT / "tests/native_reference"),
                        str(ROOT / "tests/native_reference/valkey_host.c"), str(harness), "-o", str(binary)], check=True)
        subprocess.run([str(binary)], check=True)


if __name__ == "__main__":
    unittest.main()

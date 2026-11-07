"""R139 source fixtures for R133 transport and both R138 P2 responses.

These tests transform/inspect C bytes only. They never build, emit C#, launch a
guest, execute native code or change any pinned artifact.
"""
import importlib.util
import ast
import os
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
STAGE = Path(os.environ.get('BLINK_X87_ADAPTER_INPUTS', str(ROOT / 'blink/artifacts/jit-implementation/full-jit-20261027-r1/checkpoint-inputs/x87-adapter-bound')))
spec = importlib.util.spec_from_file_location('r133_adapter', Path(__file__).with_name('full_jit_x87_state_bindings.py'))
adapter = importlib.util.module_from_spec(spec)
spec.loader.exec_module(adapter)


class X87StateAdapterTests(unittest.TestCase):
    def original(self, path):
        return (STAGE / Path(path).name).read_bytes().split(adapter.CLONES.encode())[0]

    def test_all_exact_preclone_functions_and_idempotence_rejection(self):
        for path in adapter.PINS:
            raw = self.original(path)
            result = adapter.adapt(path, raw)
            self.assertEqual(result.count(adapter.MARKER.encode()), 1)
            self.assertIn(b'#include "host-x87-state.h"', result)
            with self.assertRaises(ValueError): adapter.adapt(path, result)
            with self.assertRaises(ValueError): adapter.adapt(path, raw + adapter.CLONES.encode())

    def test_complete_native9_preclone_inventory_remains_admissible(self):
        units = sorted(STAGE.glob('*.c'))
        self.assertEqual(len(units), 98)
        changed = []
        for unit in units:
            path = 'blink/' + unit.name
            raw = unit.read_bytes().split(adapter.CLONES.encode())[0]
            result = adapter.adapt(path, raw)
            if result != raw: changed.append(path)
        self.assertEqual(set(changed), set(adapter.PINS))

    def test_every_function_pin_rejects_changed_and_duplicate_source(self):
        self.assertEqual(sum(map(len, adapter.PINS.values())), 91)
        for path, functions in adapter.PINS.items():
            raw = self.original(path); text = raw.decode()
            for name in functions:
                body = adapter.function(text, name)
                changed = body.replace('{', '{\n/* changed source body */', 1)
                with self.assertRaises(ValueError): adapter.adapt(path, text.replace(body, changed, 1).encode())
                with self.assertRaises(ValueError): adapter.adapt(path, raw + b'\n' + body.encode())
                with self.assertRaises(ValueError): adapter.adapt(path, text.replace(body, '', 1).encode())

    def test_signal_pointer_and_validation_before_architectural_mutation(self):
        result = adapter.adapt('blink/signal.c', self.original('blink/signal.c')).decode()
        restore = adapter.function(result, 'SigRestore')
        self.assertLess(restore.index('blink_host_guest_x87_state(m, 1, 2,'), restore.index('m->ip ='))
        self.assertIn('fp = Read64(sf.uc.fpstate);', restore)
        self.assertIn('CopyFromUserRead(m, &sf.fp, fp, sizeof(sf.fp)) == -1', restore)
        self.assertIn('blink_host_guest_x87_state(m, 1, 4, 0, 0);', restore)
        self.assertNotIn('DeserializeLdbl', restore.split('  } else {')[0])
        deliver = adapter.function(result, 'DeliverSignal')
        self.assertLess(deliver.index('CopyToUserWrite(m, sp, &sf, sizeof(sf))'), deliver.index('blink_host_guest_x87_state(m, 1, 22,'))
        self.assertLess(deliver.index('blink_host_guest_x87_state(m, 1, 22,'), deliver.index('Put64(m->sp, sp)'))

    def test_fx_active_image_checked_boundaries_and_inactive_original_defect_retained(self):
        result = adapter.adapt('blink/machine.c', self.original('blink/machine.c')).decode()
        save, restore = adapter.function(result, 'OpFxsave'), adapter.function(result, 'OpFxrstor')
        self.assertIn('if (address & 15) ThrowProtectionFault(m);', save)
        self.assertIn('CopyToUserWrite(m, address, image, 416)', save)
        self.assertIn('CopyFromUserRead(m, image, address, 416)', restore)
        self.assertLess(restore.index('CopyFromUserRead'), restore.index('blink_host_guest_x87_state'))
        # Historical compatibility defect is preserved, not newly qualified.
        self.assertIn('CopyToUser(m, v + 32, m->fpu.st, 128);', save)
        self.assertIn('CopyFromUser(m, m->fpu.st, v + 32, 128);', restore)

    def test_environment_no_pre_copy_commit_and_no_pointer_metadata_overwrite(self):
        result = adapter.adapt('blink/fpu.c', self.original('blink/fpu.c')).decode()
        save = adapter.function(result, 'OpFsave'); env = adapter.function(result, 'OpFstenv')
        self.assertLess(save.index('CopyToUserWrite'), save.index('blink_host_guest_x87_state(m, 1, 20,'))
        self.assertLess(env.index('CopyToUserWrite'), env.index('blink_host_guest_x87_state(m, 1, 21,'))
        self.assertIn('Osz(rde) ? 14 : 28', save)
        dispatch = adapter.function(result, 'OpFpu')
        self.assertLess(dispatch.index('blink_host_guest_x87_reject_legacy'), dispatch.index('m->fpu.ip ='))
        self.assertEqual(result.count('int BlinkGuestX87StateProtocol(void) { return 1; }'), 1)

    def test_every_reviewed_legacy_mmx_helper_is_guarded_before_old_body(self):
        self.assertEqual(sum(map(len, adapter.LEGACY_GUARDS.values())), 75)
        for path, names in adapter.LEGACY_GUARDS.items():
            result = adapter.adapt(path, self.original(path)).decode()
            for name in names:
                body = adapter.function(result, name)
                self.assertIn('{\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  blink_host_guest_x87_reject_legacy(m, 1);\n#endif', body)
                original = adapter.function(self.original(path).decode(), name)
                guard = '\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  blink_host_guest_x87_reject_legacy(m, 1);\n#endif\n'
                self.assertEqual(body.replace(guard, '', 1), original)

    def test_unreviewed_state_consumer_cannot_be_silently_added(self):
        with self.assertRaises(ValueError): adapter.adapt('blink/new-consumer.c', b'void f(void) { m->fpu.st[0] = 1; }')
        with self.assertRaises(ValueError): adapter.adapt('blink/fpu.c', self.original('blink/fpu.c') + b'\nvoid extra(void) { m->fpu.st[0] = 1; }')
        self.assertEqual(adapter.adapt('blink/irrelevant.c', b'int x;'), b'int x;')

    def test_shared_inventory_rejects_equivalent_members_in_unknown_and_known_units(self):
        spellings = ['fpu.st', 'fpu . st', 'fpu/*gap*/.st',
                     'fpu/**/ . /*gap*/ st', 'fpu//gap\n.\tst',
                     'fpu\\\n. st', 'fp\\\nu . st', 'fpu -> st']
        for spelling in spellings:
            with self.subTest(spelling=spelling):
                extra = ('\nvoid extra(struct Machine *m) { m->' + spelling + '[0] = 1; }').encode()
                self.assertEqual(adapter.legacy_accesses(extra.decode()), {'fpu.st': 1})
                with self.assertRaisesRegex(ValueError, 'unreviewed legacy'):
                    adapter.adapt('blink/new-consumer.c', extra)
                with self.assertRaisesRegex(ValueError, 'access inventory'):
                    adapter.adapt('blink/fpu.c', self.original('blink/fpu.c') + extra)

    def test_shared_inventory_rejects_all_helper_call_spellings(self):
        for name in sorted(adapter.LEGACY_CALLS):
            for gap in ['', ' ', '\t\n', '/*gap*/', '//gap\n', '\\\n/**/ ']:
                with self.subTest(name=name, gap=gap):
                    extra = ('\nvoid extra(void) { ' + name + gap + '(m); }').encode()
                    self.assertEqual(adapter.legacy_accesses(extra.decode()), {name + '(': 1})
                    with self.assertRaisesRegex(ValueError, 'unreviewed legacy'):
                        adapter.adapt('blink/new-consumer.c', extra)
                    with self.assertRaisesRegex(ValueError, 'access inventory'):
                        adapter.adapt('blink/fpu.c', self.original('blink/fpu.c') + extra)

    def test_comments_literals_and_token_boundaries_do_not_confuse_inventory(self):
        safe = r'''/* m->fpu.st[0]; MmReg(m); */
// XmmRexbRm(m);
const char *s = "/* not a comment */ fpu.st MmReg( \\";
char c = '\'';
void safe(void) { fpu_st = 0; notFpuSt(m); FpuSt_suffix(m); }
'''
        self.assertEqual(adapter.legacy_accesses(safe), {})
        self.assertEqual(adapter.adapt('blink/irrelevant.c', safe.encode()), safe.encode())
        # An apparent comment inside a string must not consume this real access.
        for extra in ['m->fpu/**/.st[0] = 1;', 'MmReg/**/(m);']:
            with self.assertRaises(ValueError):
                adapter.adapt('blink/new-consumer.c', (safe + extra).encode())
        self.assertEqual(adapter.legacy_accesses('fp/**/u.st; Fpu/**/St(m);'), {})
        xmm = b'void independent(void) { XmmRexrReg/**/(m, rde); XmmRexbRm (m, rde); }'
        self.assertEqual(adapter.adapt('blink/independent-xmm.c', xmm), xmm)
        self.assertEqual(adapter.legacy_accesses(xmm.decode()), {'XmmRexrReg(': 1, 'XmmRexbRm(': 1})

    def test_sigreturn_exact_layout_initialized_prefix_and_actual_pointer_only(self):
        source = self.original('blink/signal.c').decode()
        result = adapter.adapt('blink/signal.c', source.encode()).decode()
        restore = adapter.function(result, 'SigRestore')
        active = restore.split('  if (r133_active) {', 1)[1].split('  } else {', 1)[0]
        self.assertIn('memset(&sf, 0, sizeof(sf));', active)
        prefix = ('CopyFromUserRead(m, &sf, Read64(m->sp) - 8,\n'
                  '                         offsetof(struct SignalFrame, fp))')
        actual = 'CopyFromUserRead(m, &sf.fp, fp, sizeof(sf.fp))'
        self.assertIn(prefix + ' == -1)', active)
        self.assertNotIn('CopyFromUserRead(m, &sf, Read64(m->sp) - 8, sizeof(sf))', active)
        self.assertEqual(active.count('CopyFromUserRead('), 2)
        self.assertLess(active.index('memset(&sf'), active.index(prefix))
        self.assertLess(active.index(prefix), active.index('fp = Read64(sf.uc.fpstate);'))
        self.assertLess(active.index('if (fp) {'), active.index(actual))
        self.assertLess(active.index(actual), active.index('blink_host_guest_x87_state(m, 1, 2,'))
        self.assertIn('TerminateSignal(m, SIGSEGV_LINUX, m->segvcode);', active)
        self.assertIn('TerminateSignal(m, SIGSEGV_LINUX, SI_KERNEL_LINUX);', active)
        self.assertNotRegex(active, r'm->\w+\s*=|memcpy\(m->|ImportFlags\(')
        self.assertLess(restore.index('blink_host_guest_x87_state(m, 1, 2,'), restore.index('m->ip ='))
        self.assertLess(restore.index('blink_host_guest_x87_state(m, 1, 2,'), restore.index('blink_host_guest_x87_state(m, 1, 3,'))
        self.assertIn('} else {\n      blink_host_guest_x87_state(m, 1, 4, 0, 0);', restore)
        # Exact immutable target declarations; all ucontext members are byte arrays.
        linux = (ROOT / 'blink/ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/linux.h').read_text()
        uc = linux.split('struct ucontext_linux {', 1)[1].split('};', 1)[0]
        fields = re.findall(r'\bu8\s+(\w+)\[(\d+)\];', uc)
        self.assertEqual(re.sub(r'\bu8\s+\w+\[\d+\];|\s+', '', uc), '')
        offset = 0; offsets = {}
        for name, size in fields:
            offsets[name] = offset; offset += int(size)
        self.assertEqual(offset, 304)
        self.assertEqual(offsets['fpstate'], 224)
        self.assertEqual(offsets['sigmask'] + 8, 304)
        self.assertIn('struct SignalFrame {\n  u8 ret[8];\n  struct siginfo_linux si;\n  struct ucontext_linux uc;\n  struct fpstate_linux fp;\n};', source)
        for assertion in ['offsetof(struct SignalFrame, uc) == 136', 'sizeof(sf.uc) == 304',
                          'offsetof(struct SignalFrame, fp) == 440', 'sizeof(sf.fp) == 512',
                          'sizeof(sf) == 952']:
            self.assertIn('_Static_assert(' + assertion, active)

    def test_sigreturn_unreadable_inline_null_external_source_schedules(self):
        """Address/source fixtures only: no fake guest memory or callback execution."""
        restore = adapter.function(adapter.adapt('blink/signal.c', self.original('blink/signal.c')).decode(), 'SigRestore')
        active = restore.split('  if (r133_active) {', 1)[1].split('  } else {', 1)[0]
        self.assertIn('offsetof(struct SignalFrame, fp)) == -1)', active)
        self.assertIn('if (fp) {\n      if (CopyFromUserRead(m, &sf.fp, fp, sizeof(sf.fp)) == -1 ||', active)
        base = 0x1e48; prefix = 440; image = 512
        self.assertEqual(base + prefix, 0x2000)
        self.assertEqual(base + prefix + image, 0x2200)
        for pointer, actual_range in [(0, None), (0x3000, (0x3000, 0x3200)),
                                      (0x2000, (0x2000, 0x2200))]:
            with self.subTest(pointer=pointer):
                reads = [(base, base + prefix)]
                if pointer: reads.append((pointer, pointer + image))
                self.assertEqual(reads[1:] , [] if actual_range is None else [actual_range])
                if pointer != 0x2000:
                    self.assertTrue(all(end <= 0x2000 or start >= 0x2200 for start, end in reads))
                else:
                    self.assertEqual(reads[1], (0x2000, 0x2200))
        # Actual inline pointer remains checked; inaccessible actual image fails,
        # while null skips image read/validation and selects the reset branch.
        self.assertIn('blink_host_guest_x87_state(m, 1, 4, 0, 0);', restore)

    def test_inactive_restore_retains_exact_original_frame_read_and_fp_block(self):
        original = adapter.function(self.original('blink/signal.c').decode(), 'SigRestore')
        restore = adapter.function(adapter.adapt('blink/signal.c', self.original('blink/signal.c')).decode(), 'SigRestore')
        anchor = '  unassert(!CopyFromUserRead(m, &sf, Read64(m->sp) - 8, sizeof(sf)));'
        self.assertIn('  } else {\n' + anchor + '\n  }\n#else\n' + anchor + '\n#endif', restore)
        a = original.index('  m->mxcsr ='); b = original.index('  memcpy(m->xmm, sf.fp.xmm, sizeof(sf.fp.xmm));')
        block = original[a:b + len('  memcpy(m->xmm, sf.fp.xmm, sizeof(sf.fp.xmm));')]
        self.assertIn('  } else {\n' + block + '\n  }\n#else\n' + block + '\n#endif', restore)

    def test_current_root_producer_seams_compose_without_execution(self):
        tree = ast.parse((ROOT / 'blink/scripts/stage-threaded-core.py').read_text())
        calls = [(node.lineno, node.func.value.id) for node in ast.walk(tree)
                 if isinstance(node, ast.Call) and isinstance(node.func, ast.Attribute)
                 and node.func.attr == 'adapt' and isinstance(node.func.value, ast.Name)]
        names = [name for _, name in sorted(calls)]
        at = names.index('full_jit_x87_state_bindings')
        self.assertEqual(names[at - 1:at + 2], ['full_jit_copy_bindings', 'full_jit_x87_state_bindings', 'full_jit_bindings'])
        self.assertEqual(adapter.INPUTS, ('scripts/full_jit_x87_state_bindings.py',))
        host = (ROOT / 'blink/scripts/stage-host-bindings.py').read_text()
        self.assertIn('*full_jit_x87_state_bindings.INPUTS', host)
        core = (ROOT / 'blink/scripts/core_inputs.py').read_text()
        self.assertIn('from full_jit_x87_state_bindings import BOUNDARY_SIGNATURES', core)
        self.assertIn('*BOUNDARY_SIGNATURES,', core)
        self.assertIn("spec['required_units']['blink/fpu.c'] = [name for name, _, _ in BOUNDARY_SIGNATURES]", core)


if __name__ == '__main__': unittest.main()

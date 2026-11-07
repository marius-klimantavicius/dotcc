#!/usr/bin/env python3
"""Fail-closed producer checks; no managed build or generated C# mutation."""
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

import full_jit_bindings as full
import cpu_bindings

SCRATCH = Path(os.environ.get('BLINK_TEST_SCRATCH', '/mnt/storage/blink-full-jit-resume/bindings'))
SCRATCH.mkdir(parents=True, exist_ok=True)
os.environ['TMPDIR'] = str(SCRATCH / 'tmp')
Path(os.environ['TMPDIR']).mkdir(exist_ok=True)
tempfile.tempdir = os.environ['TMPDIR']
UPSTREAM = full.ROOT / 'ref' / json.loads((full.ROOT / 'config/source-manifest.json').read_text())['upstream']['directory']


class FullJitBindings(unittest.TestCase):
    def test_dense_and_sparse_exact_coverage(self):
        source = full.upstream_machine()
        dense, sparse = full.dispatch(source)
        self.assertEqual(len(dense), 524)
        self.assertEqual(len(sparse), 13)
        self.assertEqual(dense[0x105], 'OpSyscall')
        self.assertEqual(sparse[0x2f0], 'Op2f01')
        for mutation in (
            source.replace('return kNexgen32e[op];', 'return OpUd;', 1),
            source.replace('XLAT(0x21c, OpSsePabsb);', 'XLAT(0x20b, OpSsePabsb);', 1),
            source.replace('/*000*/ OpAlub,', '/*000*/ UnknownHandler,', 1),
        ):
            with self.assertRaises(ValueError):
                full.dispatch(mutation)

    def test_feature_selection_and_callback_guards(self):
        source = cpu_bindings.adapt('blink/machine.c', full.upstream_machine().encode())
        producer = full.Producer('blink/machine.c', source.decode())
        self.assertEqual(producer.aliases['OpMovEvqpSw'], 'OpReadUserSegment')
        self.assertEqual(producer.aliases['OpCallf'], 'OpUd')
        self.assertNotIn('OpHvcall', producer.functions)
        out = full.adapt('blink/machine.c', source)
        self.assertEqual(out.count(b'blink_host_guest_interpreter_dispatch('), 3)
        self.assertEqual(out.count(b'#ifdef BLINK_MANAGED_GUEST_THREADS'), 3)
        self.assertIn(b'default: return BlinkFullJitChoose_OpUd(', out)
        with self.assertRaises(ValueError):
            full.adapt('blink/machine.c', out)

    def test_constant_control_flow_and_fallthrough(self):
        source = '''int value; switch (3) {
          case 0: value = 5; break;
          case 2: case 3: value = 7; break;
          default: value = 9;
        } if (0) { value = 11; } else if (1) { value += 1; }
        if (runtime) { value += 2; }'''
        out = full.simplify(source, {})
        self.assertNotIn('switch', out)
        self.assertNotIn('value = 5', out)
        self.assertNotIn('value = 11', out)
        self.assertIn('value = 7', out)
        self.assertIn('if (runtime)', out)
        loop = full.simplify('int i; for (i = 0; i < 4; ++i) { if (i == 0) mark(); }', {})
        self.assertIn('if (i == 0)', loop)
        guarded = full.simplify('int x; if (runtime) { x = 1; } if (x == 1) mark();', {})
        self.assertIn('if (x == 1)', guarded)
        self.assertIsNone(full.number('__import__("os").system("false")'))

    def test_prefix_and_subgroup_semantics_native_fixture(self):
        fixture = '''
#define P struct Machine *m, unsigned long rde, long disp, unsigned long uimm0
#define A m,rde,disp,uimm0
struct Machine { int result; };
typedef void (*nexgen32e_f)(P);
typedef unsigned long u64;
typedef long i64;
#define Rep(x) (((x)>>8)&3)
#define Osz(x) (((x)>>10)&1)
#define ModrmReg(x) ((x)&7)
#define IsModrmRegister(x) (((x)>>11)&1)
#define Mopcode(x) (((x)>>16)&2047)
static void OpUd(P) { m->result = -1; }
static void OpAdd(P) { m->result = 100 + disp; }
static void OpSub(P) { m->result = 200 + uimm0; }
static const nexgen32e_f kOp0f6[] = {OpUd,OpUd,OpAdd,OpSub,OpUd,OpUd,OpUd,OpUd};
static void Op0f6(P) { kOp0f6[ModrmReg(rde)](A); }
static void OpShuffle(P) {
  switch (Rep(rde) | Osz(rde)) {
    case 0: OpAdd(A); break;
    case 1: OpSub(A); break;
    default: OpUd(A);
  }
}
#define STRING_MOVS 1
static void StringOp(P, int op) {
  switch (op) { case 1: OpAdd(A); break; default: OpSub(A); }
}
static void OpMovs(P) { StringOp(A, STRING_MOVS); }
'''
        with patch.object(full, 'primary_opcodes', return_value={'Op0f6': [0xf6], 'OpShuffle': [0x170], 'OpMovs': [0xa5]}):
            producer = full.Producer('blink/fixture.c', fixture)
            generated, inventory = producer.emit()
        self.assertEqual(producer.fields('Op0f6'), ('ModrmReg',))
        self.assertEqual(producer.fields('OpShuffle'), ('Rep', 'Osz'))
        for body in producer.generated.values():
            self.assertNotIn('kOp0f6[', body)
            self.assertNotRegex(body, r'\b(?:Rep|Osz|ModrmReg)\(rde\)')
            self.assertNotIn('switch (op)', body)
        harness = '''
int main(void) {
  struct Machine m = {0};
  for (unsigned rep = 0; rep < 4; ++rep)
    for (unsigned osz = 0; osz < 2; ++osz)
      for (unsigned reg = 0; reg < 8; ++reg) {
        u64 rde = (rep<<8) | (osz<<10) | reg;
        OpShuffle(&m,rde,3,4); int expected = m.result;
        BlinkFullJitChoose_OpShuffle(rde,3,4)(&m,rde,3,4);
        if (m.result != expected) return 1;
        Op0f6(&m,rde,3,4); expected = m.result;
        BlinkFullJitChoose_Op0f6(rde,3,4)(&m,rde,3,4);
        if (m.result != expected) return 2;
        OpMovs(&m,rde,3,4); expected = m.result;
        BlinkFullJitChoose_OpMovs(rde,3,4)(&m,rde,3,4);
        if (m.result != expected) return 3;
      }
  return 0;
}
'''
        with tempfile.TemporaryDirectory(prefix='full-jit-unit-') as directory:
            path = Path(directory)
            c = path / 'fixture.c'
            c.write_text(fixture + generated + harness)
            command = ['cc', '-std=c11', '-Werror', '-DDISABLE_JIT', '-DDISABLE_METAL',
                       '-DDISABLE_BCD', '-DDISABLE_BMI2', str(c), '-o', str(path / 'test')]
            subprocess.run(command, check=True, capture_output=True, timeout=30)
            subprocess.run([str(path / 'test')], check=True, timeout=10)

    def test_interpreter_reentry_is_rejected(self):
        with patch.object(full, 'primary_opcodes', return_value={'OpBad': [1]}):
            producer = full.Producer('blink/fixture.c', 'void OpBad(P) { LoadInstruction(m, 0); }')
            with self.assertRaisesRegex(ValueError, 'interpreter'):
                producer.emit()

    def test_all_selected_closure_and_real_c_syntax(self):
        if not shutil.which('cc'):
            self.skipTest('C syntax validator unavailable')
        profile = full.ROOT / 'generated/threaded-core/20261026-194355-38901e55'
        closure = json.loads((profile / 'closure.json').read_text())['sources']
        additions = json.loads((profile / 'managed-additions.json').read_text())['sources']
        overrides = json.loads((profile / 'binding-overrides.json').read_text())
        sources = {}
        for row in closure + additions:
            name = row['path']
            location = full.ROOT / overrides[name]['staged_path'] if name in overrides else UPSTREAM / name
            # Host declarations are handled separately by the actual producer.
            source = location.read_bytes()
            prefix = b'#include "host-bindings.h"\n'
            if source.startswith(prefix):
                source = source[len(prefix):]
            sources[name] = source
        manifest = full.inventory(sources)
        self.assertEqual(len(manifest['dispatch']), 538)
        self.assertTrue(all(row['selected_handler'] for row in manifest['dispatch']))
        self.assertEqual(next(row for row in manifest['dispatch'] if row['opcode'] == 0x8c)['selected_handler'], 'OpReadUserSegment')
        with self.assertRaisesRegex(ValueError, 'Missing selected handler'):
            full.inventory({'blink/machine.c': sources['blink/machine.c']})
        with tempfile.TemporaryDirectory(prefix='full-jit-real-c-') as directory:
            temp = Path(directory)
            config = temp / 'config.h'
            config.write_text((profile / 'config.h').read_text() + '\n#include "host-guest-threads.h"\nint blink_host_memfd_create(const char *, unsigned);\nint BlinkHostMemoryFilePageShared(const void *);\n')
            shutil.copyfile(full.ROOT / 'config/managed-host/abi.h', temp / 'abi.h')
            checked = []
            for row in manifest['units']:
                name = row['path']
                adapted = full.adapt(name, sources[name])
                target = temp / Path(name).name
                target.write_bytes(adapted)
                command = ['cc', '-fsyntax-only', '-Werror=implicit-function-declaration',
                           '-D_GNU_SOURCE', '-include', str(config), '-I', str(temp), '-I', str(full.ROOT / 'config'),
                           '-I', str(full.ROOT / 'src/Host/include'),
                           '-I', str(UPSTREAM), str(target)]
                result = subprocess.run(command, capture_output=True, text=True, timeout=60)
                self.assertEqual(result.returncode, 0, name + '\n' + result.stderr[-6000:])
                checked.append(name)
            (SCRATCH / 'inventory.json').write_text(json.dumps(manifest, indent=2) + '\n')
            (SCRATCH / 'syntax-checked.json').write_text(json.dumps(checked, indent=2) + '\n')


if __name__ == '__main__':
    unittest.main(verbosity=2)

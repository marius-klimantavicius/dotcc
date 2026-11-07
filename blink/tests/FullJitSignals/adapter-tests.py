#!/usr/bin/env python3
"""Exact source pins, reject-on-drift guards, and signal adaptation boundaries."""
import hashlib
import importlib.util
import json
from pathlib import Path
import unittest

BLINK = Path(__file__).resolve().parents[2]
REVISION = json.loads((BLINK/'config/source-manifest.json').read_text())['upstream']['directory']
UPSTREAM = BLINK/'ref'/REVISION/'blink'
spec = importlib.util.spec_from_file_location('signal_bindings', BLINK/'scripts/signal_bindings.py')
bindings = importlib.util.module_from_spec(spec)
spec.loader.exec_module(bindings)


class SignalBindingsTests(unittest.TestCase):
    def test_immutable_inputs(self):
        for name, expected in {
            'throw.c': '5df4b02be0d089b25c6318841cb072c5af9ca1b3e5aa9e11982bdc73ca7e2eed',
            'signal.c': 'e4a1a44787a5e99022a5d8224165e0c551275b4706b9f689f4f826e4f0b95c5a',
        }.items():
            self.assertEqual(hashlib.sha256((UPSTREAM/name).read_bytes()).hexdigest(), expected)

    def test_ud_metadata_and_unchanged_hlt(self):
        original = (UPSTREAM/'throw.c').read_bytes()
        result = bindings.adapt('blink/throw.c', original).decode()
        hlt = result[result.index('    case kMachineHalt:'):result.index('    case kMachineDecodeError:')]
        self.assertEqual(hlt, '''    case kMachineHalt:
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGILL_LINUX, ILL_ILLOPC_LINUX);
      break;
''')
        ud = result[result.index('    case kMachineDecodeError:'):result.index('    case kMachineProtectionFault:')]
        self.assertIn('m->trapno = 6;', ud)
        self.assertIn('SIGILL_LINUX, ILL_ILLOPN_LINUX', ud)
        self.assertNotIn('ILL_ILLOPC_LINUX', ud)
        # The architectural vector must not replace the internal jump value.
        self.assertIn('siglongjmp(m->onhalt, code);', result)
        self.assertIn('if (code == kMachineSimdException) m->trapno = 19;', result)
        # Compose the actual reviewed scalar-FP block, without running a build.
        scalar = BLINK/'src/UpstreamScalarFp'
        rule = json.loads((scalar/'patch-inputs.json').read_text())['throw.c']
        start, end = original.index(rule['start'].encode()), original.index(rule['end'].encode())
        self.assertEqual(hashlib.sha256(original[start:end]).hexdigest(), rule['block_sha256'])
        replacement = (b'#ifndef DISABLE_JIT\n#error Reviewed scalar FP staging requires DISABLE_JIT\n#endif\n' +
                       (scalar/rule['replacement']).read_bytes() + b'\n')
        altered = original[:start] + replacement + original[end:]
        composed = bindings.adapt('blink/throw.c', altered)
        self.assertIn(b'ILL_ILLOPN_LINUX', composed)
        self.assertIn(b'case kMachineSimdException: {\n      m->trapno = 19;', composed)

    def test_context(self):
        original = (UPSTREAM/'signal.c').read_bytes()
        result = bindings.adapt('blink/signal.c', original)
        self.assertIn(b'GetFlag(m->flags, FLAGS_PF)', result)
        self.assertIn(b'ImportFlags(m, Read64(sf.uc.eflags))', result)
        self.assertIn(b'Write64(sf.uc.trapno, 6)', result)
        self.assertIn(b'Write32(sf.fp.mxcsr, m->mxcsr)', result)
        self.assertIn(b'm->trapno == 0 || m->trapno == 19', result)
        self.assertIn(b'm->mxcsr = Read32(sf.fp.mxcsr)', result)
        self.assertIn(b'Write16(sf.uc.cs, m->cs.sel)', result)
        self.assertIn(b'Write16(sf.uc.ss, m->ss.sel)', result)

    def test_threaded_siginfo_composes(self):
        original = (UPSTREAM/'signal.c').read_bytes()
        insertion = b'  blink_host_guest_signal_apply_info(m, sig, &sf.si);\n'
        threaded = original.replace(b'  Write64(sf.uc.sigmask, m->sigmask);',
                                    insertion + b'  Write64(sf.uc.sigmask, m->sigmask);')
        result = bindings.adapt('blink/signal.c', threaded)
        self.assertEqual(result.count(insertion), 1)
        self.assertIn(b'Write64(sf.uc.trapno, 6)', result)

    def test_drift_and_duplicate_rejected(self):
        for name, anchor in [('throw.c', b'    case kMachineUndefinedInstruction:'),
                             ('signal.c', b'  Write64(sf.uc.eflags, m->flags);'),
                             ('signal.c', b'  m->ip = Read64(sf.uc.rip);')]:
            original = (UPSTREAM/name).read_bytes()
            modified_inputs = [original.replace(anchor, anchor + b' /* drift */')]
            if b'm->ip = ' not in anchor:
                modified_inputs.append(original + b'\n' + anchor + b'\n')
            for modified in modified_inputs:
                with self.subTest(name=name, anchor=anchor, duplicate=modified.endswith(anchor+b'\n')):
                    with self.assertRaises(ValueError):
                        bindings.adapt('blink/'+name, modified)
        original = (UPSTREAM/'signal.c').read_bytes()
        with self.assertRaises(ValueError):
            bindings.adapt('blink/signal.c', bindings.adapt('blink/signal.c', original))

    def test_unowned_path_passthrough(self):
        self.assertEqual(bindings.adapt('blink/cpuid.c', b'unchanged'), b'unchanged')


if __name__ == '__main__':
    unittest.main()

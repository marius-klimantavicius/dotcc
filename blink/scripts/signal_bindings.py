"""Reviewed guest signal adaptations for fresh host-bound source copies.

Linux sigaltstack registration records a descriptor, including any guard page.
The existing signal delivery path checks the actual frame with CopyToUserWrite.

These corrections also apply to the interpreter: Linux x86 #UD uses
ILL_ILLOPN and vector 6, and signal frames contain architectural flags rather
than Blink's lazy-parity representation. HLT keeps its existing policy.
"""
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INPUTS = ('scripts/signal_bindings.py',)


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('Guest signal binding anchor differs: ' + before[:100])
    return source.replace(before, after, 1)


def checked_function(source, marker, expected, threaded_info=False):
    if source.count(marker) != 1:
        raise ValueError('Guest signal binding function boundary differs: ' + marker)
    start = source.index(marker)
    end = source.index('\n}', start) + 3
    body = source[start:end]
    if threaded_info:
        # UpstreamGuestThreads adds only this bridge within DeliverSignal.
        # Verify the same function body in both ordinary and threaded staging.
        insertion = '  blink_host_guest_signal_apply_info(m, sig, &sf.si);\n'
        if body.count(insertion) > 1:
            raise ValueError('Guest signal binding duplicate threaded siginfo bridge')
        body = body.replace(insertion, '')
    if hashlib.sha256(body.encode()).hexdigest() != expected:
        raise ValueError('Guest signal binding function fingerprint differs: ' + marker)


def adapt(path, content):
    if path == 'blink/throw.c':
        # The scalar-FP derivation changes other HaltMachine cases. This exact
        # shared case block is the independently pinned ownership boundary.
        before = '''    case kMachineHalt:
    case kMachineDecodeError:
    case kMachineUndefinedInstruction:
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGILL_LINUX, ILL_ILLOPC_LINUX);
      break;
'''
        after = '''    case kMachineHalt:
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGILL_LINUX, ILL_ILLOPC_LINUX);
      break;
    case kMachineDecodeError:
    case kMachineUndefinedInstruction:
      RestoreIp(m);
      m->faultaddr = m->ip;
      /* Linux x86 #UD: architectural vector, not Blink's halt code. */
      m->trapno = 6;
      DeliverSignalToUser(m, SIGILL_LINUX, ILL_ILLOPN_LINUX);
      break;
'''
        source = content.decode()
        for marker in ('    case kMachineHalt:', '    case kMachineDecodeError:',
                       '    case kMachineUndefinedInstruction:'):
            if source.count(marker) != 1:
                raise ValueError('Guest signal binding halt case boundary differs: ' + marker)
        source = replace_once(source, before, after)
        # Internal negative halt codes are not x86 exception vectors. Signal
        # delivery/context must expose architectural #DE and #PF to the guest.
        source = replace_once(source, '    case kMachineDivideError:\n      RestoreIp(m);',
                              '    case kMachineDivideError:\n      m->trapno = 0;\n      RestoreIp(m);')
        source = replace_once(source, '    case kMachineSegmentationFault:\n      RestoreIp(m);',
                              '    case kMachineSegmentationFault:\n      m->trapno = 14;\n      RestoreIp(m);')
        # Scalar-FP staging gives SIMD its own priority/status classifier.
        # Preserve that classifier and normalize only its architectural vector.
        simd = '    case kMachineSimdException: {\n'
        if source.count(simd) == 1:
            source = replace_once(source, simd, simd + '      m->trapno = 19;\n')
        else:
            # The original shared FPU/SIMD body has identical delivery policy;
            # only SIMD receives #XM, while x87 keeps its existing trap value.
            marker = '    case kMachineSimdException:\n      RestoreIp(m);'
            source = replace_once(source, marker,
                '    case kMachineSimdException:\n'
                '      if (code == kMachineSimdException) m->trapno = 19;\n'
                '      RestoreIp(m);')
        # x87 waiting instructions and MMX transitions report architectural
        # vector16. Derive Linux's FP priority from the unmasked x87 status,
        # retaining the faulting instruction address and the FP saved state.
        fpu = source.index('    case kMachineFpuException:\n')
        simd_start = source.index('    case kMachineSimdException:', fpu)
        old_fpu = source[fpu:simd_start]
        if old_fpu not in ('    case kMachineFpuException:\n', '''    case kMachineFpuException:
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGFPE_LINUX, FPE_FLTINV_LINUX);
      break;
'''):
            raise ValueError('Guest signal x87 halt boundary differs')
        source = source[:fpu] + '''    case kMachineFpuException: {
      m->trapno = 16;
      unsigned active = (m->fpu.sw & 0x3f) & ~m->fpu.cw;
      int signal_code = FPE_FLTINV_LINUX;
      if (active & 0x01) signal_code = FPE_FLTINV_LINUX;
      else if (active & 0x04) signal_code = FPE_FLTDIV_LINUX;
      else if (active & 0x08) signal_code = FPE_FLTOVF_LINUX;
      else if (active & 0x12) signal_code = FPE_FLTUND_LINUX;
      else if (active & 0x20) signal_code = FPE_FLTRES_LINUX;
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGFPE_LINUX, signal_code);
      break;
    }
''' + source[simd_start:]
        source = replace_once(source, '    case 1:\n    case 3:\n', '''    case 12:
      m->trapno = 12;
      RestoreIp(m);
      m->faultaddr = 0;
      DeliverSignalToUser(m, SIGBUS_LINUX, SI_KERNEL_LINUX);
      break;
    case 1:
    case 3:
''')
        source += '\nint BlinkGuestPreciseFaultProtocol(void) { return 1; }\n'
        return source.encode()
    if path == 'blink/signal.c':
        source = content.decode()
        checked_function(source, 'void DeliverSignal(struct Machine *m, int sig, int code) {',
            'd9dc0b29ee52d64a184cd41d972f8c90d8771f71ce45fc42cf88a49ef9b9ab25', True)
        checked_function(source, 'void SigRestore(struct Machine *m) {',
            '18cc06329e49366c45bb0d76d384ad17aad51e675ff50286a5882af1158e2517')
        source = replace_once(source, '#include "blink/endian.h"\n',
                              '#include "blink/endian.h"\n#include "blink/flags.h"\n')
        source = replace_once(source, '  Write64(sf.uc.eflags, m->flags);\n', '''  /* Export only architectural RFLAGS bits, including lazy parity. */
  Write64(sf.uc.eflags, (m->flags & 0x003f7fd1u) | 2u |
                           (u64)GetFlag(m->flags, FLAGS_PF) << FLAGS_PF);
  Write16(sf.uc.cs, m->cs.sel);
  Write16(sf.uc.ss, m->ss.sel);
  if ((sig == SIGFPE_LINUX && (m->trapno == 0 || m->trapno == 16 || m->trapno == 19)) ||
      (sig == SIGBUS_LINUX && code == SI_KERNEL_LINUX && m->trapno == 12) ||
      (sig == SIGSEGV_LINUX && code == SI_KERNEL_LINUX && m->trapno == 13) ||
      ((sig == SIGSEGV_LINUX || sig == SIGBUS_LINUX) && m->trapno == 14)) {
    Write64(sf.uc.trapno, m->trapno);
  }
  if (sig == SIGILL_LINUX && code == ILL_ILLOPN_LINUX && m->trapno == 6) {
    Write64(sf.uc.trapno, 6);
    /* #UD has no error code; sf was zero-initialized. */
  }
  Write32(sf.fp.mxcsr, m->mxcsr);
''')
        source = replace_once(source, '  m->flags = Read64(sf.uc.eflags);\n',
                              '  ImportFlags(m, Read64(sf.uc.eflags));\n')
        source = replace_once(source, '  m->fpu.cw = Read16(sf.fp.cwd);\n',
                              '  m->mxcsr = Read32(sf.fp.mxcsr);\n  m->fpu.cw = Read16(sf.fp.cwd);\n')
        source = replace_once(source, '  m->ip = Read64(m->system->hands[sig - 1].handler);\n',
            '  m->ip = Read64(m->system->hands[sig - 1].handler);\n'
            '  /* The saved frame owns the interrupted instruction. Fetching\n'
            '   * the handler must not rewind by an outer syscall length. */\n'
            '  m->oplen = 0;\n')
        return source.encode()
    if path != 'blink/syscall.c':
        return content
    source = content.decode()
    marker = 'static int SysSigaltstack(struct Machine *m, i64 newaddr, i64 oldaddr) {'
    if source.count(marker) != 1:
        raise ValueError('Guest signal binding SysSigaltstack boundary differs')
    start = source.index(marker)
    end = source.index('\n}', start) + 3
    original = source[start:end]
    probe = '''      if (!IsValidMemory(m, Read64(ss->sp), Read64(ss->size),
                         PROT_READ | PROT_WRITE)) {
        LOGF("sigaltstack ss_sp=%#" PRIx64 " ss_size=%#" PRIx64
             " didn't exist with read+write permission",
             Read64(ss->sp), Read64(ss->size));
        return -1;
      }
'''
    if original.count(probe) != 1:
        raise ValueError('Guest signal binding registration probe differs')
    replacement = original.replace(probe,
        '      // Linux registers the descriptor without probing stack pages.\n'
        '      // Delivery checks the actual frame with CopyToUserWrite.\n', 1)
    return (source[:start] + replacement + source[end:]).encode()

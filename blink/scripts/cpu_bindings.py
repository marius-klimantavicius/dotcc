"""Guest CPU identity for the portable single-CPU execution profile.

GetCpuCount's portable fallback advertises CPU 0, and GetTscAux reports CPU 0
and NUMA node 0. Report the same software CPU through Linux getcpu, copying
only the requested 32-bit outputs through the ordinary guest memory boundary.
"""

INPUTS = ('scripts/cpu_bindings.py',)


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('Guest CPU binding anchor differs: ' + before[:120])
    return source.replace(before, after, 1)


def adapt(path, content):
    if path == 'blink/machine.c':
        source = content.decode()
        source = replace_once(source, '#define OpMovEvqpSw   OpUd\n', '''/* Linux user mode exposes the visible selectors even without metal mode.
 * The ELF loader initializes CS/SS; reading FS/GS does not expose their bases.
 * A memory destination is always 16 bits, independent of operand-size prefixes.
 * Register destinations retain the ordinary 16/32/64-bit register semantics. */
static void OpReadUserSegment(P) {
  unsigned segment = ModrmReg(rde);
  if (m->metal || m->mode.omode != XED_MODE_LONG || segment >= 6) {
    OpUd(A);
    return;
  }
  if (IsModrmRegister(rde)) {
    WriteRegister(rde, RegRexbRm(m, rde), m->seg[segment].sel);
  } else {
    Store16(GetModrmRegisterWordPointerWrite2(A), m->seg[segment].sel);
  }
}
#define OpMovEvqpSw   OpReadUserSegment
''')
        return source.encode()
    if path != 'blink/syscall.c':
        return content
    source = content.decode()
    marker = 'static int ValidateAffinityPid(struct Machine *m, int pid) {'
    source = replace_once(source, marker, '''static int SysGetcpu(struct Machine *m, i64 cpuaddr, i64 nodeaddr,
                     i64 cacheaddr) {
  u8 value[4] = {0};
  (void)cacheaddr;  // Linux ignores the obsolete cache argument.
  // The portable guest CPU namespace and GetTscAux both identify CPU/node 0.
  if (cpuaddr && CopyToUserWrite(m, cpuaddr, value, sizeof(value)) == -1)
    return -1;
  if (nodeaddr && CopyToUserWrite(m, nodeaddr, value, sizeof(value)) == -1)
    return -1;
  return 0;
}

''' + marker)
    marker = '    SYSCALL(3, 0x13E, "getrandom", SysGetrandom, STRACE_GETRANDOM);'
    source = replace_once(source, marker,
        '    SYSCALL(3, 0x135, "getcpu", SysGetcpu, STRACE_3);\n' + marker)
    return source.encode()

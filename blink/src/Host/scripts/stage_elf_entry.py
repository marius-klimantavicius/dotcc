"""Reviewed correction of upstream LoadArgv's Linux ELF entry register bug.

The pinned source assigns its APE program-string convention to every executable.
glibc _start consumes Linux RDX as rtld_fini and later calls that string at exit.
See tests/ElfRuntime/README.md for the native-checked minimized reproducer and
the independent baseline investigation. Keep APE and flat-binary conventions;
LoadElf populates at_phnum, while LoadFlatExecutable does not.
"""
ANCHOR = b'  Write64(m->dx, dx);'
CORRECTION = b'  Write64(m->dx, m->system->iscosmo || !elf->at_phnum ? dx : 0);'


def adapt(content):
    if content.count(ANCHOR) != 1:
        raise RuntimeError('argv entry register anchor differs')
    return content.replace(ANCHOR, CORRECTION)

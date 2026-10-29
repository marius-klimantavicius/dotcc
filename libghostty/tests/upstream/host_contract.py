"""Check genuine emitted test declarations against the authored host-service ABI."""
import os
import shlex
import shutil


def check(ctx, source, includes, label):
    cc = shlex.split(os.environ.get("CC", "cc"))
    if not cc or not shutil.which(cc[0]):
        raise RuntimeError("Upstream diagnostic host-contract checks require a GNU-compatible C compiler")
    header = ctx.root / "tests/upstream/host.h"
    ctx.run([*cc, "-std=gnu11", "-w", "-I", includes[0], "-include", header,
             "-fsyntax-only", source], label, timeout=1800)

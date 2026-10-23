"""Acceptance entry points. Missing implementations remain explicit failures."""
import sys


def run(ctx, suite):
    if suite.name == "native":
        ctx.run([sys.executable, ctx.root / "scripts/native-control.py", "--fetch", "never"], "native", timeout=7200)
        return
    raise RuntimeError("Required s2n-tls suite has not passed implementation: " + suite.name)

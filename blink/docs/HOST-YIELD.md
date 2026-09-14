# Worker scheduling hint

The retained sched_yield dependency calls BCL Thread.Yield on the dedicated
worker and returns success whether another eligible thread actually ran. That
matches a scheduling hint's contract; it promises no fairness, priority change,
real-time scheduling, or guest thread implementation. A bound HostEnvironment is
required, with ENODEV otherwise. Success preserves the translated errno.

Actual throw.c retains this call in OpHlt's privileged interrupt-enabled path.
HAVE_SCHED_YIELD remains absent, so this binding does not alter upstream's
separate SysSchedYield preprocessor selection or advertise more guest capability.

Native and raw/optimized JIT/AOT pass at
artifacts/host-yield/attempt-rybwxwrz/receipt.json, including C function pointers,
repeated calls and two worker errno values across compacting GC. Tests establish
the callback contract, not that an operating-system scheduler must switch to
another particular thread. Run python3 blink/tests/HostYield/run.py.

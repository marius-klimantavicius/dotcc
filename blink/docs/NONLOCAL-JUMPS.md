# Unmanaged nonlocal jump storage

The reduced `tests/JumpStorage/repro.c` allocates
`struct State { jmp_buf halt; int reached; }`, establishes `setjmp(state->halt)`
in a valid C controlling expression, and calls `longjmp` through three helpers.
Native execution prints `reached=42` and exits zero. With the previous dotcc
header, generated C# stored `new LongJmpToken()` into the allocated struct and
built with six CS8500 managed-pointer warnings. This is a real storage defect:
the GC cannot trace or update a CLR object reference stored in native malloc
memory. A successful short execution would not qualify it.

## Ordinary C jump buffer design

The supplied generic header now declares `jmp_buf` as `uint64_t[1]`. Its C
storage contains only a numeric identity and has the normal array parameter
decay behavior of a C jump buffer. Its size is the dotcc runtime ABI, independent
of the native host's register-save record.

At each newly executed setjmp site, the backend evaluates the buffer expression
once and calls `ArmJumpBuffer`. That helper allocates a fresh nonzero identity
from a process-local atomic counter, writes it into the slot, and returns the
same value for the emitted handler to capture. Exhaustion fails explicitly
instead of recycling an identity. The handler compares the captured value with
the identity carried by `JumpBufferException`; it does not reevaluate the C
buffer expression or reread an overwritten slot during filtering.

`longjmp` reads the slot, normalizes zero to one, and throws the numeric identity
and value. It carries no reference to the C buffer. Nested different buffers
therefore target different handlers; rearming a buffer produces a new identity.
Value-capture lowering preserves its existing goto-based second-return behavior.
Legacy managed `LongJmpToken` APIs remain available to direct callers, but the
C header no longer uses that class.

No persistent identity dictionary, GC handle, or CLR reference lives in C
storage. The exception itself is an ordinary managed object during synchronous
unwinding. Jumping to an inactive invocation, using a freed/uninitialized buffer,
or jumping across threads remains outside the defined C contract. A direct
unlowered numeric-slot `setjmp` fails; it cannot pretend to establish a resumable
handler by returning zero.

The generic regression fixture `setjmp-heap-state` uses allocated storage,
nested distinct buffers, a side-effecting buffer selector, zero-to-one return
normalization, repeated rearming, and nested use of the same buffer. The native
expectation is `visits=1 hits=10`. Eighteen focused unit tests passed, and all
seven ordinary setjmp functional fixtures passed. Fourteen external GCC/WSL/MSVC
oracle rows were skipped because those configured external oracles were
unavailable; the new native fixture was also compiled and executed directly with
local GCC.

`python3 blink/tests/JumpStorage/run.py` additionally builds an owning test
consumer that includes unchanged generated C# plus the authored `GcHooks.cs`.
Optional fixture instrumentation forces full compacting collections immediately
before every deep `longjmp`, while its buffer lives in unmanaged memory. Raw
and semantically postprocessed variants matched native output under both JIT and
NativeAOT, with CS8500 promoted to an error. The postprocessor changed the
generated code; the raw snapshot stayed unchanged. The observed four-variant
receipt is `artifacts/jump-storage/attempt-zb5vu_3f/receipt.json`;
subsequent runs preserve separate attempt directories. No generated C# was
edited to add the hook. Before-fix diagnostics remain separately preserved under
`artifacts/jump-storage/`.

## Assignment guards and evaluation order

Actual unchanged Blink code captures its jump result inside
`if (!(rc = sigsetjmp(m->onhalt, 1)))`, including a repeated execution loop and
an else-if branch inspecting the nonzero value. The generic compiler now
recognizes negated and positive simple-variable assignment guards. It reuses
value-capture restart lowering, retaining the handler through the rest of the
enclosing block. The new fixture covers loop continue/break, else-if dispatch,
jumps initiated after the if statement, and repeated jumps to the same site.
Side-effecting assignment lvalues remain explicitly rejected.

Existing-target assignment captures now evaluate and arm the buffer before
writing the direct-return zero into the target. Native-checked selectors read
the old target value 42 in both a condition capture and a standalone assignment;
each selector runs once. The setjmp intrinsic also preserves C's implicit
`void *` conversion to the numeric jump-slot pointer.

`python3 blink/tests/JumpStorage/run.py --assignment-guards` runs this fixture
through the same forced-GC raw/optimized JIT/NativeAOT comparison as ordinary
heap-held buffers. Its generated code and authored GC hooks remain separate.
All four variants passed in `artifacts/jump-storage/attempt-rdxy9ji1/receipt.json`;
19 focused units and all eight setjmp functional fixtures passed (16 configured
external-oracle rows skipped).

## Bare negated guards

Unchanged Blink debug.c also uses `if (!setjmp(g_busted))`. The compiler now
recognizes this direct negation with a synthetic numeric result and the same
restart handler. Its scope includes the enclosing block tail, and repeated
jumps from recovery execute the recovery branch again. Existing equality and
positive guard lowering is unchanged.

The native-checked `setjmp-negated-guard` fixture covers zero normalization,
nested different buffers, repeated jumps from recovery, late jumps from the
block tail, an absent else branch, and a buffer selector evaluated once.
`python3 blink/tests/JumpStorage/run.py --negated-guards` passed under forced-GC
raw/optimized JIT and NativeAOT with CS8500 forbidden; receipt
`artifacts/jump-storage/attempt-b0kr_o_b/receipt.json`. Twenty focused units and
all nine setjmp functional fixtures passed; eighteen configured external-oracle
rows were skipped. This compiler support does not implement debug.c's remaining
host signal-handler calls.

## Qualified virtual host-delivery-mask jumps

`src/Host/HostSignals.c` implements an explicit signal-aware unwind adapter. The
campaign's ordinary jump prefix reserves 200 opaque bytes; managed execution
uses only its first numeric identity word. The signal record adds a 32-bit
saved-mask flag, alignment padding, and a separately owned 128-byte mask, for
336 bytes with 8-byte alignment. Its native staged oracle uses a real native
`jmp_buf` prefix plus explicit flag and mask members; no native padding is
repurposed. The untouched POSIX oracle retains its original native sigjmp_buf.

`sigsetjmp(env, save)` expands to ordinary setjmp around
`PrepareVirtualSignalJump(env, save)`. The helper evaluates its arguments once,
saves the worker's current virtual host-delivery mask only when requested,
clears the saved flag otherwise, and returns the ordinary identity slot.
`blink_host_siglongjmp` restores the separate saved mask before initiating the
ordinary numeric nonlocal unwind. Each worker has thread-local virtual state;
`BlinkHostDeliveryMaskReset` provides explicit lifecycle reset for one active
emulation context per worker. A later same-thread interleaving policy must save
and restore that state explicitly.

This mask models host-side virtual delivery and is distinct from Blink's guest
Linux blocked-signal state. The adapter neither installs real host signal
handlers nor changes the process/thread OS signal mask. It does not yet provide
signal delivery, `sigaction`, `sigprocmask`, `pthread_sigmask`, cancellation,
fault translation, or complete instance cleanup integration.

```sh
python3 blink/tests/HostSignals/run.py
python3 blink/tests/HostAbi/run-managed.py
```

The semantic runner compares a separate real POSIX sigsetjmp/siglongjmp oracle,
a native build of the authored virtual adapter with real ordinary jump storage,
and raw/optimized emitted C# under JIT and NativeAOT. All agree for save=0,
save=1, zero-to-one normalization, nested buffers, rearming a record without
saving, repeated jumps to the same active site, a side-effecting buffer
selector evaluated once, and the exact negated assignment guard used by Blink. Native contract checks query the real host mask;
managed test-only hooks observe the executing thread's `/proc/thread-self/status`
mask before each deep jump and at completion. Those masks remain unchanged.
Managed hooks also force compacting garbage collection before jumps, and all
consumer builds treat CS8500 as an error. Hooks are supplied by an authored test
consumer without hand-editing generated C#.

The four-mode semantic receipt is
`artifacts/host-signals/attempt-_r4ncjta/receipt.json`; the updated standalone
99-case native/198-output emitted storage matrix is
`artifacts/host-abi/managed/attempt-54lu91nd/receipt.json`. The signal record's
larger size deliberately changes containing Machine layouts; untouched native
Machine offsets cannot stand in for a matching staged-profile comparison.

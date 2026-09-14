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

## Signal-aware jumps remain a separate contract

The authored Blink jump record remains the already measured 200-byte storage:
eight 64-bit words, a saved-mask flag, alignment padding, and a 128-byte signal
mask. Ordinary `jmp_buf` becoming an eight-byte identity does not justify
replacing that record or aliasing `sigsetjmp` to `setjmp`.

A proposed managed signal-jump adapter can use one reserved 64-bit word as its
ordinary unwind identity slot, but must implement and qualify these additional
steps explicitly:

1. Evaluate the jump-buffer pointer and save-mask argument once.
2. If mask saving is requested, copy the instance's current virtual signal mask
   into the record and mark it saved; otherwise clear the flag.
3. Arm the ordinary numeric slot and establish the synchronous handler.
4. Before a signal-aware long jump, restore the saved virtual mask only when
   the record says it was captured, then perform the ordinary nonlocal unwind.

The virtual mask belongs to the emulation context's host-side delivery contract;
it must remain distinct from Blink's guest Linux signal-blocking state. Upstream
uses host `pthread_sigmask` around operations such as exec and separately tracks
the guest mask. Conflating those states would change guest behavior. No host OS
signal mask or process-wide handler may be changed by this managed implementation. A
separate native oracle can compare the required save-mask/no-save behavior in
its own process. Tests must cover nested masks, multiple jumps, buffer rearming,
cleanup, cancellation/fault interaction, and unchanged mask behavior when the
save flag is zero. Signal delivery and the mask operation implementations remain
separate host work; this design is not their implementation or qualification.

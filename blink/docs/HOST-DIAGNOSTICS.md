# Owned diagnostic reads

Unchanged debug.c `ReadWordSafely` installs SIGBUS/SIGSEGV handlers, unblocks
those signals, dereferences a host pointer, and recovers with longjmp. It
ignores failures from the host signal operations. Returning ENOSYS from those
operations would therefore leave the subsequent pointer access unsafe.

The exact current callers are debug.c `GetBacktrace` and the deferred TUI's
blinkenlights.c `DrawFrames`. Both ask upstream `SpyAddress` to translate a guest
frame pointer, then read words at the returned pointer and eight bytes later.
They inspect diagnostic frame data; this boundary does not replace instruction
execution, guest memory translation, or the guest page-table algorithms.

## Staged boundary

`src/HostMemory/stage-debug.py` verifies the entire immutable debug.c hash
`09ffcc0e51c93b3f9382cc5e5c384b4cabe3144694a6e70cb1ba037e12665c3b` before replacing
only `ReadWordSafely` and adding the HostMemory header. Its receipt records the
original function hash, exact replacement text and staged file hash. The
existing `ReadWord` implementation and upstream load routines remain unchanged.

For real/legacy/long modes, the replacement asks `BlinkHostMemoryContains` for
2/4/8 readable bytes. A nonempty range wholly inside one live mapping owned by
the current worker uses the existing reader. A foreign, freed, overflowing or
out-of-range pointer returns the same mode-specific FAKE_WORD sentinel without
being dereferenced. Invalid diagnostic modes return the full sentinel. No host
signal handlers or masks are installed or changed.

The predicate walks unmanaged ownership records and compares integer addresses
without computing an overflowing end pointer or retaining a pointer into TLS
storage. It preserves errno. Zero-length and null queries return false; ranges
crossing a mapping boundary return false even if adjacent storage happens to
be physically readable. The registry includes rounded mapping payloads.
Generic malloc-owned memory outside this registry is conservatively unavailable
for diagnostics; the predicate does not claim general host address validity.
The existing worker-lifetime and slab-cache restrictions in HOST-MEMORY.md apply.

## Native and emitted evidence

`python3 blink/tests/HostMemory/run-diagnostic.py` builds the entire untouched
native debug.c and bus.c in one subprocess, and the full staged debug.c with the
real memory adapter in another. The native untouched probe puts a guard page
beyond the readable page, so its original signal recovery establishes the
expected sentinel for crossing and invalid accesses. Staged reads obtain the
same results through ownership checks. The staged executable has no imported
sigaction, signal-mask, mmap or mprotect functions.

The focused managed test extracts byte-for-byte upstream ReadWord and load/store
function bodies, with hashes, and the staged diagnostic function. This explicitly
bounded slice avoids claiming the still-unfinished complete managed core link.
It runs raw/optimized JIT and NativeAOT, checks aligned/unaligned and end-of-page
reads, all three modes, zero/overflow/cross-end/foreign/freed range queries, and
an active second owner rejecting the first owner's pointer. An authored consumer
checks the real executing thread's signal mask, forces compacting GC with live
memory, and promotes CS8500 to an error. No generated C# is hand-edited.

## Target storage defect exposed by the oracle

Before `config/target-storage.h`, both `__BYTE_ORDER__` and
`__ORDER_BIG_ENDIAN__` were undefined in the honest managed profile. Upstream
endian.h compared them as `0 == 0` and selected byte swapping. Actual aligned
16/32-bit reads were therefore wrong while the bytewise unaligned and 64-bit
paths still looked correct. The failed native comparison is preserved in
`artifacts/host-diagnostic/attempt-f2howa11/raw-jit.diff`.

The campaign now declares measured little-endian storage compatibility constants
and rejects declared big-endian targets. This describes byte representation,
not a native CPU, operating system or GCC implementation. Core and decoder
profiles include and snapshot the same target-storage header. A direct uint32
byte-address check and a managed BitConverter.IsLittleEndian guard qualify that
assumption. Actual upstream Store16/32/64 operations are checked against literal
bytes independently of the separately initialized read patterns, avoiding a
read/write round trip that could conceal two compensating defects.

The final four-mode diagnostic/read/store receipt is
`artifacts/host-diagnostic/attempt-ec7j9i_d/receipt.json`. Fresh memory/native-core
qualification with the same target-storage profile passed at
`artifacts/host-memory/attempt-40vohryz/receipt.json`; the decoder's fresh native,
raw/optimized JIT and NativeAOT comparison passed in
`artifacts/decoder/receipt.json`, which records the target-storage hash.

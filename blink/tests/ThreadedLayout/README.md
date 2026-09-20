# Threaded internal ABI layout gate

Native plus raw/optimized JIT/NativeAOT passed in
`artifacts/threaded-layout/attempt-8zznigqf/receipt.json` (SHA256
`fcc4d2f11ce27d6535ee500cb7db356f6c72c61dc7452cc5c6a2f687b25c1765`).
All five executions produced the same 169 named rows. The managed consumers also
passed field/address and bidirectional sentinel checks before and after GC.
This qualifies the measured internal layout, not guest-thread execution.

The initial layout design was inspected against
`generated/threaded-core/attempt-first`; its incomplete compiler attempt is not
qualified producer evidence. The runner requires explicit `--profile` and
`--assembly-receipt` arguments identifying a completed, matching 108-object
assembly with the current compiler. It measures that profile, not native glibc
pthread layouts. The native oracle compiles the actual
pinned Blink headers with the selected managed storage policy: 64-bit signed
thread IDs and four-byte mutex/condition handles. Native compiler atomics are
used for native C; translated code uses dotcc's atomic lowering. The native
oracle does not invoke native pthread functions on managed handle storage.

The probe emits 169 named size/alignment/offset/width/stride measurements.
Separate include-order TUs check signal-before-pthread and pthread-before-signal,
including signed identity and an actual mutex-attribute-to-int pointer function.
All original 108 producer objects remain hash-verified in the diagnostic link;
the three added TUs are test-only and contain no C execution frontend.

The C# consumer checks generated record sizes and actual field addresses against
translated C measurements, whose complete output is compared with the native
oracle. Embedded wrapper field offsets check type alignment policy; scratch
allocations independently request and verify 16-byte alignment. This does not
claim default CLR allocation alignment. Fixed register/bus array extents follow
the pinned header constants, with element stride and surrounding record layout
checked explicitly.

Bidirectional sentinel access covers the owner-facing Machine/System fields,
register bytes, TLS base, jump storage, signal mask, seven synchronization slots,
and first/last bus/futex elements. These are scratch record values, never live
mutex handles or initialized Machines. No lifecycle, syscall or guest
instruction runs. The register pointer accessor performs only its ordinary
register-address selection. The C# field spelling `blink_host_system` reflects
the actual host-preamble macro expansion of upstream `system`.

The completed gate used native plus raw/optimized JIT/NativeAOT. The runner uses
exact canonical header paths for added managed objects so anonymous record
identities match the retained core objects. It snapshots the frozen Host/bridge
consumer closure privately, preserves raw generated files and records all
source/compiler/object/binary/log identities. It never rewrites generated C#,
changes the active product, or calls either C interpreter owner loop.

The passing profile is `generated/threaded-core/attempt-thread-accessor`, with
canonical assembly
`artifacts/core/objects/af192424572f668406ed04c563f78711e53657bb28d57a591db684bd41fa11b0/receipt.json`.
The final runner SHA256 is
`9abfb3c439faff0055ca5f55e93d29385f4cf2d48d561edc58fd7a8f2fea6b9d`.
Verification rechecked 768 frozen inputs, 147 private snapshot files, all 24
commands/log hashes, and execution binary identities before and after each run.
The optimized pass restores the exact original authored Host, bridges and
fixture after postprocessing; generated code is optimized separately.

`native-intrinsics.h` supplies only declaration support that the managed compiler
normally provides intrinsically: GCC varargs/offsetof builtins and the exact
Libc division, FILE and timespec declarations. Their runtime source files are
pinned in the receipt. FILE is used only through opaque pointers in this header
closure; no native libc function is called through these managed declarations.
Measured upstream records and pthread/signal definitions remain unchanged.
Native layout TUs use `-fno-builtin`; the separately compiled native-main uses
ordinary system libc headers.

Three failed harness attempts remain preserved: `attempt-lh5n8jmm` stopped before
builds because the runner incorrectly required an absent repository global.json;
`attempt-tpr51eme` exposed missing native declarations for managed intrinsic
types; `attempt-u8auv9rf` passed native/raw JIT/raw AOT but stopped before optimized
postprocessing because its private Host reference assembly had not been built.
The final attempt records global.json presence/absence and checks it again,
uses the bounded native-only intrinsic prelude, and builds the private optimized
Host reference before postprocessing. None of those failures is reported as a
passing complete matrix.

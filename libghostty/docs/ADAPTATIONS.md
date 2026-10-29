# Zig C support-header policy

The engine is genuine output from the pinned Zig C backend, using the actual
upstream module graph and generated Unicode/build inputs. No generated C# is
edited, and the terminal engine has no handwritten substitute.

The selected C target is `x86_64-freestanding-none`. This retains the 64-bit C
terminal ABI and the selected formatter/render/grid algorithms, while upstream
feature selection removes OS services irrelevant to this product. The
freestanding upstream allocator deliberately fails by default. The authored,
translated `src/Boundary/ghostty_boundary.c` supplies an explicit allocator
through the existing C extension point; it delegates aligned allocation/free to
the shared runtime. Resize/remap callbacks report unsupported so upstream itself
performs allocation/copy/free. All formatter construction uses that allocator.
Native controls execute the same boundary and confirm matching snapshots,
dimensions, independent instances and cleanup against both original and
freestanding C engines.

`config/zig-c-overrides.json` contains five exact, signature-checked macro rules
for Zig's support header. Their selected C-route runtime acceptance passes in
`20261030-001153-ab4e8deb`; see [the evidence](C-ROUTE-ACCEPTANCE.md):

- `zig_expand_import_0` selects the header's own `zig_import_builtin` forwarding
  implementation. This expresses the same C symbol call as the assembler-label
  import using an ordinary typed declaration and inline forwarder. Required
  imported math routines must resolve to supported runtime operations; unused
  header helpers are pruned with the shared compiler flag. This is a host symbol
  adaptation, not terminal algorithm replacement.
- `zig_mangled` removes an assembler label from two unused Windows-specific
  intrinsic declarations in `zig.h`. The selected generated engine contains no
  use of this macro or those intrinsic names. A final dependency audit must
  retain that invariant and fail if an affected name becomes reachable. A
  required use would need real symbol alias support instead.

- `zig_unreachable` selects the compiler's existing `__builtin_unreachable()`
  lowering when the support header chooses its empty unknown-compiler fallback.
  The exact zero-argument/empty-body match is required. This preserves the real
  terminating trap for impossible control-flow paths, instead of losing it while
  lowering C into C#. No return value or successful execution is synthesized.

- `zig_cmpxchg_strong` and `zig_cmpxchg_weak` preserve Zig's independent
  success/failure ordering through the selected C11 support-header implementation.
  They strengthen success to sequential consistency while retaining failure
  order and evaluating the original success expression once. This makes the
  real Zig arena allocator's release/acquire combination compatible with the
  stricter C11 compare-exchange contract. Object/expected/desired/failure values
  still flow through the real atomic operation. Shared atomic validation stays
  intact. Native/original-policy and emitted controls pass, followed by the
  current four-row product qualification.

No alignment annotation is removed. Shared C frontend repairs pass actual
native/direct/object layout and unaligned-access regressions. Parse-only probes
remain distinct from execution evidence.

## Public C ABI versus generated Zig-private types

The C backend correctly emits private Zig struct names in the concrete function
definitions, while the public C headers expose opaque handles and separate ABI
struct names. Native C can link these ABI signatures. Directly concatenating
independently emitted C# calls from the header and private definitions does not
typecheck. A reduced feasibility probe retained in
`artifacts/abi-cross-unit.log` demonstrates this distinction; it is not a working
terminal or an accepted runtime result.

The selected boundary uses a tiny Zig address accessor generated from
`config/abi.json`. The private, staged copy of `src/lib_vt.zig` appends one import
for this accessor; every original upstream export and algorithm remains intact.
The original source tree is unchanged. The generated C includes the accessor
and its actual function addresses. The authored C adapter makes calls through
typed pointers obtained from this primitive `(int) -> void*` entrypoint. Their
types come from unevaluated `__typeof__` expressions over the real public header
declarations. No handwritten mirror engine/type or native terminal import is
needed, and no generated C# is edited.

Native tests use a test-only C implementation of the address accessor pointing
at the original native library. This verifies the public ABI independently of
the generated accessor. The original/native-C 16-case oracle and two boundary
cases pass with the accessor, including by-value formatter options, callbacks,
allocation, cell/style queries, modes, resize/reset and instance independence.
Their current managed runtime acceptance passes all four rows. Diagnostic C qualification
tests live in a separate `qualification` profile; test entrypoints and fixed
transcripts are absent from the scalar delivered library.

The source ABI profile also explicitly selects 16-byte x87 `long double` storage
with 16-byte source alignment. This preserves Zig's actual ABI assertions. It
supports raw storage and copying only; it does not implement x87 arithmetic.
Default compiler requests retain their established 8-byte representation.

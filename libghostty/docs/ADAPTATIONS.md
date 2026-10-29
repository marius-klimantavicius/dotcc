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

`config/zig-c-overrides.json` contains two exact, signature-checked macro rules
for Zig's support header. Both are currently translation feasibility policy;
final runtime qualification remains open:

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

No alignment annotation is removed. The observed underaligned scalar typedef
failure is being repaired in the shared C frontend, with actual layout and
unaligned-access regressions. No runtime gate is claimed by a parse-only probe.

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
Their managed runtime acceptance remains required. Diagnostic C qualification
tests live in a separate `qualification` profile; test entrypoints and fixed
transcripts are absent from the scalar delivered library.

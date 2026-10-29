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

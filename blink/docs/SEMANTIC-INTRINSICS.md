# Reviewed Blink semantic intrinsics

The selected optimization replaces only the pinned `blink/endian.h` ordinary
byte helpers: Get16/Get32/Get64 and Put16/Put32/Put64. Their unsigned C signatures
remain unchanged. The corresponding typed compiler targets use BCL
`BinaryPrimitives.ReadUInt16/32/64LittleEndian` and
`BinaryPrimitives.WriteUInt16/32/64LittleEndian`, with explicit ordinary byte
spans of the exact width. Guest address translation and page permissions still
happen in the upstream callers; these helpers never receive guest addresses
as a substitute for translated host pointers. Atomic and volatile accesses are
outside this replacement contract.

`config/semantic-intrinsics.json` pins the complete upstream header and the six
function signatures. Profile staging resolves the physical declaration path
against that pinned tree. A required global function match would incorrectly
reject producers that do not include endian.h, so the rules are optional per
translation unit. Every emitted object must instead retain an actual typed
selection report: all six functions or six explicit unmatched events. The
machine, syscall, loader and scalar/SIMD producers require all six. Assembly
revalidates these reports, including their hashes and physical declaration
identities, before linking. The final delivery carries the coverage summary.

Resolved function rules, specification, helper implementation and compiler
identity participate in the C emission cache identity. A new profile therefore
reemits affected objects; object-only linking is not used to apply an override.
Units without active macro definitions retry with only the absent macro rules
removed, retaining the typed function rules and their reports. Literal pooling
and proven inline deduplication remain enabled at link time.

Secondary candidates were inspected against the pinned source:

- `bitscan.h` selects GNU ctz/clz/popcount builtins only under `__GNUC__`.
  dotcc identifies itself with `__dotcc__`, and this campaign does not define
  `__GNUC__`, so its portable bsf/bsr/popcount implementations remain selected.
  They are useful future candidates, but are not part of this six-target change.
  In particular the actual bsf(0) and bsr(0) functions return zero; a direct
  unguarded TrailingZeroCount or LeadingZeroCount substitution would differ.
- The compiler already lowers GNU byte-swap builtins to
  `BinaryPrimitives.ReverseEndianness`; duplicating that lowering adds no value.
  No additional byte-swap rule is selected.

The pinned-header fixture passes 336 native rows and all four managed forms
(1,344 managed comparisons), including direct/pointer calls, unsigned truncation,
unaligned buffers, exact surrounding canaries, once-evaluated arguments and
reads after byte writes. Receipt `endian-intrinsics/attempt-u1ulwk20/receipt.json`
has SHA-256 `859cb515c67c0cceda30e1af49c78dc63d1d27da077d34a66324db1c4cc0eb27`.
Six exact typed selections and all six emitted BinaryPrimitives operations are
verified. Generic tests separately pass 47 unit and 12 functional cases.

Complete product regeneration, its selected coverage and actual Kestrel/CPU
execution remain pending. These finite results establish no measured speedup
and do not start P6.

# Static-local identity across emitted objects

The first actual full C# core build failed with duplicate `once__s0`, `b__s0`,
and `buf__s0` members in `BlinkCoreGlobals`. The original receipt and diagnostics
are `artifacts/core-execution/attempt-68vow5of/`. These were valid independent
C objects with function/block scope and static storage duration.

The IR's existing local-static counter produces unique names while multiple
translation units share one direct compilation. Every separately emitted
object, however, starts that counter at zero. The object emitter qualified
internal function symbols with the input-path identity, but omitted hoisted
local-static variable symbols. Differing initializer/type lines produced
CS0102; identical emitted lines could instead deduplicate silently and merge
independent storage. Suppressing the duplicate-member diagnostic would therefore
be incorrect.

`IrBuilder.StaticLocals.cs` explicitly tracks each hoisted local-static Symbol
at registration. Scalar locals, aggregate-initialized locals, explicit arrays,
array typedefs, and character arrays all use the shared registration helper.
Before separate-object emission, the existing deterministic full-input-path
suffix is appended to those exact shared Symbols. Their declarations, aliases,
body references, address expressions and metadata observe the same final name.
Ordinary global declarations keep their linkage names. Direct compilation keeps
its existing monotonic counter and output spelling. No C algorithm or generated
C# text is patched.

The native-checked `static-local-multiple-units` fixture has three translation
units with colliding local names and different counters, identical zeroed
arrays, string-initialized arrays, struct storage, and array-typedef locals.
The pre-fix separate-object reproduction is retained at
`artifacts/static-local-multiple-units/repro-1zgbi2vf/`; native output passed and
managed compilation failed with the expected duplicate local-static members.

`StaticLocalObjectIdentityTests` also compiles same-basename files in different
directories and verifies distinct deterministic object identities without
renaming exported globals. The functional consumer checks independent storage
and retained pointers after compacting GC in direct/object-linked and flat/
nested forms. Focused tests passed 15 units and 7 functional tests; two
unavailable external-platform oracle rows were skipped. Logs are
`artifacts/static-local-multiple-units/focused-unit.log` and
`focused-functional.log`.

For the actual core retry, `affected-core-objects.json` records the 12 affected
objects out of the original 95. The derived profile
`generated/core-profile/static-local-refresh-a2sk1lkc` preserves every frozen
staged source/header hash and changes only the recorded compiler identity;
`derivation.json` retains its original-profile provenance. Existing assembly
tools re-emit those 12 objects with the repaired compiler. Combining them with
83 older unaffected objects is explicitly a diagnostic derived link, not a
claim that the entire canonical 95-object cache was rebuilt with one compiler.
The independent C tag/function namespace fix is applied by the new linker.

All 12 actual source refreshes passed. Their receipt is
`artifacts/core/objects/8f5ca2f01bb2d10f2674a342c5c853793e0546514a4ce87dea9bcbb2591e5916/receipt.json`.
The object hashes were checked, and their metadata contains 21 distinct
source-qualified static-local globals with no unqualified `__sN` declarations
remaining. `artifacts/static-local-multiple-units/fresh-core-objects.json`
records that receipt's hash and qualification counts. The combined derived
replay receipt is
`artifacts/core/objects/cffff5ed03345dfe797b829027bb5e00f5c6343aa47ecf9075ac32ca9b6176b0/receipt.json`;
consumer attempt `artifacts/core-execution/attempt-0imxwoxa` has zero CS0102
duplicate-member errors. It advances to 355 other semantic/missing-symbol
errors, so this fixes the observed naming blocker without claiming full managed
core compilation or execution.

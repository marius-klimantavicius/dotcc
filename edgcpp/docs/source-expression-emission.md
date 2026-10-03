# Source expressions and named bitfields

This implementation restores ordinary assignment, compound assignment, increment,
member and condition syntax at statement boundaries. It does not change EDG IL
or source language rules. `dot_gen_source_expression_api.h` separates generated
statements that must precede an expression from the expression itself. The
structured statement emitter must place those prefixes inside the appropriate
branch or loop iteration, including the increment edge reached by `continue`.

`plan_source_value` yields `{prefix, value}`. `plan_source_condition` yields a
C# Boolean condition without the previous integer truth roundtrip.
`plan_source_effect` yields complete statements and, only when there are no
prefix statements, a candidate `for` header expression. Plans use scoped
node-to-expression overrides with explicit storage-address provenance, never
mutate EDG nodes, and restore overrides
before returning. A named ref alias is never assumed to be directly addressable
with C# `&`; its original arena/frame/receiver pointer travels separately from
its readable lvalue. The ordinary `expr` and `location` entry points consult those
overrides before their existing ABI and storage adapters. `address(e)` remains
responsible for turning a source lvalue/ref alias into an actual pointer.

For example, an unused `flags->mode += 2` uses that property expression directly.
An assignment whose value is consumed must instead expose the value actually
stored in the bitfield. C# property assignment returns the setter input, so the
planner truncates/sign-extends a local stored-value temporary and returns it.
An effectful right operand is evaluated before resolving/reading the assignment
target, preserving C++17 sequencing even for `counter += change_counter()`.
Nontrivial receivers are captured once. Prefixes of conditional and short-circuit
operands remain inside their selected branches. Prior operand values are saved
before a later operand's prefix could change them.

The separate named-property emitter follows dotcc's `PackMappedBitFields`,
`BitFieldAccessor` and `SplitBitFieldAccessor` presentation, but takes every
layout offset, width and signedness from EDG. Consecutive occupied byte ranges
are tiled with aligned unsigned scalar storage. Zero-width and ordinary fields
end a group. No tile extends beyond the occupied range or the record. Properties
combine split fragments and preserve unrelated bits on every store. Signedness
comes from `field->bit_field_is_signed`, as in EDG C backend
`dump_bit_field_base_type_name`; Boolean fields normalize nonzero inputs to one.

EDG C backend `adjust_bit_field_value` supplies the relevant precedent for
truncating a consumed assignment result. Its C target can otherwise preserve
native member operations, whereas C# setters require the explicit distinction.
The existing volatile bitfield byte-access profile, atomic operations, lowered
lifetime/exception constructs and their adapters remain separate special paths.
This first change does not claim that all exceptional expression contexts have
been localized or that compiler-created ABI helpers can disappear.

Focused verification is `backend/tests/source_expressions_verify.py`: original
EDG C and C++ lowering oracles, static/two-owner generated execution, JIT and
NativeAOT, emitted-body inspection, consumed signed/unsigned truncation, split
64-bit storage with adjacent guards, nested C++ bitfield lvalues, branch-local
work, argument values and assignment sequencing. Existing packed-bitfield and
volatile-bitfield matrices remain regression requirements. All evidence is
preserved under `artifacts/campaign/persistent-continuation`.

Volatile helper deduplication uses the occupied width, bit remainder, signedness,
Boolean status, field/result/operand types and operation. A source field name is
scoped within its record and cannot identify a helper globally. The regression
with `Plain.flag` and `Boolean.flag` exercises this distinction: their properties
have the same source name but different bit positions.

Accepted focused evidence for this increment (all paths beneath
`artifacts/campaign/persistent-continuation`):

- `source-expressions-translated-006/results.json`: ten checks, including void
  comma tails, void conditional branches, numeric conversions and readable use
  sites, with original EDG C/C++ oracles and all four JIT/AOT owner configurations.
- `source-packed-bitfields-005/results.json`: five checks; 142 layouts and 4544
  independent byte-model cases per execution, with neighboring bytes preserved.
- `source-volatile-bitfields-005/results.json`: ten checks; exact C/C++ traces,
  discarded volatile reads, Boolean fields, scoped-name/layout distinction,
  owner/TLS isolation and GC lifetime.
- `source-eh-regression-008/type_info/results.json` and
  `source-eh-regression-008/function_try/results.json`: 21 checks preserving
  RTTI metadata handling, exception/lifetime traces and fatal boundaries.

The RTTI plan consumes `typeinfo_expression` before descending into its opaque
native metadata variable, matching the existing location emitter. Generated
compiler ABI calls similarly retain their specialized operand handling. Native
RTTI descriptors must not become ordinary C# storage just to make traversal
uniform. The source planner also preserves the existing explicit rejection of
unsupported right-to-left lowered operations.

`backend/formatter` provides an independent SDK Roslyn whitespace pass. Its
six synthetic formatting/execution checks are in `source-formatter-001`; the
15-file batch over actual accepted generated outputs is retained in
`source-formatter-generated-001`. The formatter changes no emission semantics
and is not linked into translated applications.

The durable hash index is `evidence/source-expression-components.json`. It
records 52 unique accepted checks and hashes the retained generated sources,
projects, command/results logs, input snapshots and compiler observation. The
005 batch ran 25 checks, but its ten expression checks and the expanded 006
expression checks are superseded by expanded 009 expression checks; the
combined expression/bitfield count stays 25. The 008 RTTI/EH checks add 21 and
the independent formatter checks add six.
The 15-file formatting batch and termination-message audit are supplementary
proofs, not additional counts. These runs used the explicitly recorded compiler
snapshots; this is not a claim that all checks ran against one final executable.

## SQLite pointer compound regression and correction

The first readable SQLite execution after 008 exposed a gap in the preceding
synthetic matrices. The parser statement
`yypParser->yytos = yymsp += yysize+1;` entered the scalar compound-assignment
fallback because an integer displacement cannot implicitly convert to a pointer.
That fallback cast the pointer to `ulong`, adding bytes instead of elements.
SQLite's parser stack entries occupy 24 bytes, so the resulting misaligned stack
corrupted semantic values and eventually caused an access violation. The
original EDG IL and C backend were correct and remain unchanged.

Pointer `+=` and `-=` now use typed pointer arithmetic in both ordinary and
localized expression forms. This retains element scaling, including pointers to
arrays. Side-effecting right operands still execute before the target read in
C++, and complex targets are evaluated once. The generated code can use ordinary
`position += 2` syntax without integer address conversions.

`source-expressions-translated-009/results.json` records ten passing checks:
original EDG C/C++ to C oracles, followed by static and two-owner JIT/NativeAOT
execution. The expanded fixture covers 24-byte records, array-pointer strides,
positive and negative displacement, consumed and discarded results, nested
lvalues, receiver evaluation counts, and an RHS that changes the pointer.
The failed SQLite sources/logs remain under `sqlite-readable-named-002`; the
durable hash index links that evidence. Real SQLite acceptance is recorded
separately by the coordinator and is not inferred from these focused tests.

## Deferred typed-enum integration audit

This is a read-only audit for the next phase. No typed-enum implementation is
part of the accepted component hashes above.

- `source_implicit_conversion` currently reasons about physical integer width
  and signedness. Once `type()` returns a named managed enum, that reasoning
  cannot authorize an implicit conversion to or from another numeric type.
  Preserve direct assignment between the same enum; use explicit conversions
  for other numeric/enum assignments, apart from C#'s zero-constant exception.
- Only an actual EDG `variant.integer.enum_type` should select a managed enum
  type. An ordinary integer with `enum_info.affiliated_type` remains an integer;
  it carries enumeration provenance without cancelling EDG's promotions. This
  matters for arithmetic, conditional operands and default variadic promotions.
- Ordinary arithmetic between different source enums must use EDG's promoted
  physical operands. Casting only the final result cannot make an otherwise
  invalid C# enum operation compile. The comparison planner already computes
  EDG integral promotions; source-preserving simplifications must retain that
  distinction. C enum increments and integer-to-enum stores also need explicit
  coverage, separate from C++ scoped enums.
- Named bitfield getters, setters and local truncation expressions already use
  explicit casts at the storage boundary. Keep enum properties source-typed,
  but calculate compound values in EDG's promoted integer type before the enum
  store. The bitfield width and `bit_field_is_signed` remain authoritative,
  regardless of the declared enum's underlying width.
- Switch labels must match the final governing expression's type. Emit a named
  enum member for an enum switch and the correct numeric conversion for a
  physically promoted switch. The existing contextual `source_enum_case` hook
  is the appropriate integration point.
- Enum pointers, callback signatures and ref aliases must preserve the named
  enum while retaining its physical size/alignment. Variadic argument packing
  must use the lowered promoted argument type; it must not accidentally pack a
  narrow enum where `va_arg(..., int)` expects an integer. CLR enum storage stops
  at 64 bits, so wider EDG enum extensions retain an explicit physical fallback.

Core's prepared `source_enums.c` covers fields/globals/locals, distinct enum
arithmetic, C mutation, enum bitfields, pointers and callback signatures,
variadic use, and C++ scoped/unsigned-64 enums. A narrow fixed-underlying enum
passed through default variadic promotion is an additional useful boundary
case. The next implementation should be verified after the current project
freeze, alongside the pending omission of source-inaccessible padding-field
properties; neither requires changing EDG.

# Generic typing repairs from the real core

The historical C# build at `artifacts/core-execution/attempt-0imxwoxa` exposed three
independent compiler issues. Their original native and generated failures are
retained in `artifacts/core/reduced/typing/receipt.json`.

* **B025 — `_Bool` at numeric sinks.** The C# coercion helper excluded `CBool`
  from its integer spelling table, leaving unsigned call arguments, returns and
  stores without the conversion C requires. It now converts the already
  normalized Boolean value through `int`, then to the numeric destination. This
  also supports wide integer targets without chaining two implicit user-defined
  conversions. No Boolean representation or runtime operator is changed.
* **B026 — array members used with `->`.** Upstream declares
  `struct OpCache opcache[1]` and accesses `m->opcache->field`. Member lookup did
  not apply array-to-pointer decay and silently inferred `int` for the field,
  even though the emitted record had the correct pointer or unsigned type.
  Lookup now resolves the decayed element for direct and promoted fields and
  respects qualified pointer layers. The expression retains its original array
  lvalue for storage emission. The same source pattern occurs in disassembler
  accesses through `d->xedd`.
* **B027 — a floating literal without fractional digits.** C literals such as
  `1.f`, `2.` and `3.e1f` are not valid C# spellings. The C# target inserts a zero
  after the decimal point, preserving the original digits, exponent and suffix
  without reparsing or rounding the numeric value. The SSE diagnostics reporting
  an `int` without field `f` came from `1.f`, not its union-array operand.

Expanded native fixtures cover small and wide unsigned sinks, `UInt128`, floating
Boolean sinks, pointer stores, a promoted union field, const aggregate access,
unsigned arithmetic, signed conversion, omitted fractional parts, hex floats and
negative-zero bits. Historical native receipts are under
`artifacts/core/reduced/typing/native-expanded/`.

Before recovery, default generated execution and separate-object flat/nested execution passed all
nine focused tests (`artifacts/core/typing-focused.log`), with six unavailable
external-platform oracle skips. These receipts do not validate the restored
workspace or its newer fixed-address global storage.

On recovery onto `3f12b2e`, the three typing repairs and the conservative B028
constant-branch emitter were reapplied independently of the newer global/TLS
storage implementation. The old patch's blanket `FixedAddressValueType` field
attribute was deliberately excluded. All four recovered native fixtures pass
strict C17 warnings; the fresh receipt is
`artifacts/core/recovery-native-brv45ycl/receipt.json`.

The combined recovery build succeeded, and fresh focused checks passed two unit
tests and 18 functional tests, with eight unavailable external-platform skips
(`artifacts/core/recovery-typing-units.log` and
`artifacts/core/recovery-typing-functional.log`). This includes direct/object
constant-branch execution, flat/nested typing cases, and the existing scalar/TLS
array address checks under forced compacting GC. Full repository regression and
a fresh real-core build remain required; these focused results do not establish
interpreter execution.

Fresh isolated full repository regression now passes: 2257 unit tests and556
functional tests, with1057 explicit platform skips, zero failures and a successful
Release build (17 existing analyzer warnings). The log is
`artifacts/recovery-isolated-repository.log`. Full translated-core execution
remains a separate gate.

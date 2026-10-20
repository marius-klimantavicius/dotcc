# Produced foundations review — 2026-10-20

Status: **partial code review; no milestone approved**. Scope is the initially
produced `src/EdgSharp/Source.cs`, `Types.cs`, their project files, and the existing
native baseline receipts. Parser, lexer, semantic tests, managed validation and
NativeAOT validation were not available for this review. The reviewer executed
no builds/tests/native processes.

## Produced code observations

No concrete critical/high defect found in these initial two source files.

- `SourceFile` retains immutable strings and private line-start storage. Range
  validation is explicit, ranges are half-open, and UTF-16 column/offset units
  are documented. `DiagnosticBag.Snapshot` copies before read-only wrapping.
- `SemanticType` nodes use reference identity and expose immutable properties.
  `FunctionType` clones its incoming array before read-only wrapping, closing
  the common caller-array/public-downcast mutation issue. `TypeTable` owns its
  dictionaries/lists per instance. The shared LP64 descriptor is immutable.
- LP64 ranks distinguish long and long long despite identical widths.
  `UsualArithmeticConversions` first promotes, then handles same signedness,
  unsigned rank, signed representability and the corresponding unsigned type.
  `ConvertInteger` explicitly implements bool and target-width congruence using
  BigInteger; it does not rely on CLR arithmetic widths.
- Library and executable test project reference .NET 10/C# 14 and contain no
  package or native/compiler dependency. `IsAotCompatible` is declarative;
  executed NativeAOT qualification is still pending.
- Both source files retain `Apache-2.0 WITH LLVM-exception` attribution and
  identify their handwritten adaptation. Detailed correspondence and omitted
  branches still belong in the supervisor's subsystem ledger.

Required checks in later callers/tests (not established defects):

- Validate null target and undefined language enum inputs at the public session
  boundary; do not silently analyze an unspecified profile.
- `TypeTable.Const` returns non-integer types unchanged. A caller must not
  silently discard unsupported qualifiers on void/function types.
- `Function` correctly strips by-value parameter top-level const for canonical
  signatures; definition parameters must retain their own declared const in
  the body. Return type cv must follow its separate EDG rule.
- `Promote` is correct for the fixed LP64 profile's bool/char/short types; it is
  not yet a general width-configurable target promotion implementation.
- Constant folding must distinguish signed overflow from target conversion.
  Calling `ConvertInteger` on every arithmetic result would incorrectly invent
  signed constant values; inspect the actual expression code before acceptance.
- Verify line/column positions at CR, LF, CRLF and EOF, collection immutability,
  same-session/repeated-analysis isolation and separate-session identity through
  the public API once the session implementation exists.

## Native evidence inspected

Read `native-profile.json`, `native-positive.json`, `native-unresolved.json`,
`native-preprocess.json`, `native-semantic-witness.json` and their input bytes.
Recomputed input SHA-256 values match all those receipts. Receipts record
executable SHA-256, exact strict C++20 arguments, disabled predefined macro file,
no code generation, output and return status. These are supervisor-executed
receipts; the reviewer did not independently execute native EDG.

- Profile witness succeeds and explicitly checks `__cplusplus`, char-bit range,
  bool/short/int/long/long long/pointer sizes and signed plain char.
- Positive source succeeds; the unresolved-identifier control fails with the
  expected native diagnostic. This establishes native behavior only.
- The native semantic witness includes the exact hashed positive input and
  proves `Number` aliases int; add/caller calls return int; long + long long is
  long long; unsigned int + long is long; unsigned long + long long is unsigned
  long long. It does not prove local-reference identity or body graph facts.
- Preprocessing receipt is for the macro-free positive source; it does not
  establish macro expansion, include provenance or conditional directive support
  in the managed implementation.

Acceptance still requires the full produced implementation, meaningful managed
semantic tests, negative/malformed cases, exact correspondence/omissions and
executed managed/AOT evidence. Baseline risks remain in `review-baseline.md`.

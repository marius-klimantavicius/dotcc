# C macro overrides and typed runtime intrinsics

Status: **PLANNED — not implemented**. Created 2026-09-11.

## Recommendation

Add two independent features:

1. A preprocessor policy that replaces selected upstream macro definitions with
   configured **C token sequences**, without editing the downloaded source.
2. A small typed intrinsic surface for target/runtime facts. The first intrinsic,
   `__dotcc_is_little_endian()`, becomes a read of
   `global::System.BitConverter.IsLittleEndian` in the C# backend.

Dotcc knows what endianness is, but does not know what `SQLITE_BIGENDIAN` is.
SQLite-specific choices live in a checked-in campaign configuration file.
The override can also replace a macro with ordinary C tokens and need not use
an intrinsic. Likewise, an intrinsic can appear in C input without an override.

This plan changes no implementation, SQLite configuration, or generated output.
Commit each coherent, tested implementation step locally; do not push.

## Why this belongs at two different stages

Currently `CPreprocessor` seeds `-D` definitions, and a later `OnDefine` writes
another definition into the same macro table. Therefore `-DNAME=...` does not
reliably override an unconditional definition in an upstream header.
Redefining a macro after including a complete implementation is too late: its
function bodies have already been preprocessed.

After preprocessing, the macro name is normally gone. Detecting the spelling
`SQLITE_BIGENDIAN` in the C# backend would both couple dotcc to SQLite and miss
that architectural boundary. Text replacement in emitted C# would lose C types,
source positions, macro lifetime, and backend independence.

Use the following sequence instead:

```text
configuration/CLI -> validated macro override rules
C source -> active #define -> select effective macro body -> normal macro expansion
        -> normal C parser -> typed runtime-intrinsic IR -> backend-specific expression
        -> generated C# -> existing optional Roslyn postprocessor
```

The C# syntax is `global::System.BitConverter.IsLittleEndian`, with dots after
`System`; `global::System::BitConverter` is not the intended spelling.

## Proposed user interface

These flags and files are proposals, not commands supported by today's dotcc.

```sh
dotcc engine.c --emit=managedlib --class-name Sqlite --namespace Managed.Database \
  --override-macro 'SQLITE_LITTLEENDIAN=(__dotcc_is_little_endian())' \
  --override-macro 'SQLITE_BIGENDIAN=(!__dotcc_is_little_endian())'
```

Prefer a persistent profile for a campaign:

```json
{
  "version": 1,
  "macroOverrides": [
    {
      "name": "SQLITE_LITTLEENDIAN",
      "replacement": "(__dotcc_is_little_endian())",
      "requireMatch": true
    },
    {
      "name": "SQLITE_BIGENDIAN",
      "replacement": "(!__dotcc_is_little_endian())",
      "requireMatch": true
    }
  ]
}
```

Save this later as `sqlite/config/dotcc-overrides.json`, then pass:

```sh
dotcc engine.c --overrides-file sqlite/config/dotcc-overrides.json ...
```

There is intentionally no override for `SQLITE_UTF16NATIVE`: its existing C
replacement list refers to `SQLITE_BIGENDIAN`, so ordinary rescanning composes
the changes. The resulting expression has these semantics:

```csharp
(byte)(global::System.BitConverter.IsLittleEndian ? 2 : 3)
```

The actual emitter may retain CBool conversions, negation, or extra parentheses;
the acceptance requirement is a correctly typed BCL property read with no
`sqlite3one` address probe. The existing postprocessor can simplify supported
Cond.B/CBool patterns. This work does not introduce an unrelated general-purpose
Boolean optimizer merely to force the exact illustrative spelling above.

**Polarity matters:** `SQLITE_BIGENDIAN` must use the negated intrinsic. Mapping
it directly to `IsLittleEndian` would select the wrong UTF-16 encoding and can
also change SQLite's WAL checksum handling.

## Override semantics: replace when defined

The first version supports object-like macros and one optional JSON profile,
plus repeated CLI rules. It does not override ordinary C variables/functions or
accept raw C# replacement strings.

- A rule does not predefine its name. It becomes effective when an active source
  or included-header `#define NAME ...` is processed. Earlier uses, `#ifdef`,
  `#ifndef`, and `defined(NAME)` retain their normal temporal behavior.
- On that definition, validate the original macro and install the replacement
  body as the effective macro. Expand its tokens at the normal use site, not
  when loading the configuration or encountering the definition.
- `#undef NAME` removes the definition normally. A later active redefinition
  reapplies the rule. Inactive conditional branches do not match rules.
- A function-like definition matched by an object-like rule is a clear error,
  rather than a silent arity change. Parameterized/variadic rules are later work.
- Apply a profile to every C translation unit in the invocation, with a fresh
  macro table per unit. Match requirements are checked across the invocation,
  not independently for each unit; an unrelated C unit need not define the macro.
- The profile is the base layer; an explicit CLI rule for a name replaces that
  profile entry. Reject duplicate names within a layer, unknown schema fields,
  invalid names, directive/newline injection and malformed replacement tokens.
- Keep `-D` semantics unchanged. Initially reject using `-D` and an override rule
  for the same name in one invocation, rather than inventing implicit precedence
  between a seed definition and a replace-when-defined rule.
- `requireMatch` defaults to true; a rule matching no active source definition
  fails with the name and configuration location. Allow explicit false for
  profiles spanning optional features. Report definitions matched separately
  from source expansions, so a defined-but-unused macro remains visible.
- Diagnostics retain both the original definition/use location and the rule's
  profile location. A trace must make it clear that an override changed the body.

Add an optional `expect` list of original replacement strings to a rule for
upstream-drift detection. Compare token sequences after normal whitespace/comment
normalization, before expanding macros. If a matched active definition differs
from every expected body, fail rather than silently overriding new semantics.
This is configurable detection of source changes, not built-in recognition of
any library. Exact values for the SQLite profile must be taken from its pinned
source during integration; multiple conditional definitions can be allowed
explicitly when necessary.

Do not add a permanently forced macro mode in the first implementation. A future
separately named mode could deliberately survive source `#undef` and ignore
source redefinitions, but that changes include guards and feature detection and
must not be confused with this contract.

## Typed intrinsic semantics

Recognize `__dotcc_is_little_endian()` as a reserved, zero-argument builtin in C
expression binding. No user prototype/header is required. Validate its arity and
reject attempts to declare, redefine, take its address, or assign to it; it is
an intrinsic expression, not a linkable libc function or mutable global.
Unknown unresolved `__dotcc_*` builtin invocations should produce a targeted
diagnostic, not survive as accidentally unresolved C# identifiers. Preserve
resolution of existing declared dotcc runtime helpers before applying that check.

Represent it with a typed IR node such as
`RuntimeIntrinsic(IsLittleEndian)` and an explicit C `_Bool` result type. Add its
purity/effect and runtime-value classification to the relevant IR visitors.
Do not represent it as an untyped C# source fragment or resolve it via reflection.

C# lowering reads `global::System.BitConverter.IsLittleEndian`. In contexts
where dotcc represents C Boolean values as `CBool`, emit a compatible conversion,
for example `((CBool)global::System.BitConverter.IsLittleEndian)`. Preserve integer
0/1 semantics for arithmetic, comparisons, casts, arguments, return values,
ternary expressions and stores. A raw C# bool is not valid in every C integer
context; preserve the existing typed conversion machinery and test both value
and condition contexts. An intrinsic has no addressable storage.

This is a **target runtime property**, not the endianness of the machine running
dotcc. Do not evaluate `BitConverter.IsLittleEndian` inside the compiler and emit
its current result. Leave it as a runtime IR expression even if the eventual JIT
can fold the property. Initial C# support is mandatory; other backends must either
implement their own specified semantics or reject the intrinsic explicitly.
For the first increment, WAT may reject it with a precise unsupported-intrinsic
error; it must not emit C# text or inherit the compiler host's value. C overrides
must not alter Zig input in a mixed compilation.

### Compile-time versus runtime expressions

A C preprocessor conditional cannot evaluate a .NET runtime property. After
expanding a `#if`/`#elif` expression, detect runtime intrinsic tokens and issue a
specific error (including indirect macro expansion), instead of applying the
usual undefined-identifier-to-zero rule. `#ifdef` and `defined` test macro
presence only and remain valid; their name operands are not runtime evaluations
and must be excluded from this check. Treat an expanded runtime intrinsic in such a
conditional as an error even if short-circuit evaluation would skip it.

Similarly, reject a runtime intrinsic wherever a C constant expression is
required: enum values, case labels, static assertions, fixed compile-time array
bounds and applicable static-storage initializers. Do not pretend it is a C
integer constant just because its target value is stable during execution.
An ordinary configured numeric replacement remains usable in these contexts.

The constant-macro exporter must inspect effective replacement bodies and omit
runtime-valued macros from `public const` output, recording the reason in its
report. Do not emit invalid C# const fields or add public properties as an
implicit part of this feature. Existing numeric/string macro exports continue
to work, including constants supplied by overrides.

## Integration points in this repository

| Layer | Planned work |
| --- | --- |
| CLI and public API | Add `--override-macro`, `--overrides-file`, and an optional strongly typed C preprocessing-options argument. Parse JSON with AOT-safe BCL APIs; avoid reflective serialization/dependencies. |
| Frontend requests | Thread the same immutable normalized rules through `Compiler.BuildIr`, `FrontendRequest`, C frontend discovery/emission passes, mixed-input handling and standalone preprocess/dependency entry points. |
| Preprocessor | Apply rules in `CPreprocessor.OnDefine`, retain normal `OnUndef`, expose effective macro bodies, preserve lazy expansion/source provenance and diagnose runtime intrinsics in conditional evaluation. |
| Expression binding/IR | Add the typed intrinsic node and generic reserved-name/arity validation; update walkers, effect analysis and constant evaluation to understand it. |
| C# backend | Emit the fully qualified BCL property with correct CBool/numeric/context conversions and expression precedence; do not add SQLite names. |
| Other backends | Give explicit intrinsic support or rejection. Do not require Roslyn in the compiler/runtime. |
| Macro exports | Use effective definitions; never export runtime intrinsics as C# constants. Preserve source/object conflict handling. |
| Object output/linking | Lower intrinsics using the chosen target at object creation. Current objects contain generated C# fragments, so preserve those expressions plus version/profile provenance; do not promise source macro re-expansion at link time. |
| Diagnostics/reproducibility | Record normalized rules, config hash, original/effective definitions, source locations, match counts, skipped exports and intrinsic dependencies in an optional override report. Report once, not once per frontend pass. |

The same profile must govern `-E`, source emission, object creation, include and
dependency discovery. Add the JSON profile path to dependency output so a changed
profile triggers rebuilds; repeated CLI values are part of the build command.
Compiler passes must not disagree about an overridden macro's type-affecting
expansion or selected include path. Reports from discovery/speculative macro
harvesting must not inflate actual source-use counts.

Source rules apply before object creation. Reject override flags for an
object-only link with an explanation to rebuild objects. For mixed source/object
input, apply rules only to the C sources and report that existing objects retain
their recorded translation profile. Different per-unit profiles are not inherently
invalid, but preserve current conflicting macro-constant diagnostics/omissions.
Older objects without profile metadata remain linkable with an explicit unknown
provenance status; no silent claim that current overrides affected them.

## Implementation milestones and gates

### M0 — Freeze the contract

- [ ] Confirm the replace-when-defined lifecycle, strict match/expect behavior,
      schema/version, CLI precedence, and constant-expression diagnostics above.
- [ ] Document proposed API additions without breaking existing callers.
- [ ] Record small native C oracle fixtures for normal definition/undefinition,
      macro rescanning and endian-probe semantics before changing implementation.

### M1 — Configurable macro definition replacement

- [ ] Implement validation and propagation through all C preprocessing paths.
- [ ] Test original-source redefinitions, undef/redefine, include guards, inactive
      branches, nested aliases, token pasting/stringification interactions,
      multiple translation units, expected-body mismatches and unmatched rules.
- [ ] Verify unchanged ordinary `-D` behavior, deterministic reports and identical
      effective expansion in discovery, `-E`, compilation and dependency output.
- [ ] Cover effective numeric/string macro exports and reject function-like rule
      mismatches. Commit this independently of the endian intrinsic.

### M2 — Runtime endian intrinsic

- [ ] Add typed binding/IR/C# lowering and explicit unsupported-backend behavior.
- [ ] Test values, conditions, negation, ternaries, arithmetic, casts, returns,
      argument conversions, invalid arity, address-taking and reserved-name misuse.
- [ ] Test both Boolean polarities using constant-valued provider-independent
      fixtures where appropriate; on real execution compare against the BCL
      property. Inspect emitted code to ensure no host-dependent folding occurred.
- [ ] Reject runtime use in preprocessing/constant-expression contexts; skip
      runtime macro constants correctly, including aliases and combined expressions.
- [ ] Exercise source/object emission and linking, custom namespaces, every split
      mode, raw/postprocessed code and NativeAOT; keep all lowering type-aware.

### M3 — SQLite integration without compiler coupling

- [ ] Add `sqlite/config/dotcc-overrides.json` and wire it into translation only.
      Record expected original definitions for the pinned SQLite profile.
- [ ] Replace both endian macros using the generic facility. Leave upstream C,
      `SQLITE_UTF16NATIVE`, and the native SQLite oracle unchanged.
- [ ] Regenerate SQLite and inspect all generated files: endian use sites read
      the BCL property; no expression takes `sqlite3one`'s address for this probe.
      An unused original declaration may remain; deleting it is not required.
- [ ] Run UTF-16LE/BE/native C APIs, encoding conversions, and databases whose
      text encoding differs from the host. Compare with native SQLite results.
- [ ] Run WAL checksum/checkpoint/recovery and native database interoperability,
      product layout checks and ManagedConsumer under JIT and NativeAOT.
      Endianness affects storage behavior, not just readability of UTF-16 code.
- [ ] Verify the JSON profile is a build dependency and document exact commands,
      profile trace, results and remaining portability limits. Commit locally.

Completion requires working ordinary macro overrides independent of SQLite,
typed intrinsic behavior independent of overrides, equivalent source/object
results, and verified SQLite behavior using configuration alone.

## Later extensions, deliberately separate

- Function-like macro overrides with explicit parameter/variadic contracts.
- Origin-file selectors for macros shared by unrelated included libraries, if
  real integrations require narrower scope than invocation-wide name matching.
- Additional generic intrinsics with typed signatures and backend mappings.
- Explicit function/symbol binding overrides at semantic binding time, rather
  than rewriting every occurrence of a token or replacing arbitrary C# text.
- Generic structural recognition of endian probes in IR. This would require
  proofs of initialization, aliasing, mutation, storage and byte-access semantics;
  it is not necessary for the explicit macro solution and must never match a
  library's variable/macro name. Keep it a separate optimization proposal.

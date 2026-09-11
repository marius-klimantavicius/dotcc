# C macro overrides and typed runtime intrinsics

Status: **COMPLETE** (2026-09-11). M0–M3 implemented and validated. Usage/API:
[macro-overrides.md](../macro-overrides.md). SQLite commands and evidence:
[sqlite/docs/macro-overrides.md](../../sqlite/docs/macro-overrides.md).


## Recommendation

Implemented as two independent features (see [usage and API](../macro-overrides.md)):

1. A preprocessor policy that replaces selected upstream macro definitions with
   configured **C token sequences**, without editing the downloaded source. Select
   by name alone, exact body tokens, or a body regex with named captures; support
   templates referring to both captures and original macro parameters.
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

## Definition matching and replacement templates

A selector is optional. **Without a body selector, a rule applies to every active
source definition of the named macro**, subject only to any explicit signature
selector. With a selector, a nonmatching definition is installed unchanged.
Reconsider the rules on every active `#define`, including after `#undef`.
This is selection, not an error condition.

For a precise endian-probe override:

```json
{
  "name": "SQLITE_BIGENDIAN",
  "match": {
    "exact": "(*(char *)&sqlite3one == 0)"
  },
  "replacement": "(!__dotcc_is_little_endian())"
}
```

`exact` compares C preprocessing tokens, ignoring whitespace and comments.
It does not expand aliases, discard parentheses, or prove algebraic equivalence.
In particular, the currently inspected SQLite definition contains `(&sqlite3one)`;
its exact selector would be `"(*(char *)(&sqlite3one)==0)"`. Those extra parentheses
are significant to exact matching. This avoids silently changing the scope of a
rule described as exact. Use a regex to accept deliberately chosen spelling
variants, or multiple exact rules for clearly enumerated alternatives.

For a parameter/capture example:

```c
#define X(n) do_call(n, 5)
```

```json
{
  "name": "X",
  "signature": { "kind": "function", "parameters": ["n"], "variadic": false },
  "match": {
    "regex": "do_call\\(\\s*n\\s*,\\s*(?<num>\\d+)\\s*\\)"
  },
  "replacement": "dotcc_mama(${__dotcc_n}, ${num})"
}
```

This produces the effective definition:

```c
#define X(n) dotcc_mama(n, 5)
```

At a later call `X(value + 1)`, normal macro expansion produces
`dotcc_mama(value + 1, 5)`. The override applies to the **definition**, not to each
invocation's source text. `dotcc_mama` is an ordinary function name unless a
separate declaration/intrinsic defines it; this mechanism does not turn arbitrary
replacement names into new compiler intrinsics.

### Selector contract

- `match` is optional and contains exactly one of `exact` or `regex`. Reject a
  malformed selector; never interpret a broken regex as no match.
- Match the original, unexpanded replacement body only, excluding `#define`,
  the macro name, and its parameter list. A separate optional `signature` selects
  object/function form and, for functions, the ordered formal names and variadic
  shape. Omitted signature means any original shape; explicit mismatch skips
  the rule rather than turning a function-like definition into an object macro.
- Preserve the original parameter list and variadic flag in the effective macro.
  The override changes its body, not its public invocation syntax or arity.
- Regex input is the original logical body spelling after line splicing and
  comment removal (comments act as whitespace), with leading/trailing whitespace
  trimmed and internal whitespace preserved. String/character literal contents
  remain intact. Do not fabricate this string by concatenating token spellings.
- Use full-body, case-sensitive matching with .NET-style named groups. Anchor
  internally as `\A(?:pattern)\z`; partial matching requires an explicitly
  written pattern such as `.*do_call.*`. The JSON example uses `\s*` to tolerate
  spacing; `do_call\(n, (?<num>\d+)\)` also works for that literal
  spacing. Regex delimiters `/.../` are explanatory notation, not part of JSON.
- Because regex matches spelling, a changed formal name can change its result.
  No automatic alpha-renaming or C expression parsing is implied. Exact and regex
  are complementary: exact is the robust default for token-identical bodies;
  regex is the explicit tool for spelling variation and captured replacements.
- Keep an ordered list of rules per name. **First matching rule wins**, evaluated
  against the original definition. Later rules do not see a replacement produced
  by earlier rules. Put a name-only fallback last; reject trivially unreachable
  later rules after an unconditional same-name rule. Report which rule won.

Example lifetime behavior for a selective rule matching only `do_call(n, 5)`:

```c
#define X(n) do_call(n, 5)   /* selected -> dotcc_mama(n, 5) */
/* uses see the replacement */
#undef X                    /* X really becomes undefined */
#define X(n) other_call(n)   /* not selected -> remains other_call(n) */
/* uses see the original second definition */
#undef X
#define X(n) do_call(n, 5)   /* selected again */
```

### Template contract

- `${num}` inserts the text captured by the regex group named `num`. Only named
  captures are supported initially; reject missing/unmatched groups used by the
  template. A referenced group must have exactly one capture, avoiding implicit
  last-capture behavior for repeated groups. Groups not referenced may repeat.
- `${__dotcc_n}` inserts the **formal parameter token** named `n` in the selected
  original function macro. It does not insert an already-expanded actual argument.
  The same rule applies to other formal names. Reserve the `__dotcc_` group-name
  prefix so regex captures cannot shadow these parameter placeholders.
- Preserve existing standard variadic semantics. `__VA_ARGS__` may appear as a
  normal C replacement token for an originally variadic macro; no new parameter
  syntax or variadic behavior is introduced by templates.
- Literal formal names remain valid C replacement tokens, but placeholders make
  the binding intention explicit and diagnose an absent parameter. Undefined
  parameter references in a selected rule are an error; signature mismatch is
  instead a nonmatch. An object-like definition cannot supply formal parameters.
- Support only `${name}` and `$$` (literal dollar) as template escapes. Substitute
  once; captured text is not recursively interpreted as another template. Do not
  apply `Regex.Replace` replacement syntax, `$1`, `$&`, scripting, or C# evaluation.
- Parameter placeholders occupy complete preprocessing tokens, not substrings
  inside literals or identifiers. Use ordinary C `#`/`##` where stringification or
  token pasting is intended. Validate parameter placement and the expanded token
  stream before installing the effective definition.
- Lex the constructed body as C preprocessing tokens and validate normal macro
  constraints, parameter use, `#`/`##`, and variadic tokens. Preserve original
  definition/use provenance and record the rule, captures and resulting body.
  Replacement templates cannot introduce new preprocessor directives.
- Invocation arguments then follow the existing macro expander's rules for
  prescan, rescanning, recursion suppression, stringification and token pasting.
  Preserve argument spelling/parentheses rather than inventing grouping or an
  extra evaluation. A template that repeats a parameter intentionally repeats
  normal C macro argument evaluation; this feature is not a semantics-preserving
  optimizer of arbitrary user-authored replacement bodies.

## Override semantics: replace when defined

The first version includes object-like and function-like macro body overrides,
one optional JSON profile, and repeated CLI shorthand rules. It does not override
ordinary C variables/functions or accept raw C# replacement strings. Use JSON
for selectors and capture templates; the existing proposed `--override-macro
'NAME=BODY'` shorthand stays a name-only rule with a literal replacement body.

- A rule does not predefine its name. Earlier uses, `#ifdef`, `#ifndef`, and
  `defined(NAME)` retain their normal temporal behavior.
- On each active source/included-header definition, try the matching rules and
  install the selected effective body, or the unchanged original when none match.
  Expand tokens at the normal use site, not when loading configuration.
- `#undef NAME` removes the definition normally. A later active redefinition
  starts selection again. Inactive conditional branches do not match rules.
- Apply a profile to every C translation unit in the invocation, with a fresh
  macro table per unit. Report and aggregate rule matches across the invocation;
  an unrelated C unit need not define the macro.
- The profile is the base layer. An explicit CLI shorthand rule for a name
  replaces that name's entire profile rule list; report this precedence clearly.
  Reject duplicate CLI shorthand names, malformed/unknown schema fields, invalid
  names and malformed replacement tokens. Multiple ordered JSON rules for the
  same name are valid and necessary for different definitions.
- Keep `-D` semantics unchanged. Initially reject using `-D` and an override rule
  for the same name, rather than inventing precedence between a seed definition
  and a replace-when-defined rule.
- `requireMatch` is an optional **whole-invocation assertion**, default false.
  When true, a rule that selects no active definition fails after processing;
  ordinary nonmatching definitions still remain unchanged. This is useful for
  verifying a pinned campaign actually used a rule and is distinct from selector
  mismatch. Report candidates, signature/body nonmatches, selected definitions,
  rules shadowed by earlier matches, and actual source expansions separately.
- Keep optional `expect` as a strict, separate upstream-drift assertion: after a
  rule is selected, its original body must match one listed exact token sequence,
  otherwise fail. `match` chooses whether to replace; `expect` validates a chosen
  replacement. A regex selector can therefore accept broad spelling while an
  optional expectation asserts the approved bodies. Omit `expect` for permissive
  capture-driven transforms. Do not treat either assertion as an implicit selector.
- Diagnostics retain both original definition/use locations and the rule's
  configuration location, together with an override trace.

Use BCL regex evaluation compatible with NativeAOT and runtime-supplied patterns;
no generated dynamic code is required. Use a finite per-match timeout and bounded
pattern/body/output sizes, with deterministic diagnostic categories for invalid
patterns, timeouts and oversized results. Validate capture references as early as
possible and leave no half-installed macro when evaluation or tokenization fails.

Do not add a permanently forced macro mode in the first implementation. A future
separately named mode could deliberately survive `#undef`, but that changes
include guards and feature detection and is not this contract.

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
| Preprocessor | Parse original signatures, retain logical body spelling and tokens, select ordered rules in `CPreprocessor.OnDefine`, interpolate/validate templates, retain normal `OnUndef`, and expose effective macro bodies. Preserve lazy expansion/source provenance and diagnose runtime intrinsics in conditional evaluation. |
| Expression binding/IR | Add the typed intrinsic node and generic reserved-name/arity validation; update walkers, effect analysis and constant evaluation to understand it. |
| C# backend | Emit the fully qualified BCL property with correct CBool/numeric/context conversions and expression precedence; do not add SQLite names. |
| Other backends | Give explicit intrinsic support or rejection. Do not require Roslyn in the compiler/runtime. |
| Macro exports | Use effective definitions; never export runtime intrinsics as C# constants. Preserve source/object conflict handling. |
| Object output/linking | Lower intrinsics using the chosen target at object creation. Current objects contain generated C# fragments, so preserve those expressions plus version/profile provenance; do not promise source macro re-expansion at link time. |
| Diagnostics/reproducibility | Record normalized rules, config hash, original/effective definitions, signature/body nonmatches, selected rule and captures, source locations, match counts, skipped exports and intrinsic dependencies. Report once, not once per frontend pass. |

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

- [x] Confirm replace-when-defined lifecycle, exact/regex selectors, first-match
      ordering, formal/capture placeholders, optional assertions, schema/version,
      CLI precedence and constant-expression diagnostics above.
- [x] Document proposed API additions without breaking existing callers.
- [x] Record a small native C oracle fixture for macro rescanning and endian-probe
      semantics, alongside definition/undefinition preprocessing regression tests.

### M1 — Configurable macro definition replacement

- [x] Implement validation and propagation through all C preprocessing paths,
      retaining original logical body spelling for regex selection alongside tokens.
- [x] Test original-source redefinitions, undef/redefine, include guards, inactive
      branches, nested aliases, token pasting/stringification interactions,
      multiple translation units, selector nonmatches, expected-body assertions
      and explicit requireMatch failures. Verify each definition in a repeated
      define/undef/define sequence is selected independently.
- [x] Verify unchanged ordinary `-D` behavior, deterministic reports and identical
      effective expansion in discovery, `-E`, compilation and dependency output.
- [x] Test exact-token versus regex spelling semantics, comments/continuations,
      named captures, changed numeric literals, first-match/fallback precedence,
      invalid regex, timeout, unknown or repeated captures and malformed output.
- [x] Test the X(n) example through invocation, side-effecting arguments, multiple
      formal names, zero-argument functions, standard variadics, #/##, recursion,
      missing parameter placeholders and signature changes across redefinitions.
- [x] Cover effective numeric/string macro exports and NativeAOT loading of a
      runtime-supplied regex profile. Commit independently of the endian intrinsic.

### M2 — Runtime endian intrinsic

- [x] Add typed binding/IR/C# lowering and explicit unsupported-backend behavior.
- [x] Test values, conditions, negation, ternaries, arithmetic, casts, returns,
      argument conversions, invalid arity, address-taking and reserved-name misuse.
- [x] Test both Boolean polarities using constant-valued provider-independent
      fixtures where appropriate; on real execution compare against the BCL
      property. Inspect emitted code to ensure no host-dependent folding occurred.
- [x] Reject runtime use in preprocessing/constant-expression contexts; skip
      runtime macro constants correctly, including aliases and combined expressions.
- [x] Exercise source/object emission and linking, custom namespaces, every split
      mode, raw/postprocessed code and NativeAOT; keep all lowering type-aware.

### M3 — SQLite integration without compiler coupling

- [x] Add `sqlite/config/dotcc-overrides.json` and wire it into translation only.
      Select the precise original probe bodies for the pinned SQLite profile,
      with requireMatch enabled to verify this translation encountered the probes.
      Known numeric definitions with different bodies remain unchanged.
- [x] Replace both endian macros using the generic facility. Leave upstream C,
      `SQLITE_UTF16NATIVE`, and the native SQLite oracle unchanged.
- [x] Regenerate SQLite and inspect all generated files: endian use sites read
      the BCL property; no expression takes `sqlite3one`'s address for this probe.
      An unused original declaration may remain; deleting it is not required.
- [x] Run UTF-16LE/BE/native C APIs, encoding conversions, and databases whose
      text encoding differs from the host. Compare with native SQLite results.
- [x] Run WAL checksum/checkpoint/recovery and native database interoperability,
      product layout checks and ManagedConsumer under JIT and NativeAOT.
      Endianness affects storage behavior, not just readability of UTF-16 code.
- [x] Verify the JSON profile is a build dependency and document exact commands,
      profile trace, results and remaining portability limits. Commit locally.

Completion requires working name-only, exact and regex/capture macro overrides
(including function-like definitions) independent of SQLite,
typed intrinsic behavior independent of overrides, equivalent source/object
results, and verified SQLite behavior using configuration alone.

## Completion evidence

- Generic name/exact/regex overrides, original signatures, formal/capture
  templates, assertions, CLI precedence, dependencies and object provenance.
- Typed runtime `_Bool` intrinsic, C# lowering, reserved-name/constant-context
  diagnostics and explicit WAT rejection. No compiler-host folding.
- 2,094 unit tests passed; 387 functional tests passed, 969 opt-in tests skipped.
  Six source/object and split-mode runtime combinations passed.
- NativeAOT compiler loaded a runtime regex profile. Generated JIT/NativeAOT
  fixture programs matched gcc, with mixed source/object and depfile checks.
- SQLite raw/postprocessed builds and ManagedConsumer passed with zero warnings.
  Profile selects both original probes; generated code has 26 BCL reads and only
  an unused `sqlite3one` declaration. JSON profile is in `engine.d`.
- Native UTF-16LE/BE API/database exchange, full WAL/process interoperability and
  recovery campaigns passed under JIT and NativeAOT. Product layout passed
  41 offsetof contracts, 67 field offsets, 48 aggregate and 8 inline-array checks.
- Tested on Linux x64. Opposite-endian data is covered; execution on a big-endian
  .NET host and other OSes was not performed in this campaign. Later extensions
  below remain separate. Commits are local; nothing was pushed.

## Later extensions, deliberately separate

- Signature-changing macro rewrites, if needed; this plan preserves original
  parameters/arity and existing variadic behavior.
- Origin-file selectors for macros shared by unrelated included libraries, if
  real integrations require narrower scope than invocation-wide name matching.
- Additional generic intrinsics with typed signatures and backend mappings.
- Explicit function/symbol binding overrides at semantic binding time, rather
  than rewriting every occurrence of a token or replacing arbitrary C# text.
- Generic structural recognition of endian probes in IR. This would require
  proofs of initialization, aliasing, mutation, storage and byte-access semantics;
  it is not necessary for the explicit macro solution and must never match a
  library's variable/macro name. Keep it a separate optimization proposal.

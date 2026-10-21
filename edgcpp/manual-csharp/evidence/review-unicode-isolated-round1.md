# Independent isolated Unicode review — round 1

2026-10-21. Reviewer: GPT-6.1 Sol, xhigh, independently delegated as requested.

**Decision: changes required; isolated acceptance is withheld.** U1 and U2 below
are open high correctness findings. Passing authored groups do not cover them.
This report does not accept main-checkout Unicode integration,
full M1, or the remaining PLAN.

The reviewed checkout is
`/home/marius/.t3/worktrees/dotcc/t3-96dc97a5`, branch
`t3/unicode-identifier-frontend`, HEAD/base
`3b667ce7350337cfff45e2de78d923c42c750274`. It contains the accepted 79-group
base plus ten authored Unicode groups, not main's later 95-group scalar-reference
integration at `7c16cf48dbcdb57c070ca90625ebec4b703c66a4` or its current storage
evaluator work. The implementation author remains READY/HOLD. The supervisor's
three explicit tool copies are separate from the author's 27-file map.

I read the main manual-campaign AGENTS, PLAN, SUPPORT, PROGRESS and complete
FEATURE-INVENTORY, the applicable parent instructions, the full held handoff,
the produced changes/new module/tests/fixtures/data and the cited native
branches. I changed no implementation, test, fixture, data or Git state, and ran
no build, test harness, compiler, native oracle or frontend. Read-only scripts
computed fingerprints and compared declarative source data. My only filesystem
write is this authorized report.

**U1 — High: active UCN diagnostics disappear inside pp-numbers and literal
suffixes when an argument is discarded. Open; acceptance blocker.**

Affected held code:

- `src/EdgSharp/UnicodeIdentifiers.cs:124–127`: `PartLength` calls `ReadUcn`
  and discards both its success result and `Problem`.
- `src/EdgSharp/Tokens.cs:269–288`: pp-number collection consumes that length
  into an IntegerLiteral/FloatingLiteral token without retaining UCN problems.
- `src/EdgSharp/Tokens.cs:406–411`, called by quoted/raw scans at 362/401:
  suffix collection likewise consumes a UCN without retaining its problems.
- `src/EdgSharp/Preprocessing.cs:219–222`: active validation checks only
  `TokenKind.Identifier`. The containing number/character/string token bypasses
  it in `ReadActive`, expansion and final output validation.

Each of these exact inputs illustrates the lost error:

```cpp
#define DROP(x) 1
int value=DROP(1\uD800);
```

```cpp
#define DROP(x) 1
int value=DROP(U'a'_\uD800);
```

```cpp
#define DROP(x) 1
int value=DROP("x"_\uD800);
```

By inspection, held managed processing consumes the malformed UCN into the
non-Identifier token, drops the argument without decoding it, and reaches valid
`int value=1;` without recording that error. This managed behavior is a traced
source result, not a reviewer-executed test. The same blind spot applies to
preprocessing-only/stringized cases; checking only final output or the character
value decoder cannot repair the initial active scan contract.

Native correspondence: `lexical.c:f_is_identifier_char` around 10196 calls
`scan_universal_character` with `issue_diagnostics=TRUE`, even before deciding
whether the scalar is an identifier character. Both `scan_number` around
12730–12743 and `check_for_ud_suffix` around 14466–14491 use that predicate.
`scan_universal_character` around 13339 suppresses diagnostics in a skipped PP
group. Character literal *contents* instead use a diagnostic-suppressed initial
scan in `accum_quoted_string` around 14127–14162. These are different branches.

The parent/supervisor executed new main-checkout oracle controls after my early
finding. I inspected their actual inputs, commands, hashes and results:

- `evidence/native-unicode-preview-dropped-ppnumber-ucn-round1.json`:
  input SHA `e893970697f8d2d3bf4f688c4c36d9984b613b77ee91f54155af202ed6e7f9c4`.
- `evidence/native-unicode-preview-dropped-character-suffix-ucn-round1.json`:
  input SHA `d79616d07bf496a685b9fb7adfa331e9b9e0e9c3bb5e063aa9c1d19ecb902b7d`.
- `evidence/native-unicode-preview-dropped-string-suffix-ucn-round1.json`:
  input SHA `81f4373b309efe606baede6e6c0421bfb7a9a87b69d59c760cfd5ded1c210a24`.

All three explicitly use the two arguments `--unicode_source_kind UTF-8`, strict
C++20, `--instantiate used`, and the recorded test-only compiler SHA
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
All actually reject with language exit 2 and diagnose the surrogate UCN. Input
hashes match disk. These are native controls, not managed regression passes.

Closure requires handwritten scanner/PP integration that retains the appropriate
UCN errors in numeric/suffix token paths and reports them under the same active
versus skipped timing policy. Add actual source-text regressions for dropped
and stringized pp-number, character/string/raw suffixes; malformed digits,
surrogates and out-of-range values; active and eligible/taken-elif scans; and
outer inactive suppression. Preserve lazy invalid UCN *contents* in unused
definitions and discarded character literals, valid supplementary suffix
boundaries, physical ranges and header-name opacity. Execute matching held
managed/AOT gates and exact-byte native controls. Repair and independent recheck
remain supervisor/implementation work; no source repair was made by this review.

**U2 — High: original macro replacement spelling changes observable nested
stringization data. Open; acceptance blocker.**

Native `macro.c:9911–9916` canonicalizes Unicode identifier replacement text with
`make_canonical_identifier(..., force_ucn=TRUE)`. Held replacement tokens retain
their original spelling, and `MacroExpansion.cs:232–240` stringizes `token.Text`.
The handoff identifies this as a representation adaptation. It changes the
result of implemented preprocessing operations, not just physical provenance.
The following source distinguishes the behaviors:

```cpp
#define A é
#define S(x) #x
#define E(x) S(x)
#define EE(x) E(x)
E(A)
EE(E(A))
```

Held code, by inspection, produces `"é"` followed by `"\"é\""`. The outer
stringization exposes the inner spelling as literal data. The parent executed
new native controls after my source-contract question; I audited their actual
inputs/hashes, explicit UTF-8/strict/used commands and exit-zero results:

- `native-unicode-preview-replacement-stringize-round1.json`, input SHA
  `127a78efe2963cde67a724694a3ec0ab13cca5f7c99c9dff7933f33e17f9fa56`,
  outputs `"\u00e9"` followed by `"\"\\u00e9\""`.
- `native-unicode-preview-replacement-stringize-values-round1.json`, input SHA
  `56169d7cdfecd7982f8a812b35c05df1715792ad644ba440a97eb7ef855d3180`,
  accepts `sizeof(EE(E(A)))==9` and asserts the resulting ASCII quote,
  backslash, `u`, `e`, `9`, quote data. This proves the distinction is observable
  beyond the native preprocessing printer. It does not implement full managed
  string values, which remain Unsupported.
- `native-unicode-preview-stringize-contexts-round1.json`, input SHA
  `b6557f5440a51b575dff45ef3296273ce692ee584cb429f4d7466159096d4b94`,
  accepts and separately shows that direct `S(é)` / `E(é)` preserve direct
  spelling and `S(\u00E9)` / `E(\u00E9)` preserve that original uppercase UCN;
  expansion from `#define A é` or `#define B \u00E9` instead gives canonical
  lowercase `"\u00e9"`. `S(ID(é))` retains raw `ID(é)` and `E(ID(é))`
  ultimately retains direct `é`.

All three use the same pinned compiler SHA recorded under U1, match their input
bytes, and actually accept with exit 0. No managed execution of these new probes
was performed by the reviewer.

Closure requires following the native replacement-storage versus argument-raw
spelling distinction through rescanning, stringization and paste, while
retaining original physical spelling/contributors separately. Simply
canonicalizing every stringized identifier would break the direct-argument
controls. Add direct/UCN/supplementary replacement and argument variants,
one/two-level expansion, nested stringization, retained original origins and
paste controls to the managed suite. Compare actual PP spellings with exact
native output and rerun held managed/AOT qualification. The representation
adaptation currently described in the handoff is not accepted as harmless.

**Executed evidence audited so far.**

The following are in the *isolated* campaign's `evidence/` directory:

- `unicode-isolated-prevalidation.json` records the correct worktree/base,
  explicit author hold, 89 expected groups and three copied tool fingerprints.
- `unicode-isolated-build-round1.json`: Release solution build, exit 0,
  zero warnings and zero errors.
- `unicode-isolated-tests-round1.json`: direct-dotnet JIT executable,
  exit 0, 89 passed / 0 failed / 0 skipped. This includes all 79 base groups
  and the ten authored Unicode groups, with included alias/template/overload/
  constexpr assertions and retained-source/encoding/concurrency checks.

The build and JIT receipts have identical 66-file SHA maps. I independently
compared every listed file against held disk bytes: zero mismatches. The map
explicitly includes all three Unicode data/provenance/license files and all
three copied tools, in addition to source/tests/fixtures. The full handoff SHA
is `f7980104b5386ed16de78a5e871fc2a62a999f78a13941e0b42b6cc4f3b65439`;
all 27 non-self authored fingerprints match disk. All 14 cited upstream source/
documentation SHA values also match the held upstream files. The current native
compiler bytes match the SHA above.

No isolated NativeAOT publish/run/artifact receipt was available at this audit
point. NativeAOT execution, embedded-resource access in that executable, output
fingerprints and final held-map equality therefore remain unverified here.
The JIT pass does not close U1/U2 or establish combined main-reference/storage
compatibility.

Main native preview controls are separately bounded. Gate/header and
native-values inputs match the held fixture bytes and accept with exit 0 under
explicit UTF-8. The same values reject under explicit legacy `none`. The
original timing positive, ordinary Unicode string-value positive and stringized
Unicode value positive actually reject with exit 2; their expected-accept
receipts remain disproved and excluded. The held mixed/disproved macros fixture
is not a positive gate. The original timing snapshot differs from the current
held fixture's explanatory/comment bytes and is not an exact held-fixture pass.
Split dropped/stringized/taken-elif identifier controls reject; outer-inactive
and unused/discarded character-content controls accept. Stringization spelling
and ASCII literal-token value witnesses accept, including the retained 9/14
size witnesses. I verified the preview receipt input hashes and additional-input
hashes against their actual source files.

The copied oracle tool uses exact status 0 for acceptance and exact status 2
for rejection; status 4, other tool failures and signals cannot become negative
passes. It supplies source kind as a distinct option/value pair. Native preview
evidence does not substitute for executing managed behavior on the same inputs.

**Source/data correspondence assessed within the held contribution.**

- The resource is the full pinned `DerivedCoreProperties-17.0.0.txt`, dated
  2025-07-30, SHA
  `24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`.
  The parser selects only exact XID_Start/XID_Continue property rows, validates
  ordered scalar ranges and performs a handwritten lower-bound search. ASCII
  underscore is admitted explicitly; host Unicode categories and normalization
  are absent. I independently compared the non-ASCII declarative sets to the
  UAX44/non-initial UAX44 flags in the actual native table: exactly 145,841
  starts and 149,158 continuations, zero differences in either set. This is
  source-data inspection, not managed routine coverage or a compiler run.
- The full Unicode License V3 is retained separately, SHA
  `e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96`,
  with provenance, notices and version selection. Authored implementation and
  fixtures retain Apache-2.0 WITH LLVM-exception attribution. No generated
  frontend logic, transpilation or native semantic fallback was found in this
  contribution. The fixed manifest-resource name and static immutable range
  owner are suitable for independent sessions by inspection; AOT execution is
  still required.
- Scalar identity and raw spelling are separate. Semantic/parser/template
  substitutions use canonical names while declaration ranges and physical
  origins retain source spelling. Macro names/parameters/defined/undef,
  suppression sets, redefinition comparison and lookup use canonical identity;
  paste retokenizes raw concatenation. The scoped central-file changes do not
  implement reference or overload algorithms and must be merged additively.
- Source policy and encoding metadata are immutable; managed strings carry
  decoded UTF-16 storage provenance without an inferred byte BOM. Structured
  include reads retain original encoding/BOM metadata on actual source owners;
  logical/presentation streams and origin contributors retain their documented
  parent/physical distinctions. Provider probes do not decode or activate
  sources; retained graph queries do not call providers. No new mutable
  session-owned state is stored in the static Unicode tables.
- Character conversion follows the cited scalar-versus-code-unit distinction:
  direct/UCN ordinary and u8 scalars encode to UTF-8 units; strict ordinary
  multibyte and multi-unit prefixed character forms are invalid; supplementary
  UTF-16 needs two units; UTF-32/wchar32 retains a scalar. Numeric escapes may
  retain raw surrogate code units while UCNs cannot name surrogate scalars.
  Existing target packing, signedness, type and PP promotion paths remain.
- The native strict ordinary nonraw string-width diagnostic at
  `literals.c:2240–2277` is not proof of an ASCII execution charset. UTF8/raw
  branches exclude that check. Full managed string values remain Unsupported,
  as do the recorded unknown non-ASCII escape, floating, UDL and legacy-policy
  branches; these must not be credited as implemented string semantics.

**Bounds and continuation.**

U1/U2 prevent isolated acceptance even with the current 89/89 JIT result.
Their repairs/regressions and isolated AOT evidence remain open. Each repair
review needs a new delegated review round carrying this brief,
findings, actual responses and new matching evidence. Subsequent additive
integration with main's scalar references and object-storage constexpr work
needs its own combined source hold, build/JIT/AOT/native gates and independent
review; no main Unicode support is accepted here.

The complete main `docs/FEATURE-INVENTORY.md` remains authoritative: remaining
M1 string/raw/concatenation/encoding, floating/UDL, broader dialect/preprocessor/
header behavior; remaining M2 declarations/expressions/control flow; M3 complete
references/classes/advanced overloads; general M4 templates and constexpr;
M5 language/lifetime/layout/library breadth; and M6 API/resource/recovery/platform/
performance/update/full-coverage qualification all remain required. Unicode data
indexing and this bounded review do not complete any of those larger surfaces.

Independent isolated Unicode review, round 2 — 2026-10-21.
Reviewer: GPT-6.1 Sol, xhigh, independently delegated for this round.

**Decision: accept the repaired isolated Unicode checkpoint within the bounds
below. U1 and U2 are closed. No new critical/high correctness blocker was found
in the reviewed contribution.** This decision covers the actual held isolated
implementation and matching executed evidence. Main's additive integration,
combined reference/storage/Unicode regression gate, full M1, and the remaining
PLAN are outside this acceptance.

I read the main manual-campaign AGENTS.md, PLAN.md, PROGRESS.md, SUPPORT.md,
the feature inventory, applicable parent guidance, prior
review-unicode-isolated-round1.md, the corrected handoff, actual isolated
production/test/fixture/data/tool files, and the upstream EDG routines cited
below. I ran no build, test harness, frontend, native compiler/oracle, Git
operation or subagent. Read-only scripts inspected declarative data and
computed file fingerprints. My only filesystem write is this authorized
main-checkout report. Source references below identify the isolated campaign,
unless explicitly labeled main evidence or upstream.

The checkout is /home/marius/.t3/worktrees/dotcc/t3-96dc97a5, branch
t3/unicode-identifier-frontend. Direct inspection of worktree HEAD/ref storage
confirms base 3b667ce7350337cfff45e2de78d923c42c750274. This is the 79-group
base plus ten Unicode groups, separate from main's subsequent scalar-reference
and object-storage work. The author remains READY/HOLD.

The corrected handoff SHA-256 is
774440b24afaeaaba25db2c6909215435c766f7131119f62913ddc60f31098dc.
All 27 authored non-self fingerprints match disk. Compared with the prior
66-file validation map, exactly six entries changed: MacroExpansion.cs,
Preprocessing.cs, PreprocessingDirectives.cs, Tokens.cs,
UnicodeIdentifiers.cs and UnicodeTests.cs. No entries were removed or added.
The data and three supervisor-copied tools are unchanged.

**U1 — previously High, closed for the repaired active-scan contract.**

The prior loss of malformed UCN diagnostics in dropped pp-numbers and literal
suffixes is repaired in the scanner and preprocessing boundary:

- UnicodeIdentifiers.cs:124–130 adds a PartLength overload returning ReadUcn's
  syntax/scalar problem. This does not confuse UCN validity with identifier
  membership or literal value conversion.
- Tokens.cs:28–34 retains an immutable original ScannedUcnDiagnostic on Token.
  ScanIdentifierPart at 413–427 retains the first problem at its actual source
  range and diagnoses it immediately for direct Lex. The pp-number loop at
  274–294 and quoted/raw suffix scans at 360–411 and 429–435 use that helper.
  Literal contents do not use it.
- PpToken construction/copy and lexical/presentation materialization preserve
  that diagnostic: Preprocessing.cs:110–134, 245–259, 307–330 and
  Tokens.cs:148–165. DiagnoseIdentifier now checks this metadata independently
  of token kind at Preprocessing.cs:226–238.
- ReadActive validates active source tokens before expansion can discard or
  stringize arguments, at PreprocessingDirectives.cs:29–36, 86–88 and 126–127.
  Eligible and first taken-group #elif scanning uses the explicit scanActive
  condition at 69–77. Outer inactive text remains unvalidated. Paste rejects
  a retokenized token carrying a lexical issue at MacroExpansion.cs:293–305.

This follows upstream lexical.c:f_is_identifier_char at 10155ff, whose UCN
call enables diagnostics even with is_identifier FALSE; scan_number at
12730ff and check_for_ud_suffix at 14466ff use that predicate. The fixed-width
malformed/scalar rules and skipped-group suppression are in
scan_universal_character at 13308ff. In contrast, initial quoted literal
contents are scanned with suppressed value diagnostics in accum_quoted_string.
preproc.c:scan_if_expr, proc_elif and skip_to_endif support the active/skip
distinction. These upstream branches were inspected directly.

UnicodeTests.cs:143–178 now checks the three exact prior token spellings
1\uD800, U'a'_\uD800 and "x"_\uD800 under both DROP and STR; malformed digits,
truncation, out-of-range values and raw suffixes; direct Lex; inactive
suppression; lazy invalid character/string/raw contents; and valid _tag
controls. These assertions executed in both fresh 89/89 JIT and AOT runs.
The retained main native receipts
native-unicode-preview-dropped-{ppnumber,character-suffix,string-suffix}-ucn-round1.json
each actually reject with status 2 under explicit UTF-8. Their source/compiler
hashes match. Thus U1 is closed by actual repaired code, exercised managed/AOT
assertions and the previously discriminating native controls.

This closure does not claim exact malformed-token recovery or diagnostic
multiplicity parity, complete UDL semantics, or an exhaustive malformed corpus.
The broader required lexical/recovery branches remain open.

**U2 — previously High, closed for replacement storage and stringization.**

Upstream macro.c:9911–9916 stores ordinary literal identifier replacement
tokens through make_canonical_identifier(force_ucn TRUE). Parameter and
stringized/raw argument sections take separate branches before that storage
path. lexical.c:13739ff and output_ucn_value produce lowercase fixed-width
hexadecimal UCNs. I inspected these branches and macro.c:stringized_arg at
3323–3438 directly.

The repair follows that context distinction:

- UnicodeIdentifiers.cs:133–149 supplies CanonicalUcnSpelling using scalar
  identity and lowercase x4/x8 formatting.
- PreprocessingDirectives.cs:370–373 canonicalizes only literal Identifier
  replacement tokens, excluding formal parameters. Preprocessing.cs:285–287
  applies the same boundary to configured object replacements.
- Preprocessing.cs:114–134 keeps RawSpelling/Range/Origin independent of
  EffectiveSpelling/Text, and copies retain both. MacroDefinition exposes
  effective ReplacementSpelling and RawReplacementSpelling separately at
  83–90.
- MacroExpansion.cs:178–211 substitutes original argument tokens and their
  origins. Stringize at 229–245 uses effective text and doubles backslashes
  and quotes only for literal tokens. Paste at 293–305 combines effective
  text, retokenizes it and retains the original contributors.

UnicodeTests.cs:120–141 asserts the eight direct/UCN/replacement/ID contexts
plus nested EE(E(A)), original A/B replacement ranges, direct-argument range,
nested expansion history, Unicode formal parameters, literal function
replacement, equivalent redefinition and supplementary replacement spelling.
The existing identity/paste controls at 95–119 remain. The repaired assertions
executed in the fresh JIT and AOT runs.

I re-read the actual main native inputs, commands and output in
native-unicode-preview-{replacement-stringize,stringize-contexts,replacement-stringize-values}-round1.json.
Direct S(é)/E(é) preserve é; direct uppercase \u00E9 stays uppercase; E(A)/E(B)
produce lowercase \u00e9; S(ID(é)) preserves ID(é); E(ID(é)) preserves é.
Nested EE(E(A)) has the escaped canonical ASCII spelling. The independent
native value witness accepts sizeof(EE(E(A)))==9 and discriminating quote,
backslash, u, e and 9 data checks. All three receipts actually accept with
status 0 and match source/compiler hashes. This closes the observable
replacement-spelling mismatch; no blanket canonicalization of direct
stringized arguments was introduced. Full managed string values remain
Unsupported and are not inferred from these preprocessing assertions.

**Other original review dimensions remain sound within these tested bounds.**

- UnicodeIdentifiers.cs:14–59 loads the fixed embedded resource once, checks
  its version and ordered scalar intervals, selects exact XID_Start and
  XID_Continue rows, and uses a handwritten lower-bound search. Resource and
  reader have using scopes; the static owner retains only private immutable
  range arrays, with no session/source/symbol graph. This is a bounded trusted
  embedded data source, not a general external UCD loader. AOT's executed
  boundary/semantic groups exercise actual manifest-resource access.
- The unmodified Unicode17 resource is 1,134,783 bytes, SHA-256
  24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08.
  Unicode-3.0 license SHA-256 is
  e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96.
  PROVENANCE.md retains official URLs, dates, notices and property selection;
  implementation/fixtures retain Apache-2.0 WITH LLVM-exception attribution.
  I independently compared declarative non-ASCII sets with the actual
  lexical.c UAX44 flags: 145,841 starts and 149,158 continuations, zero
  differences in either direction. This was data inspection, not test or
  native execution. No generated C# frontend logic or native semantic fallback
  was found.
- Canonical scalar identity without normalization reaches macro names,
  parameters, suppression, defined/undef/redefinition and parser/type/name
  lookup. Parser.cs:51/119/131/212/279–304,
  Expressions.cs:100–104, Templates.cs:164/199–201 and
  SemanticGraph.cs:60 use canonical names. Physical declaration ranges and
  origin contributors remain separate. These substitutions do not implement
  or qualify main's later reference/storage algorithms.
- Source.cs:12–66 and CompilationSession.cs:55–93 retain immutable configured
  policy, validate scalar storage and explicitly diagnose unsupported
  legacy/native source or execution policies. Managed strings retain
  ManagedUtf16 provenance without an inferred byte BOM. Source encoding and
  target character widths are distinct.
- FileSystemIncludes.cs:310–337 strictly decodes selected UTF-8/BOM UTF-16 and
  attaches original encoding/BOM metadata; buffers/handles keep their existing
  disposal scopes. PreprocessingDirectives.cs:184–249 retains per-analysis
  structured read/probe/miss caches, typed failures, actual physical owners
  and separate include activations. Probes do not decode or activate sources.
  Retained graph queries do not call providers. The executed metadata,
  filesystem and independent-session groups cover these bounded contracts.
- CharacterLiterals.cs:49–148 follows scalar conversion versus numeric code
  units: ordinary/u8 scalars form UTF-8 units, strict ordinary multibyte and
  multi-unit prefixed characters fail, UTF-16 supplementary scalars need two
  units, and UTF-32/wchar32 retain scalar values. Numeric surrogate code units
  remain possible while surrogate UCNs fail. Upstream literals.c:
  conv_unicode_literal_char/conv_single_char/conv_single_wide_char/
  conv_char_literal and host_envir.c:ucn_to_utf16 distinguish these cases.
  Existing target signed packing, type selection and PP promotion are retained.
- The included semantic test at UnicodeTests.cs:210–237 checks canonical
  alias/function identity, two actual char16_t/char32_t template instances,
  selected promotion overload, four successful constant-required assertions,
  header/macro origins and retained character units. Independent session
  tests at 318–333 check separate symbols/types/instances. Existing repeated
  template collection tests also pass in both runs. Broader M6 retention,
  cancellation and adversarial resource qualification remain required.

All 14 upstream source/document fingerprints in the handoff match the actual
held upstream files. In particular lexical.c is
e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875,
and macro.c is
3cd96e32ba6676fb66b17900922abdc9a33caab5aade78f2046dba0490d996a3.
Input BOM/conversion and exact option syntax were checked against host_envir.c,
cmd_line.c:12070ff, lex_pp.rst:709ff and ext_intf.rst:436ff.

**Executed evidence and identity audit.**

The following receipts are in the isolated campaign's evidence directory:

- unicode-isolated-build-round2.json: Release solution build, exit 0,
  zero warnings/errors.
- unicode-isolated-tests-round2.json: direct-dotnet JIT executable, exit 0,
  89 passed, 0 failed, 0 skipped.
- unicode-isolated-nativeaot-publish-round2.json: Linux x64 NativeAOT publish,
  exit 0.
- unicode-isolated-nativeaot-tests-round2.json: published native executable,
  exit 0, 89 passed, 0 failed, 0 skipped.

All four source maps are identical, contain 66 entries, and independently
match current held disk bytes. The entries include all three Unicode
data/license/provenance files and all three copied tools. The existing ten
Unicode registrations were expanded, with no extra registration or skipped
group. The held-snapshot receipt's map also matches. Its SHA-256 is
854b3ad7684541ca36c611990d73e8983f2a6b794dbc1b7faa42fdd922bc2d68.

I independently checked all three artifacts recorded by
unicode-isolated-held-snapshot-round2.json:

- src/EdgSharp/bin/Release/net10.0/EdgSharp.dll: 1,366,528 bytes;
  SHA-256 0492730a43b490510cbd3c2c38188b28a37124ae39288861133507b0290f6e0d.
- tests/EdgSharp.Tests/bin/Release/net10.0/EdgSharp.Tests.dll: 190,976 bytes;
  SHA-256 468f27d67b6096e9d751e97725f2a2f42867d03e60e0aaf49459f43ac0b2c6cf.
- tests/EdgSharp.Tests/bin/aot-unicode-isolated/EdgSharp.Tests:
  5,333,656 bytes;
  SHA-256 479c0ff7a8756895f4e83916b6e9ab5a5e89ff0c59d4739194d0255f58d24115.

Fresh exact isolated native controls are
native-unicode-isolated-gate-round2.json (gate plus explicitly hashed header,
accept 0), native-unicode-isolated-native-values-round2.json (accept 0), and
native-unicode-isolated-native-timing-round3.json (reject 2). Source/header
hashes match the held fixtures. All use strict C++20, --instantiate used and
the two distinct arguments --unicode_source_kind UTF-8. Compiler bytes match
SHA-256 e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3.
The copied oracle recognizes only exact language statuses 0/2; tool failures,
catastrophes and signals do not satisfy a negative.

native-unicode-isolated-native-timing-round2.json mistakenly requested accept
for the already documented negative timing fixture. It actually returned 2,
matched false, and remains retained/excluded. Round3 records the correct
reject expectation for the same bytes. The held-snapshot explicitly records
this invocation-expectation error; it is not an implementation repair or pass
under the wrong expectation.

Historical disproved timing/string-value positives and the mixed native
macros proposal remain excluded. I rechecked their actual receipt outcomes
and hashes. The native strict ordinary nonraw string-width branch at
literals.c:2240–2277 does not establish ASCII/legacy execution encoding:
conv_unicode_literal_char still converts Unicode source to UTF-8. Raw and
UTF8-prefixed string branches differ. Native ASCII nested-stringization data
witnesses do not qualify full managed string conversion.

**Acceptance boundary and continuation.**

U1/U2 no longer block the isolated checkpoint. This report authorizes no
main merged acceptance: integration against main's reference/storage commit
must preserve additive identity hunks and receive a new combined held-map
Release/JIT/AOT/native regression and independent produced-code review.
Main changes were neither inspected nor accepted by this round.

Full strings/raw/prefixed/adjacent conversion and concatenation, floating/UDL
semantics, legacy/native encodings, older/different dialect identifier gates,
named/delimited escapes and security diagnostics, broader recovery/resources,
target/platform breadth and every other open PLAN/FEATURE-INVENTORY branch
remain required. Diagnosing these forms Unsupported does not complete them.
This bounded acceptance is a continuation checkpoint, not campaign completion.

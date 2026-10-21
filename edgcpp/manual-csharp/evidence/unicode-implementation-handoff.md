# Unicode identifier/source/character implementation handoff

Addressed to `/root/manual_edg_supervisor`. This is one bounded implementation
contribution to the continuing manual frontend campaign, not campaign completion.

Worktree: `/home/marius/.t3/worktrees/dotcc/t3-96dc97a5`.
Branch: `t3/unicode-identifier-frontend`.
Verified starting HEAD: `3b667ce7350337cfff45e2de78d923c42c750274`.
The worker did not stage, commit, modify Git, run a build/test/compiler/native
oracle, spawn children, edit the primary checkout, or edit campaign ledgers.
Implementation and tests are handwritten. No generated EDG frontend input,
translator, native semantic fallback, or fixture-dependent frontend path exists.

## Implemented interfaces and bounded behavior

- `Token.IdentifierText` and internal `PpToken.IdentifierText` are immutable
  scalar identities for identifiers/keywords; all other token kinds return raw
  spelling. Direct characters, four-digit UCNs and eight-digit UCNs share one
  identity. `Spelling`, ranges, physical contributors and expansion origins stay
  separate. No NFC/NFKC composition/decomposition or host Unicode classification
  is performed. Invalid identifiers are diagnosed and retain their raw spelling
  in partial/error graphs.
- `UnicodeIdentifiers.cs` contains the handwritten scalar/UCN scanner,
  Unicode17 range parser, binary search and UTF-8 scalar encoding. It loads the
  pinned declarative resource by its fixed embedded resource name. It enforces
  four/eight hex digits, scalar/surrogate/range checks, basic-character identifier
  restrictions and distinct XID start/continuation membership. Physical offsets
  remain UTF-16 code units. Numeric pp-token and literal-suffix collection now
  recognize scalar/UCN identifier parts without rewriting literal contents.
- Preprocessing uses canonical identity for macro names, unavailable-name sets,
  parameters, `defined`, ifdef/ifndef/undef, replacement parameter comparison,
  configured names, equivalent redefinitions and rescanning. Paste concatenates
  effective token spelling, actually retokenizes it, and checks the resulting
  identifier. Stringization preserves effective identifier UCN backslashes;
  only literal tokens have quotes/backslashes escaped, following `stringized_arg`.
  A literal identifier in macro replacement text has native force-UCN effective
  spelling (lower hex, four/eight digits). `PpToken.RawSpelling`/`Range` and
  original origins remain separate from `EffectiveSpelling`/`Text`.
  `MacroDefinition.RawReplacementSpelling` retains original token spellings;
  `ReplacementSpelling` exposes effective replacement text. Direct arguments and
  substituted parameter arguments are not globally canonicalized into UCN text.
- Initial active source-line identifier scanning diagnoses invalid UCNs even in
  dropped/stringized arguments. Skipped conditional groups suppress identifier
  diagnostics. The first `#elif` leaving a taken active group scans identifiers
  with diagnostics enabled before entering the skip; eligible elif expressions
  also validate identifiers. Character values remain lazy: unused definitions
  and dropped character arguments are not decoded. Further dialect/recovery
  corners still require differential qualification. The released timing fix
  additionally retains lexical UCN syntax/scalar diagnostics from pp-number
  parts and actual literal suffixes in immutable internal
  `Token.ScannedUcnDiagnostic`/`PpToken.ScannedUcnDiagnostic`. Existing active/skip
  boundaries report those diagnostics even for dropped/stringized arguments.
  Literal contents do not enter this scanner path and retain lazy conversion.
- Parser, expression, template and public scope lookup use canonical name
  identity. Declaration `NameRange` still retains raw spelling. No declarator,
  type-owner, reference, value-category, conversion-ranking or overload-selection
  algorithm was changed. `Types.cs` and `OverloadResolution.cs` are untouched.
- `SourceInputOptions` is sealed with get-only `UnicodePolicy` and
  `ExecutionEncoding`. It is retained by the get-only
  `PreprocessingOptions.SourceInput`; session timestamp capture preserves the
  same immutable policy object. Defaults explicitly mean decoded managed
  Unicode source and UTF-8 ordinary-character execution encoding.
- `SourceEncodingInfo` is sealed with get-only `OriginalEncoding`,
  `HasByteOrderMark`, `UnicodePolicy`, and `ExecutionEncoding`. `SourceFile.Encoding`
  retains it; logical/presentation buffers preserve their parent policy and
  origins retain each actual physical source. Root/legacy-provider .NET strings
  are decoded Unicode in UTF-16 storage, with `ManagedUtf16` provenance and no
  inferred byte encoding/BOM. Managed text is not BOM-stripped. Unpaired UTF-16
  surrogates anywhere in an activated source produce `SRC001` and stop its scan.
- `IncludeReadResult.Encoding` is non-null only on Found. The optional third
  argument of `Found(path,text,encoding)` lets a structured provider supply
  original encoding/BOM provenance. Its compatibility default explicitly means
  managed Unicode text. The filesystem provider reports UTF-8/no BOM, UTF-8/BOM,
  or BOM-selected UTF-16 LE/BE after strict decoding. Its Probe, glibc ABI
  qualification, descriptor/open/stat/fstat behavior, allowed-root policy and
  read/probe caches are unchanged. Probe does not decode or allocate sources;
  graph queries do not call providers.
- Legacy non-Unicode/native multibyte input or legacy execution policy is
  explicitly `UNS030`, without host fallback. UTF-32/UTF-7 filesystem input
  remains Unsupported. `UnicodeSourcePolicy`/`ExecutionCharacterEncoding`
  describe these unimplemented modes without claiming implementation.
- The shared character decoder now decodes direct scalars and literal UCNs,
  independently of identifier restrictions, then encodes target units. Ordinary
  and u8 character scalars use UTF-8 bytes; strict ordinary multibyte entities
  and u8 literals exceeding one byte are invalid. A UTF-16 supplementary scalar
  produces two units and is invalid in one u character literal. UTF-32 and the
  configured LP64 32-bit wchar retain the scalar. Numeric escapes remain target
  code units: u'\\xd800' is distinct from invalid surrogate UCN u'\\ud800'.
  Existing warning, numeric-range, signed first-entity packing, type selection,
  PP promotions and target-width behavior remain in place.

## Source correspondence and exact consulted bytes

The actual imported working files, rather than the old import revision alone,
are the reference. This addition remains Apache-2.0 WITH LLVM-exception;
Unicode data has its separate retained Unicode-3.0 license.

- `lexical.c`: `is_valid_UCN_identifier_char` 13157ff (modern UAX44/XID choice,
  lower-bound interval membership and non-initial restriction),
  `check_for_invalid_cplusplus_ucn` 13210ff, `scan_universal_character` 13308ff
  (fixed-width digits/scalars and skipped-context suppression),
  `make_canonical_identifier` 13739ff (scalar identity, not normalization),
  `f_is_identifier_char` 10155ff (notably scan_universal_character with
  issue_diagnostics TRUE at10196), `scan_number` pp-number scan12736 and
  `check_for_ud_suffix`14481, `is_nonstandard_character`10387ff,
  identifier scan in `get_token` 18250ff, and input push/pop 8077/8189/8590.
- `macro.c`: `stringized_arg` 3323–3438 (escape only within literal tokens),
  canonical macro comparison/lookup5623ff and `proc_define` ordinary-token
  replacement storage9911–9916. Parameter substitution sections are handled
  separately before process_ordinary_token. The initial adaptation that used
  original raw literal-identifier spelling as effective replacement text was
  observably wrong under nested stringization and is superseded: effective
  replacement identifiers now use make_canonical_identifier(force_ucn TRUE),
  while original raw spelling/ranges/origins remain retained independently.
- `preproc.c`: `scan_if_expr` 463–494, `proc_elif` 539ff and
  `skip_to_endif` 705–815 explain active versus skipped diagnostic timing.
- `literals.c`: `conv_unicode_literal_char` 1144ff,
  `conv_single_char` 1238ff, `conv_single_wide_char` 1743ff and
  `conv_char_literal` 1820–2172: scalar conversion versus numeric code units,
  strict character unit/count errors and first-entity signed packing.
- `host_envir.c`: `do_check_for_byte_order_mark` 1443ff,
  `clear_getc_source_state` 4856ff, `getc_utf16` 4869ff and `ucn_to_utf16`
  5181–5217. The last explicitly allows numeric surrogate code units while UCN
  scanning rejects surrogate scalars. Managed providers decode once, retain
  provenance, and never use a native character-conversion implementation.
- `cmd_line.c` 12070–12086 and `ext_intf.rst` 436–444 define the exact
  case-sensitive Unicode input option; `lex_pp.rst` 709–742 separates input
  Unicode encodings, canonical identifier representation and native none/Latin1.

Consulted SHA-256 values:

```
e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875  upstream/src/lexical.c
de5276f87f073a67bb4b7c6a4cd152a66eab0a155fda84f0d991e3d296213d95  upstream/src/literals.c
7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c  upstream/src/host_envir.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  upstream/src/host_envir.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  upstream/src/targ_def.h
77ce3949dffb0fd85a8bca85a33ae590fc2c5b7b7f200b47ff672c35cca8bded  upstream/src/cmd_line.c
3cd96e32ba6676fb66b17900922abdc9a33caab5aade78f2046dba0490d996a3  upstream/src/macro.c
e0927cba0b5ba139152d69063a6ec73114e103be957effd90eec4616438a65af  upstream/src/preproc.c
805ade6649e8b2e0d2d11c2eaf8364904e51d54bbaa3701d2011f8609e74e8b3  upstream/src/lexical.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  upstream/src/defines.h
64f9c35401c9ab612443d85edd74574881d074a9036cb863cf7fe12c7a3b1948  upstream/src/defines_linux.h
2a138f937994d38e4ec4bed88ceffa7817199752e33aa53a0a58a05153fbb798  upstream/src/error_msg.txt
fdeea9d1bba1c5f3f89d5fde9596070aa0d5c9aa22927a62756049e29eceb06b  upstream/doc/source/lex_pp.rst
1617b34a82eb53c671ed2b894df8cb793a641c6c75abebc056338d727f104d98  upstream/doc/source/ext_intf.rst
```

## Pinned data

Official unmodified
<https://www.unicode.org/Public/17.0.0/ucd/DerivedCoreProperties.txt>,
version17.0.0 dated 2025-07-30, SHA-256
`24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`.
Retained Unicode License V3 from <https://www.unicode.org/license.txt>, SHA-256
`e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96`.
Selection, exclusions, dates and attribution are in
`data/unicode/17.0.0/PROVENANCE.md` and `LICENSE.txt`.
A read-only declarative comparison found exact non-ASCII equality with native
UAX44 flags: 145,841 XID_Start and 149,158 XID_Continue scalars, zero differences.
No EDG C algorithm or logic was extracted/generated as C#.

## Authored acceptance and native observations

Ten groups are registered in Program after CharacterPreprocessing, all in
`UnicodeTests.cs`: `UnicodeIdentifierTokens`, `Unicode17Boundaries`,
`UnicodeIdentifierDiagnostics`, `UnicodeMacroIdentity`,
`UnicodePreprocessingTiming`, `UnicodeCharacterSemantics`,
`UnicodeSemanticGate`, `UnicodeSourceMetadata`, `UnicodeFileEncodingMetadata`,
and `UnicodeIndependentSessions`. These exercise raw/canonical identity,
supplementary UTF-16 offsets and splice contributors; exact Unicode17
start/continuation/exclusion boundaries; malformed/scalar/basic restrictions;
macro identity/inertness/params/defined/redefinition/stringize/paste; timing;
actual target character type/code-unit/value/count errors; an included
alias/template/overload/constexpr/provenance gate; per-source BOM metadata,
cache/no-query-I/O contracts and independent sessions. They are AUTHORED,
NOT EXECUTED by this worker. Supervisor initially built the first held bytes
in Release with zero warnings/errors and ran89/89 JIT groups, recorded in
`unicode-isolated-{build,tests}-round1.json` (66-file source map). The two released
fixes below change those bytes and require fresh supervisor execution and final
independent review; the earlier89/89 does not qualify the corrected snapshot.
NativeAOT execution remains supervisor work.

Native must use TWO arguments `--unicode_source_kind UTF-8` in addition to the
pinned strict C++20 LP64 profile. No oracle tool was changed here.

Supervisor relayed these actual native observations from retained main-checkout
snapshots/receipts, not this worker's execution:

- `m1-unicode-gate.cpp/.h`: accept0; `m1-unicode-native-values.cpp`: accept0,
  including distinct prefix types, UCN/direct scalar values, numeric surrogate
  code units and U'\\xffffffff'. These support the precise character policy,
  not a managed implementation pass.
- Original combined timing positive was DISPROVED (exit2): active
  DROP(\\uD800), STR(\\uD800) and first taken #elif identifier UCN diagnosed.
  Outer #if0 identifiers, unused malformed character definition and dropped
  malformed character literal were not diagnosed. Tests now reflect the split.
  Original snapshot and `native-unicode-preview-native-timing-round1.json`
  remain excluded from positive pass counts in main evidence.
- Stringization SPELLING accepts: identifier UCN backslashes stay single;
  literal-token backslashes and quotes are doubled. Native ASCII literal-token
  stringization VALUE witness accepts (sizes9/14, escapes retained as data).
- Proposed ordinary Unicode string VALUE accepts, both direct and stringized,
  were DISPROVED (exit2). This is explained by actual
  `literals.c:conv_string_literal_chars` 2240ff:
  `consider_char_width=(ORDINARY && !raw && strict_ansi_mode)`;
  a multibyte entity (`remaining_char_count!=0`) registers
  `ec_char_too_wide_for_rep` at 2270–2277. `conv_unicode_literal_char` still
  creates UTF-8 bytes for Unicode source. This is a strict representability
  branch, not proof of ASCII/legacy execution encoding. Host configuration
  branches in `host_envir.h` 2654–2751 separately control native multibyte
  conversion and BOM-less input defaults; Microsoft/native source conversion
  in `literals.c` 2430ff is another branch. Ordinary raw and UTF8-prefixed
  strings exclude that strict width check; full managed string semantics,
  including these branches, remain Unsupported and unaccepted.
- `m1-unicode-native-macros.cpp` retains disputed ordinary-string value
  assumptions visibly as a mixed/disproved proposal, never a positive gate.
  Main receipts `native-unicode-preview-{stringize-spelling,stringize-values,
  ordinary-string-values,stringize-ascii-values}-round1.json` retain the split.

## Exact integration/conflict guidance

All new module/data/fixture/test files are additive. Central files use bounded
hunks, never whole replacements. Merge against base
`3b667ce7350337cfff45e2de78d923c42c750274`, preserving the supervisor's newly
integrated references:

- `Parser.cs`: only identity substitutions in StartsDeclaration, EnterAlias,
  NewLocalSymbol, ScanType alias lookup, and FunctionDeclaration's name/main/
  binding/new-symbol sites (eight changed lines). Preserve new reference
  declarators, type-owner guards, binding types and every reference algorithm.
- `Expressions.cs`: only VisibleSymbols and UnresolvedFunctionExpression name
  arguments (two lines). Preserve ReferenceType/ValueCategory.XValue semantics.
- `SemanticGraph.cs`: Scope.FindLocalBinding canonicalizes query identity and
  Symbol.Name gains a canonical-name doc comment. Preserve XValue enum additions
  and all reference graph changes.
- `Templates.cs` (explicit ownership expansion): exactly three name identity
  substitutions: TemplateParameterType name; own parameter shadow check; main
  name check. Preserve all ErrorCount, replay, ownership and instantiation logic.
- `Program.cs`: ten additional registrations. Preserve reference groups and all
  other registration/command behavior.
- `CharacterTests.cs` (expanded ownership): only obsolete Unicode-unsupported
  assertions: literal UCN0041 now value65 positive; direct ordinary é now strict
  LEX020 invalid. UDL and unknown nonASCII escape Unsupported assertions remain;
  all warning/packing/target/FS controls are retained.
- `PreprocessingTests.cs` (expanded ownership): only #if UCN0041 Unsupported
  becomes #if UCN0041==65 positive. __has_cpp_attribute remains Unsupported.
- `Source.cs`, `CompilationSession.cs`, `Preprocessing.cs`, `Tokens.cs` add the
  immutable source policy/metadata and identifier identity/scan integration.
  `PreprocessingDirectives.cs`, `PreprocessingExpressions.cs`, `MacroExpansion.cs`
  change identity, active scan timing, stringize/paste and include metadata only.
  `CharacterLiterals.cs` adds scalar conversion before existing packing.
  `FileSystemIncludes.cs` adds Found metadata plus decode selection provenance
  only, with no host-adapter behavioral changes.

No shared PLAN/PROGRESS/SUPPORT/AGENTS/correspondence file, upstream source,
older campaign, Types.cs, OverloadResolution.cs, or oracle tool was edited by
this worker. Supervisor-copied tools/receipts now present in the isolated tree
are outside worker ownership and were preserved without edits.

## Narrow release: both independent native-confirmed findings

The original terminal HOLD was explicitly released by the supervisor for these
two findings only. Revised files are `Tokens.cs`, `UnicodeIdentifiers.cs`,
`Preprocessing.cs`, `PreprocessingDirectives.cs`, `MacroExpansion.cs`,
`UnicodeTests.cs`, and this handoff. No data, source/execution policy, character
decoder, parser/reference/type algorithm, registration, supervisor tool or
supervisor receipt changed in this release.

1. Active pp-number / suffix UCN timing: initial PartLength discarded ReadUcn
   diagnostics, while active validation only analyzed Identifier tokens. Native
   explicitUTF8 controls DROP(1\\uD800), DROP(U'a'_\\uD800), and
   DROP("x"_\\uD800) all reject2. The retained main inputs are
   `tools/oracle-inputs/m1-unicode-preview/dropped-{ppnumber,character-suffix,string-suffix}-ucn.cpp`
   with their supervisor native receipts. The handwritten PartLength overload
   now returns its lexical problem. Lexer.ScanIdentifierPart retains the first
   syntax/scalar issue with its actual source range while collecting pp-number
   parts or suffixes; direct Lex diagnoses it immediately, preprocessing
   reports it at the existing active boundary. Token/PpToken copies and
   presentation materialization preserve this immutable diagnostic. Paste
   rejects a retokenized lexical issue. No literal-content scan was added and
   no UDL value semantics were enabled. Membership/tokenization rules otherwise
   remain the existing bounded implementation; this is lexical-error timing.
   The existing UnicodePreprocessingTiming group adds the three exact native
   negatives, DROP/STR variants, malformed digits/truncation/out-of-range/raw
   suffix proposals, direct Lex controls, inactive-group suppression and lazy
   character/string/raw-content controls. Additional controls are authored,
   not separately claimed native observations or executed managed passes.
2. Replacement spelling: native stringize-contexts.cpp/replacement-stringize.cpp
   show S(é)/E(é) retain direct é; S(\\u00E9)/E(\\u00E9) retain original UCN
   case; E(A)/E(B) yield lower canonical UCN when A/B contain literal replacement
   identifiers é/\\u00E9. S(ID(é)) yields raw ID(é), E(ID(é)) yields direct é,
   and EE(E(A)) produces escaped ASCII data preserving the canonical UCN.
   CanonicalUcnSpelling now implements the actual force-UCN representation
   boundary: literal Identifier replacement tokens are encoded, parameter
   substitution sections are excluded, and direct/parameter actuals retain
   their effective spelling. Configured object replacement identifiers use
   the same ordinary-token branch. Canonical identity and equivalent macro
   redefinition comparison remain separate. The existing UnicodeMacroIdentity
   group adds all nine native contexts, original A/B/direct-argument contributor
   assertions, nested expansion history, Unicode formal-parameter preservation,
   literal function replacement, equivalent redefinition and supplementary
   canonical replacement controls. These new assertions remain unexecuted by
   the worker; final supervisor validation/review is required.

The consulted lexical.c/macro.c SHA-256 values above are unchanged. The new
algorithm correspondence is exactly the f_is_identifier_char caller paths and
proc_define ordinary replacement storage cited above. The source/character
and strict ordinary-string distinctions remain as previously documented.

## Remaining required work

Every other required PLAN/FEATURE-INVENTORY branch stays open. This contribution
does not complete M1: full string/raw concatenation/conversion/type semantics,
floating/UDL, native/legacy encodings and broader configured dialect/recovery
branches remain required. Older identifier tables/dialect gates, named and
delimited escape support, security/confusable diagnostics and broader target
encoding models are not implemented here. Corrected Unicode semantic and AOT execution,
broader adversarial differentials and independent review are still required.
All remaining M2/M3/general M4/M5/M6 work continues under the supervisor.

## READY / HOLD

READY for supervisor three-way integration, builds/tests/oracle qualification
and review. HOLD after this handoff: all worker file edits stop until explicit
supervisor release. No campaign-complete claim is made.

## Authored file fingerprints

The following inventory covers every changed/new implementation, test, fixture
and data file in this isolated worktree. This handoff file itself is additionally
new; its final SHA-256 is delivered with the final handoff message (self-hashing
inside its own contents would not be well-defined).

```
24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08  data/unicode/17.0.0/DerivedCoreProperties.txt
e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96  data/unicode/17.0.0/LICENSE.txt
06d0be9f48a18d798d6a095b6e90c1990c151104cb6a56b6c6ce7621179b386c  data/unicode/17.0.0/PROVENANCE.md
235ffa1c9012b39b854a123f11689db9101e05db916d734e325dd12b50b55ff8  fixtures/m1-unicode-gate.cpp
644c35f3e7745be2911777dfe3f17d9235b4759009d77e2d6bfcb63ba35cce0b  fixtures/m1-unicode-gate.h
453fc77a472030a7e2db6181f01472c2bd0f0887190ffd180b806d009d42c856  fixtures/m1-unicode-native-macros.cpp
c0dc9c8a08d1b6b0c72335b91ce434be6fbd16a880626ddf3fa1a849919c5c0a  fixtures/m1-unicode-native-timing.cpp
0e75dd098baf861d1bd59d42a84a41c4451e46535777dadb63ef8474305146b2  fixtures/m1-unicode-native-values.cpp
407afcf36e865521089a1866c4bcfc9e7c0c8e494ad09c4422e77d4e1f1a2508  src/EdgSharp/CharacterLiterals.cs
bb110d0510d58202821350c3f46a1451d5c9af5e1a8cb87b86c93cdceba3cde7  src/EdgSharp/CompilationSession.cs
96248bd3cb328520880f1c604709c26c4e73d4a982e6eb5e9702b456fe02761d  src/EdgSharp/EdgSharp.csproj
8d869e81cf78598c7ba5f3bc1f7ac0e3808efb229a4bb130736e198713d563a5  src/EdgSharp/Expressions.cs
a13e11df95522fd55502b3483d540b1e02f914a4d88b6ab5482cbc57b68e3228  src/EdgSharp/FileSystemIncludes.cs
b375185b6b66c11b97247c0e978dfad91f78a3d9bdfde540317f2632763c2a7d  src/EdgSharp/MacroExpansion.cs
3864a481ad724056414b0e47c3538b2ceb342167b2959f83b6c5f95a1a4f35bb  src/EdgSharp/Parser.cs
1e29078e5d38f1013c652d3fb6d0ea2bb47e52a72734db992b9ee3cb1c9571f4  src/EdgSharp/Preprocessing.cs
1ac3fd88eac2536e3932f84f7e25140c8b9fe662f031d6cb5e7274eb19df9d37  src/EdgSharp/PreprocessingDirectives.cs
fbd9a64046a65b8223129ae0645ea33104ebd4418e99f0dbf535bf0595a48536  src/EdgSharp/PreprocessingExpressions.cs
faec977a36cda28f3b779999310010d4686f4c2840f29b9a9823d21a8ea722c5  src/EdgSharp/SemanticGraph.cs
ee9e7e69d7150000ef971eeb7b66826f2a1095a61fb707ad280ad4427617b2bc  src/EdgSharp/Source.cs
5658a55069c9ed47c3360d1b65f5324fc888c2a0d2b00bfdd5debea42dfc5b66  src/EdgSharp/Templates.cs
c9499f8ff4e84db884dafe5b6b189d12b81fc953e677fdb994bfffd0cbfff1e1  src/EdgSharp/Tokens.cs
5efa6c24dcb94cf14db83d85d91da6f8d832d81290b5627969ce50ef4a76b84b  src/EdgSharp/UnicodeIdentifiers.cs
706278ed64d769fad18fb7d234c238d23ab6e4d2b524b13e1d0df79ff5ae13ac  tests/EdgSharp.Tests/CharacterTests.cs
a2a2ceaa45ba86f3c578c383d35d475441dd5717d4a007d1bd6cb04c3c908190  tests/EdgSharp.Tests/PreprocessingTests.cs
876d9f4d2330d41d4a6f03c5b80707ec89e0ca55e8dcaaa2aa8fd4eb3259e345  tests/EdgSharp.Tests/Program.cs
af35bddf1775b632639f11c15ec945b61a6068a783680d8955a62c14ec6e8f56  tests/EdgSharp.Tests/UnicodeTests.cs
```

Owned inventory: 17 modified tracked files and11 new owned files, including
this handoff; 27 non-self file hashes above. Supervisor-copied tools and
supervisor receipts are intentionally excluded from this worker inventory.
The released delta against the supervisor round1 66-file map is exactly the six
source/test files listed in the narrow-release section; this handoff is outside
that map. Prior map entries for all other source/test/data/fixture/tool files
remain unchanged. Read-only git diff --check produced no whitespace errors.
HEAD/branch remain the verified base and branch. No build/test/compiler/oracle
or Git mutation was run by this worker. The corrected snapshot is unexecuted.

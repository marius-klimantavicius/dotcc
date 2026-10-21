# Unicode source and identifier integration

The main integrated gate passes112/112 groups under JIT and Linux-x64 NativeAOT,
zero failures/skips and a warning-free Release build. Four receipts share111
current source/test/fixture/data/tool hashes; three actual artifacts are recorded
in unicode-integrated-held-snapshot.json. Six exact native fixtures match four
accepts0 and two language rejections2, including the coupled reference/storage
type witness and forbidden-read complement. Independent final integration acceptance is recorded in
`../evidence/review-unicode-integration-final.md`. Isolated repaired implementation separately
passes89/89 with66-file maps and bounded round2 acceptance; U1/U2 are closed.

| EDG branch | Handwritten representation and preserved behavior |
| --- | --- |
| lexical.c UAX44/XID classification, scan_universal_character, canonical identifier | Scalar/UCN scanning over pinned official Unicode17 XID data; canonical scalar name identity separate from physical spelling; no CLR category tables or NFC rewriting |
| lexical.c scan_number/check_for_ud_suffix | Immutable token diagnostics survive dropped/stringized active arguments, ppnumbers and suffixes; inactive source follows explicit preprocessing timing |
| macro.c stringized_arg and proc_define force_ucn | Escape quotes/backslashes within literal tokens; replacement literal identifiers use lowercase UCN effective spelling while direct/parameter argument spelling stays raw; physical origins remain immutable |
| literals.c Unicode conversion and strict count checks | Ordinary/u8 UTF8 units, u UTF16 units and U/wchar32 scalar values with prefix/count/range rules; numeric escapes denote code units while UCNs must denote scalars |
| host_envir.c BOM/source interpretation | Original byte encoding/BOM metadata is separate from configured decoded-Unicode and ordinary-character execution policy; retained queries perform no provider I/O |

Official DerivedCoreProperties17.0.0 is unmodified declarative data with retained
Unicode License V3 and provenance. Its SHA is
24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08.
Handwritten resource loading validates version/order/ranges and uses immutable
binary searches. Independent declarative comparison found exact equality with
the pinned EDG nonASCII UAX44 start/continue flags:145841/149158 scalars, zero
differences. This is classification/provenance evidence, not whole-file coverage.

Managed strings retain UTF16 physical offsets and have no byte BOM. Structured
file reads retain strictUTF8 or BOM-marked UTF16 metadata; includes preserve each
physical source's encoding. Unicode source and UTF8 ordinary-character execution
are explicit defaults; legacy/native multibyte policies diagnoseUnsupported.
Source encoding does not imply complete string-value support. In the pinned
strict ordinary nonraw string branch, literals.c rejects multibyte entities for
width; raw/UTF8-prefixed branches differ and remain required managed work.

Native Unicode receipts explicitly use TWO arguments `--unicode_source_kind UTF-8`
plus strict C++20 LP64 and used instantiation. Exact malformed active identifier,
ppnumber and suffix controls reject2 even when a macro drops the argument;
inactive groups and unused/dropped character contents have separate lazy checks.
Nested replacement stringization has an ASCII sizeof/index witness, so raw
provenance cannot substitute for observable effective spelling. Historical
disproved positive assumptions and mistaken native expectations remain retained
and excluded. Full strings/floating/UDL, delimited/named UCN profiles, legacy
encodings and complete dialect/header compatibility remain required.

The integrated gate must additionally preserve accepted scalar references,
scoped signature ownership, token replay, frozen overload identity, real constant
storage/logical work, retained origins and independent session state. Source
fingerprints are in source-unicode.json and the implementation handoff; exact
independent review and main execution receipts determine final accepted bounds.

The main coupled source inspects Char16/Long instance reuse, canonical and raw
names, effective literal replacement UCN spelling, original physical contributors,
earlier const-reference selected entities despite late void/caller hiding, actual
reference/local storage identity, cleanup and retained include/macro origins after
disposal. Global address-only transport succeeds while the paired required load
fails; logical short circuit skips a forbidden read. Source ranges remain UTF16
half-open offsets and source owners survive subsequent analyses/session disposal.

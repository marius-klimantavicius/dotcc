# Supported scope and retained-data limits

Initial public format 1.0 serializes the actual resident EDG IL from the accepted
[14 profiles](PROFILES.md). The [field](field-coverage.tsv) and [kind](kind-coverage.tsv) worksheets record
exact adapters, public proof and justified dispositions. All required field proofs
are closed; unsupported/error/sentinel/retired catalog labels do not imply absent
C++ implementation. Exhaustive shared discriminator adapters are distinct from
runtime activation of every enum value. EDG remains the sole semantic authority.

The lowered native graph has priority. EDG can rewrite current signatures, hidden
ABI arguments, member-pointer/reference operations, range-for machinery and
virtual calls. Original virtual-call selected declarations, pre-lowering signatures,
declaration defaults and original source structure are exposed only when retained.
Actual DefinitionType is a retained definition descriptor, not a promise of an
unchanged original signature. Metadata backing expressions and ignored prototype
recipes remain separate from evaluated child traversal; independently EDG-scheduled
execution roots retain precedence. The reader does not reconstruct source syntax,
lookup, overload selection, layout, access or lifetime scheduling.

Partial EH tables retain exact cleanup/storage/condition/array records. Tableless
partial EH retains actual initialization associations and null numeric states;
it does not promise table-based storage parity. Lowering-private destructible
entity descriptors are freed and never recovered. Retained lifetime navigation
means reachability, not a newly inferred unique owner or cleanup dependency graph.
CleanupAction.IsUnordered is explicitly unknown/null rather than inferred from an
Initialization flag. Full portable throw/setjmp lowering remains outside the
approved configuration; MINIMAL_INLINING requires it and is inactive.

Coroutine protocol, await/yield and parameter-copy records are serialized where
retained. They are not a generated frame machine or a simple C# async mapping.
Compiler-accepted inline assembly keeps actual operands/constraints or opaque
asm-function text; no instruction interpreter or later runtime support is promised.
These phases contain no translator or cross-translation-unit linker.

Two module fields (unit-partition name/unit) have a selected stock-input limitation:
the public stock-generated partition IFC import fails before backend due to its
stored/requested name mismatch, while the standalone producer retains no module
root. Their direct writer/API remain present. This is not universal EDG field
inactivity, and neither root nor partition name is fabricated. Actual header-unit
name/resolved-header/import attributes have genuine separate acceptance.

Selected stock-only facts retain source-backed dispositions: disabled test,
unrecognized and checking pragma target seeds; consumed class promotion/allocation
staging; absent CP-generator-only dependent string/hidden-name seeds; nonlazy TLS
startup conjunction inactive in all 14 profiles. Native Microsoft COM/IDL/UUID,
properties/events and ABI records are supported as retained. C++/CLI and C++/CX
were explicitly excluded; no managed-dialect compatibility is claimed.

Source positions are optional. Direct existing metadata preserves actual physical
files/includes versus presumed remaps, source declaration order and retained macro
associations. No token-perfect source tree, macro paste/trivia recovery, frontend
parser cache or lexical-parent guessing is promised. Token-sequence expression
ranges are omitted by backend policy while valid point/token/interpolation data
remain retained. Other unavailable optional positions/links are null/empty. There
are 89 captured optional-source worksheet rows and 15 explicit optional exclusions;
none is relabeled required semantic data. Ordinary C mode disables source name-form
recording at runtime even with a source-profile enabled default.

Reader opening/cold-warm measurements use selected canonical symbol/type/routine
roots, not a whole-TU materialization or OS-cache flush. Row/page caches and batched
reads are bounded; an explicitly requested public list can itself contain all
matching retained elements. FindSymbols is exact stored-name search, not C++ lookup
or guaranteed constant-time resolution. Atom/value sharing does not merge distinct
constant occurrence, source, storage or independent artifact identities.

[Format admission](FORMAT.md) requires the exact supported fingerprint and enabled
required capabilities. Earlier development objects and unknown future contracts
are not promised compatible. No migration or forward-compatibility guarantee is
made. See [the managed guide](../managed/README.md) for nullable relationships,
actual owner/target distinctions and normal property navigation.

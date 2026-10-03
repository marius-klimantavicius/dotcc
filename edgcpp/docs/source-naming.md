# Source names and storage views

`dot_gen_source_names.h` allocates C# spellings independently of EDG entity
identity. Registry keys remain original IL pointers; two source declarations
with the same spelling never become the same routine, field, variable or type.
Source naming is the default, including private function bodies and internal
functions. Managed library exports retain source API names; a separate boundary
wrapper uses a `__body` suffix on the translated implementation when needed.

C# reserved words are escaped (`@params`, `@out`, `@event`, `@ref`). Ordinary
names, including contextual words such as `value`, remain plain. Collision
allocation is deterministic within the relevant owner, record, function or
label scope. It accounts for:

- The configured wrapper class, lifecycle methods and inherited object methods.
- C's separate record-tag and function namespaces, which share a C# member scope.
- Locals whose source scopes overlap after lowering.
- CLR property accessor names (`get_name` and `set_name`). Source methods take
  precedence over global storage properties; bitfield properties reserve their
  accessor slots before ordinary record fields are named.
- The generated `__edg_` namespace and legacy anonymous `fN`, `mN`, `vN` IDs.
- `_`, which is renamed rather than allowed to capture generated C# discards.

Collisions retain a source cue with a suffix such as `__type`, `__global` or
`__local`; metadata records the resulting spelling. Enclosing owner/type name
sets are shared during local allocation, avoiding one copy of all global names
per function in large amalgamations.

## C++ context after lowering

EDG's `clear_parent` and namespace/member promotion intentionally flatten
retained parent scopes. The original mangled name still identifies the source
context. The backend links the unchanged EDG `util/decode.c` under its existing
Itanium target configuration and uses `decode_identifier` for presentation.
Simple namespace, class and function-template context is retained, for example
`Left_Item`, `Right_Item`, and `Left_twice`. Internal-linkage annotations such as
`[local to source.cpp]` are display annotations, not namespaces, and are removed.
Unrecognized decoded context falls back to the retained source basename and
collision allocation. Decoded text never controls semantic identity or ABI.

## Globals, TLS and records

`dot_gen_source_storage.h` exposes existing arena storage through source-named
ref-return properties and uses source-named ref locals for ordinary frame
aggregates. Storage layout, alignment, TLS ownership and cleanup remain under
the existing runtime. Explicit address paths still resolve to stable pinned,
native or caller-frame storage; they do not take an illegal address of a C#
property or move data into an unpinned object.

Defined records and incomplete tags use source names. Nested public types are
the default; `--no-nest-types` prefixes top-level type names with the owner class.
An opaque `handle*` can therefore be declared as `Core.handle*` in a managed
consumer without obtaining its type name from a manifest. Same-spelled C++
records remain distinct. Fields and bitfield properties have source names.

Retained enum definitions and symbolic enumerators are emitted, and switch
cases can recover an enumerator from the retained enum type and exact integer
value. At this checkpoint, enum storage/signature declarations still use the
physical integral type. Source-typed enum declarations have a separate prepared
fixture (`source_enums.c`) and are not yet claimed as implemented. Wider-than-64
bit extension enums use a named constant container because CLR enum bases stop
at 64 bits.

## Metadata and validation

The emitter records kind, original file/line/column/name, managed spelling and
scope, plus public type mappings and routine member origins. The driver writes
`dot-gen.sources.json` and `source-index.md`; `dot-gen.json` references the source
map and its hash. Source-oriented grouping and navigation consume this metadata;
function/size splitting remains available. The compiler's intermediate protocol
is `dot-gen:source`, `dot-gen:public-type` and `dot-gen:origin` comments, which the
driver consumes rather than leaving them throughout formatted C#.

Build 008 acceptance is retained under
`artifacts/campaign/persistent-continuation/`:

- `source-names-008-20261003-100800/results.json`: eight checks covering C/C++
  native and EDG C oracles, static/no-nest and instance/nested JIT, keywords,
  local shadowing, enum cases, namespace/template context, `_` safety, accessor
  collisions, metadata, and direct managed-library APIs with opaque pointers.
- `source-names-multi-tu-008-20261003-100800/results.json`: six original-C/JIT
  checks covering C opaque/complete records and C++ linkage, inline state, RTTI,
  exception identity and owner isolation.
- `source-names-array-005-20261003-094100/results.json`: six original-C/JIT checks
  covering compatible pointer-to-array arguments, returns, assignments,
  conditional expressions and function pointers in C/C++ and both owner modes.

The focused verifier optionally publishes NativeAOT with debug symbols disabled;
the checks listed above are JIT acceptance and do not imply that option was run.

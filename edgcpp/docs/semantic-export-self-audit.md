# EDG semantic-export self audit

The audit uses the **77 actual `cpfe-dot` translation units** selected from
`build/compile_commands.json`, including `upstream/src/dot_gen_be.c` and
`dot_sys_predef.c`. The backend copy is checked against the canonical
`backend/dot_gen_be.c`. It does not substitute the C backend, an implementation
provider, a unity file, or previously emitted C/C#.

The retained campaign is
`artifacts/campaign/persistent-continuation/semantic-export-self-audit-001/`.
It uses the retained `semantic-export-c-emission-001` producer, capturing the
baseline before the subsequent SQLite-driven adapter expansion. Baseline gap
counts belong to that producer hash; the follow-up below uses the newer build.
Its per-attempt command files, native logs, package/source hashes, and
`corpus-corrected/results.json` are the evidence. Each translation unit is
exported separately; this does not test a semantic linker or compiler execution.

## Completed baseline corpus

All **77 actual translation units exported successfully**, after host-header
retries for `dot_gen_be.c` and `float_pt.c`. The packages contain a combined
**49,920,047 nodes, 117,529,173 native edges, and 1,332,581 adapter diagnostics
across 54 categories**, with zero missing or duplicate IDs. The graphs total
45,352,491,492 bytes. These are sums across packages, including repeated
headers and template records, rather than unique whole-program counts.

The 77 baseline primary-source snapshots and both follow-up snapshots were
rehashed and matched their recorded pre-export source hashes. The dot backend
was subsequently edited by another coordinator: both audited captures have
source hash `886aff1674db…`, while the workspace copy at finalization has
`b5b92c380834…`. The evidence records both full hashes. This audit therefore
describes the retained source revision, not those later unrelated edits;
no source-before/snapshot mismatch requires a rerun.

## Current producer follow-up

After the SQLite-driven adapter additions, the actual dot backend and `cfe.c`
were exported again with the new `semantic-export-sqlite-001` producer and the
same successful header profiles. Both still have zero missing/duplicate IDs.

| Translation unit | Nodes | Baseline diagnostics | Current diagnostics |
| --- | ---: | ---: | ---: |
| `dot_gen_be.c` | 3,115,613 | 298,992 | 271,252 |
| `cfe.c` | 427,750 | 8,118 | 4,112 |

The current dot graph still needs typed support for parameter references
(1,296), `sizeof...` (246), new/delete (223), `if constexpr` (165), range-based
loops (104), lambda initialization/expressions (34/30), initializer expressions
(27), void constants (11), fold expressions (6), braced initialization (2), and
one attribute whose local expression is unresolved. Most remaining diagnostics
describe template/class metadata, including hidden names, template parameters,
name qualifiers, exception specifications, and inheritance. The prior builtin,
floating-constant, and noninteger-typed integer-constant gaps disappear in these
two samples. This is a **two-unit follow-up**, not a rerun of all 77 units with
the new producer.

Detailed current inventories are in `current-producer-sample/results.json`;
the tracked [evidence manifest](../semantic-export/evidence-self-audit.json)
records both producer hashes, package hashes, aggregates, and unit summaries.

The follow-up also exposes a concrete **managed-reader size limit**. The dot
graph is 2,452,604,523 bytes, and the current `inspect` command exits 1 with
`IOException: The file is too long`, because `SemanticGraph` reads the whole
file into a byte array. The diagnostic says files must be smaller than 2 GB.
`current-dot-managed-inspect.json` preserves the exact invocation and consumer
hash. The incremental audit reader can inventory this graph; the C# reader
needs a scalable storage/loading design before it can consume it.
The same managed consumer successfully validates and inspects the smaller
374,272,666-byte `cfe.c` package (`current-cfe-managed-inspect.json`, exit 0).

## Initial findings

The first `cfe.c` attempt failed before export: the invocation lacked GNU
compatibility and the portable POSIX/self header directories. Adding the same
GNU 13.3 mode used by `dotnet.py`, explicit C++17 selection, the portable
self-host facts, and `USE_MMAP_FOR_MEMORY_REGIONS=0` produced an unlowered graph
with **427,708 nodes, 976,574 native edges, and 8,117 explicit adapter gaps**.
The audit found zero missing IDs across native edges and the reader's named
node reference fields. This is a closure check, not a complete package validator.

That first successful graph exposes 38 diagnostic categories. The highest
counts are hidden names (1,859), attributes (1,630), attribute groups (1,264),
template parameters (824), and template-parameter type supplements (556).
Concrete semantic gaps include integer constants whose type is not integer
(395), new/delete expressions (54), lambda initialization (11) and expressions
(8), parameter references (4), floating constants (3), `sizeof...` (2),
braced initialization (1), and a range-based loop (1). Counts are occurrences
inside this one package, including headers; they are not distinct source bugs.

## Target and header limitations

The audited semantic producer retains upstream Linux/x86 system predefined
macros. It does **not** install `dot_sys_predef.c`'s portable target hook.
Disabling the predefined macro file does not disable that native system hook.
Portable provider headers and self-host facts therefore do not make this
producer a fully portable target configuration.

This matters for the dot backend's C++ standard library. The initial full
77-unit attempt supplied pinned libc++ headers to every unit, causing
`libc++/__config` to follow `__linux__` into native `features.h` and fail to
find architecture-specific glibc headers. Adding those headers exposed mixed
portable/native locale types, so those failures are configuration evidence,
not missing EDG language support. Ordinary EDG units can use the existing
lightweight portable C and `new` headers without pulling libc++ in.

The real dot backend **does export successfully** with the native GCC 13 C++
header search, explicit LP64 target macros, and the two unsigned maximum macros
used by libstdc++. Its retained `dot-host-native-facts-2` package has
**3,115,613 nodes, 356 source files, and 298,992 adapter diagnostics**. This
fallback is explicitly host-conditioned; it does not validate the portable
libc++ path or provide managed bindings for native library declarations.
All **6,801,927 native edges** and named node reference fields close over the
exported IDs, with zero missing or duplicate IDs. Fifty diagnostic categories
remain. Besides template/class metadata, the graph exposes builtin operations
(1,989), parameter references (1,296), `sizeof...` (246), new/delete (223),
`if constexpr` (165), range-based loops (104), lambdas, initializer expressions,
floating/void/repeated constants, and fold expressions. Generic native edges
retain identity evidence for these records; they do not replace typed adapters.

`float_pt.c` also needs the diagnostic host-header fallback in this campaign.
The portable C `isnan` macro collides with the self-host `quadmath.h` C++
overload. Exporting the unchanged source with native C++ headers succeeds:
**688,962 nodes, 243 source files, and 25,189 adapter diagnostics** are retained
in `float-pt-host-native/`. This does not repair the portable header collision.
Its graph also closes over all referenced IDs. It exposes 1,433 floating
constant adapters, 22 integer constants exceeding the 64-bit adapter, and two
complex-type adapters, in addition to the common C++ metadata gaps.

`audit-self.py` records source hashes, original native compile commands,
export commands, generated configuration hash, semantic producer hash,
libc++ pin and verified header hashes, and per-unit results. Export subprocesses
are serial and bounded by a timeout. Graph inventory uses incremental JSON
decoding, keeping one node at a time plus sets of IDs and references.

## Missing work exposed by the audit

- A portable target hook for the semantic producer and a consistent portable
  C++ header profile for the backend and floating-point implementation.
- Managed graph loading beyond the current 2 GB whole-file byte-array limit.
- Typed adapters for the reported expressions, constants, statements,
  templates, class relationships, and metadata. Closed generic edges alone
  cannot drive correct C++ code emission.
- Linking the 77 packages: IDs are package-local, and cross-unit declarations,
  definitions, static initialization, templates, and ODR identity need a
  semantic linking contract or a supported whole-program export mode.
- Managed C++ lowering for object construction/destruction, exceptions,
  virtual dispatch, and callable/class layout. The bounded C emission profile
  is not an EDG compiler backend yet.

The counts include reachable header/template records and deliberately retain
unsupported or dependent forms. A future backend can distinguish necessary
executable semantics from metadata, but must prove that exclusion explicitly.

## Reproduction

From the repository root, select a fresh output directory:

```sh
python3 edgcpp/semantic-export/audit-self.py \
  --compiler edgcpp/artifacts/campaign/persistent-continuation/semantic-export-c-emission-001/native-build/build/bin/cpfe-semantic \
  --libcxx-acquisition edgcpp/artifacts/campaign/libcxx-acquired-005 \
  --dot-header-profile host \
  --host-source float_pt.c \
  --output-dir edgcpp/artifacts/campaign/persistent-continuation/semantic-export-self-audit-next
```

`--source NAME.c` restricts the run to named compilation-database entries.
The C# emission profiles do not cover these C++ packages. The subsequent
[SQLite milestone](semantic-export-sqlite.md) exercises C translation;
C++ lifetime, template, class-layout, and virtual-dispatch lowering remain
separate work. The [storage investigation](semantic-export-storage.md) addresses
the graph-size problem without changing the production package format.

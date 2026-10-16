# Bounded DuckDB guard-definition attribution

Status: the one-object public definition lookup passed with no issues. This
locates a defined candidate for the exact captured linkage
`_ZN6duckdb29ThrowNullSharedPtrDereferenceEv` in pinned core entry 293. It does
not establish program-wide uniqueness, linking, body behavior, exception closure
or translator/generated execution acceptance. No target body or type graph was
traversed.

The [final receipt](../acceptance/duckdb/guard-definition-report.json) is 13,658
bytes, SHA-256
`055a141376e405625af54ed2dacad6f69d6620481c0bb02ca2a20d2da059ca94`.
Exact audit copies are saved as
[guard-definition-location.json](../acceptance/duckdb/guard-definition-location.json)
and [builtin-expect-stock-audit.json](../acceptance/duckdb/builtin-expect-stock-audit.json),
with the original hashes recorded below.

## Manifest location and public candidate

The location audit read the committed core manifest, its identical
`selected-core-final.json` copy, and `core-source-coverage.json`: three manifests
totaling 485,552 bytes. It searched the exact identifier only in 213
manifest-listed official `src/common/**/*.cpp` files, totaling 3,208,264 bytes.
The sole location was `src/common/memory_safety.cpp:11`, whose bytes match
official DuckDB revision `a809d3cd519df6db70223c562c4de80da311868b`.
Coverage maps that source to unity entry 293. Source text supplies candidate
location only, not function semantics or captured binding.

The coordinator then opened only
`/mnt/storage/roslynobj-duckdb-20261014/export/major2/0293/unit.roslynobj`,
1,837,615,104 bytes, SHA-256
`e8f1840398a035990420e67e3261db8b2ed044c8e6b471d07d36a7f2542d4a46`.
The streamed before/after hashes agree. Its public manifest identifies
`roslyn_gen_be`, EDG revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`,
native-lowered format 2.0, Itanium little-endian target, 64-bit pointers aligned
to eight bytes.

The bounded display-name index returned one FunctionSymbol candidate. Its
CurrentName is the exact requested linkage, AssemblyName is null,
NameHasBeenMangled is true, retained original name is
`ThrowNullSharedPtrDereference`, and MangledEncoding is null. Comparison uses
captured AssemblyName when present, otherwise captured CurrentName; no name
normalization or remangling occurs.

Report-local Function 1 is defined, BodyState `definition`, with body-root
association 3 reciprocally naming Function 1 and scope association 4. Captured
source position is the unity-relative `memory_safety.cpp` path, line 11,
column 6. Signature 7 has a prototype, default convention, C++ external linkage,
no this/source/execution parameters, no variadic or hidden return/copy roles,
and nullable noexcept null. Return-type association 8 is deliberately
unexpanded; the earlier entry-316 declaration report recorded a void result.
This lookup does not separately prove cross-unit ABI compatibility.

Mandatory Open admission used 10 commands/1,632 rows; subsequent lookup used
20 commands/16 rows, total 30/1,648. Lookup limits were 128 commands/512 rows,
with 4,096 cached rows, 128 pages and page size 128. At most four candidates
could be retained; observing a fifth would stop with bounded ambiguity.
Measured build/lookup times were 5.696/5.224 seconds, sampled process-tree RSS
458,002,432/89,022,464 bytes, with no budget violations. The whole lookup phase
had a 120-second/1-GiB budget including hashes and Open admission.

## Separate stock builtin audit

The pristine EDG audit uses exact revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. `builtin_kinds.h:7881` and
`builtin_defs.h:118639–118640` establish the `bfk_expect` discriminant/name map;
`folding.c:10972,11041–11042` includes it in foldability classification.
`interpret.c:12706–12731` evaluates the first argument during constexpr
interpretation and ignores the hint argument there. It deliberately refuses
top-level nonconstant folding so a backend can retain prediction hints.
`statements.c:3184–3199` looks through the intrinsic's first argument for
true-constant classification. No dedicated discriminant/literal case was found
in actual `c_gen_be.c`; the initial nonexistent `c_gen.c` path is preserved in
the audit.

These are scoped stock facts, not a general retained-runtime side-effect rule.
The actual saved call has signed-64 result/two signed-64 operands, a first
argument cast from Boolean, an exact zero hint and LTR evaluation. Future
lowering still needs explicit discriminant/signature validation and preservation
of typed operand effects. It cannot discard arbitrary effectful hint operands
or replace the application guard from its name. The guard's defined body,
other callee bodies and earlier partial shallow-type inventory remain outside
this lookup. The separately attributed arrow-callee trace inspected `_M_get`;
its selected get dependency was not retraced there. No additional lookup or
recursive closure is implied.

## Saved attribution

Artifacts below are under `/mnt/storage/roslynobj-csharp-20261015/t5/` and
contain exact commands, revisions, hashes and bounded excerpts where applicable.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `duckdb-definition-location-1.json` | 70,031 | `9737eac8a3d897041f33006fdff4d6122c467dba1f9c7e06c594d3df581bdcab` |
| `duckdb-definition-facts-1.json` | 7,079 | `69cad7ad3555222e8bcd975a425f47555117b02f54fc5f6e65fe68d9b2b80f62` |
| `duckdb-expect-stock-audit-1.json` | 22,866 | `ccc3a244490f771eb5f135cc14a19a599df4c8bc37dbd7656cd58c289de5c081` |
| `duckdb-definition-source-1/source-snapshot.json` | 7,654 | `8fc06cc52579845996938e895e18a15962d1f9364d51ca185750c878d5b72599` |

The immutable source snapshot contains 48 reader/tool/configuration files;
Program.cs SHA-256 is
`8f09de99ad87fb3b0591bb39457f6f82af10a3c090c21f255038fe68f6f93ebd`.
No producer rebuild/export, product/runtime change, native application call,
raw database access or broad object scan was involved.

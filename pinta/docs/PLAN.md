# Translate the Marius.Pinta interpreter to C# with dotcc

Status: **Implementation in progress; Linux raw/optimized × JIT/NativeAOT runs
the owning consumer and all 73 upstream test bodies in release/debug profiles. One native case crashes;
cross-project regressions pass, while full edge-case qualification remains open. Windows is deferred.**
Created 2026-09-14. Campaign directory: `<repo>/pinta/`.
The user authorized implementation with a coordinator and subagents on 2026-09-14.
The original planning-only restriction is superseded by that request.
Work continues on branch `sqlite`; no branch change or commit is required.
The user subsequently instructed **"ignore win x64 for now"**. Linux x64 is the
active qualification target; Windows items below describe deferred follow-up
work and do not block the current Linux delivery. No Windows success is claimed.

Current evidence and open gates: [VALIDATION.md](VALIDATION.md).
Reproduction entry points: [USAGE.md](USAGE.md). The original native fixture
crash, compiler diagnostics, and deferred Windows qualification remain visible;
successful source emission alone does not complete an interpreter milestone.

## Objective and scope

Translate the user's actual C interpreter in upstream `Marius.Pinta` into a
reusable unsafe C# library. Preserve its bytecode execution, fixed-point decimal
arithmetic, strings, binary data, objects/functions, allocator, and garbage
collector. Supply module input and application callbacks through a small BCL
host, then deliver a separate managed consumer under JIT and NativeAOT.

Only the interpreter is the translation target. Do not port or make product
dependencies of `Marius.Script`, `Marius.Script.Test`, `Marius.Pinta.Script`,
`Marius.Pinta.Managed`, `Marius.Pinta.Managed.Sample`, or any debugger project.
The existing managed wrapper is not a substitute for translating the C engine.
`Marius.Pinta.Test.Files` is allowed as immutable test data; fetching it does not
expand the product scope to the parser/compiler. A source-to-bytecode compiler
is not required for consuming already compiled `.pint` modules.

Use the established [SQLite](../../sqlite/docs/PLAN.md),
[picotls](../../picotls/docs/PLAN.md), and
[MsQuic](../../msquic/docs/PLAN.md) methodology: immutable sources, explicit
configuration/ABI, native differential evidence, reduced compiler regressions,
actual-source retries, canonical callbacks, owning API, raw/optimized output,
and JIT/NativeAOT qualification. Preserve existing translations. Historical
completion, commit, branch-switch, and delegation authorizations in those plans
do not authorize new implementation work here.

## Reviewed source baseline

Inspected upstream `master` at commit
**`b8aeef8f0a97ef8072c9460f9da1c21226f20b1c`** (2020-04-11).
The public header reports `PINTA_API_VERSION` **2.0.2**. Use this commit as the
proposed baseline; before implementation, record its archive SHA-256, MIT license,
all included notices, source manifest, and fixture hashes. No successful native
or translated build is claimed by this planning review.
[Pinned repository](https://github.com/marius-klimantavicius/pinta/tree/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c).

The reviewed `Marius.Pinta.vcxproj` lists 30 C files. Removing only
`src/platform-windows.c` gives **29 core translation units**, approximately
20,018 physical source lines at this revision, before headers/tests. This is an
inventory, not a claim that all 29 are required by every runtime configuration.

```text
api.c             array.c           binary.c          buffer.c
code-rt.c         code.c            core.c            debug.c
decimal.c         decompress.c      encoding.c        domain-global.c
format.c          function-body.c   function-managed.c function-native.c
gc.c              global-object.c   json.c            pattern.c
property.c        heap.c            integer.c         memory.c
object.c          module.c          stack.c           string.c
weak.c
```

Use this project manifest rather than globbing every file under `src/`:
`sample.c` and `native-function.c` are not listed in the library project.
The latter contains a placeholder accessor and must not replace
`function-native.c`. Keep core `debug.c` and any required declarations/hooks;
excluding external debugger applications does not justify deleting core code.
[Project manifest](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/Marius.Pinta.vcxproj).

| Reviewed source | Planning consequence |
| --- | --- |
| `inc/pinta-api.h`, `src/api.c` | `pinta_api_create` accepts caller memory and file callbacks, constructs an API table, and exposes module loading, globals, execution, and borrowed output. This is the primary host seam. |
| `inc/pinta.h` | `decimal` is `i64` with scale `100000000` and precision 8. Preserve these arithmetic semantics rather than replacing them with BCL `decimal`. |
| `inc/pinta.h`, `src/gc.c` | Explicit native root frames and moving-GC fixups depend on stable addresses and object layouts. The CLR GC is not a replacement for Pinta's GC. |
| `inc/pinta.h` | Pinta's `wchar` is 16-bit, but `PINTA_CHAR`/`PINTA_STRING` normally use `L` literals. Native Linux wide literals require deliberate handling. |
| `src/module.c` | Module decoding can byte-swap and mutate input storage. Immutable caller bytes cannot simply be passed through as writable module memory. |
| `src/code.c` | `pinta_code_step`, `pinta_code_execute`, and `pinta_code_execute_module` already exist. An optional budgeted runner can build on actual stepping without translating a debugger. |
| `tests/tests_common.c`, `tests/main.c` | The suite invokes 16 test groups. Its entry/message formatting is Windows-specific, waits for console input, and returns zero; a portable test runner must report actual SPUT failures. |
| `Marius.Pinta.Test.Files` | 50 checked-in `.pint` fixtures, including both ordinary and `-v2` filenames, permit interpreter tests without rebuilding the compiler. Inventory compatibility per fixture; filenames alone do not prove a supported format. |

References: [public API](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/inc/pinta-api.h),
[core types](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/inc/pinta.h),
[module loader](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/src/module.c),
[execution](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/src/code.c),
[test entry](https://github.com/marius-klimantavicius/pinta/blob/b8aeef8f0a97ef8072c9460f9da1c21226f20b1c/Marius.Pinta/tests/tests_common.c).

## Required interpreter profile

| Area | Required behavior |
| --- | --- |
| Engine | Complete selected interpreter/library manifest; no source-language parser/compiler or external debugger. |
| Values | Integers, upstream fixed-point decimals, strings/substrings/multistrings/chars, arrays, objects/properties, buffers/blobs, weak references, and managed/native function values as implemented by the pinned core. |
| Execution | Existing opcode/control-flow semantics, function calls/closures, module globals/imports, stack errors, and explicit Pinta status values. |
| Memory | Caller-provided bounded arena, separate heap/stack budgets, upstream allocation/GC/compaction, deterministic exhaustion, and explicit disposal ownership. |
| Modules | Checked-in compatible bytecode fixtures, module resolver callbacks, exact bytecode encodings, compressed blobs, and endian handling as actually supported. |
| Text/output | Pinta 16-bit string semantics; documented C/UTF-8/UTF-16 API encodings, embedded NUL/length behavior, exact output bytes and explicit copies into managed values. |
| Host | BCL-only in-memory module resolver by default; optional explicitly supplied file resolver. No default unrestricted filesystem or external native function discovery. |
| API | Raw translated API plus owning `PintaEngine`/module handles, global setters/getters, execute, output copy, and documented callback registration/lifetimes. |
| Concurrency | Serialize operations within an engine; qualify independent engines with separate arenas and callback state. Do not equate internal `PintaThread` records with a supported OS-thread scheduler. |
| Targets | .NET 10/C# 14; Linux x64 raw/optimized × JIT/NativeAOT. Windows x64 is deferred by user instruction; other architectures remain separate qualification. |

Preserve supported pattern, formatting, JSON, and compression helpers in the core.
Do not disable value types/opcodes merely to bypass compiler failures. Debug
assertions and core diagnostic hooks get a separate validation build; no Webby,
TCL, web debugger, GUI debugger, or C++/CLI bridge is needed in the product.

## Critical contracts

### Native baseline, wide characters, and actual storage

The original Visual Studio configurations target Win32; they are not evidence
of native x64 correctness. First build a matching 64-bit native oracle and audit
pointer arithmetic, compact frame offsets, heap relocation, alignment, and every
pointer-to-integer conversion. `PintaStackFrame.prev_offset_flags` is `u32` with
an upstream warning about large stacks. Derive and enforce its actual bound;
do not assume an arbitrary 64-bit-sized arena is valid.

Dotcc now documents 16-bit `wchar_t` and wide literal support in
[C-SUPPORT.md](../../docs/C-SUPPORT.md); these are starting capabilities, not
proof that Pinta's actual macros work. Include the supplied declarations needed
for `WCHAR_MAX`/`wchar_t` through an explicit campaign preinclude when necessary.

For native Linux probes, select a documented UTF-16-compatible compilation
profile (for example GCC/Clang `-fshort-wchar`) and test its widths and literals.
Do not pass that profile's 16-bit strings into the system libc's 32-bit `wcs*`
or wide formatting functions. Supply a small portable test-message adapter or
explicit fixed-width test literals where needed; record these harness changes.
Windows native probes can use its ordinary 16-bit wide-character ABI.

`PINTA_HAVE_LONG_LITERAL=0` is not a general fix: its string macro casts a narrow
literal to `wchar*` without transcoding. Do not use that setting to hide the width
problem. Compare non-ASCII literals, surrogate pairs, terminators, lengths, and
the actual emitted arrays before accepting the Unicode baseline.

Separate on-disk module records from runtime pointer-bearing structures.
Compare sizes, alignment, offsets and actual storage of heap objects, unions,
root/reference arrays, stack frames, module records, and API/environment tables.
Host LP64/LLP64 differences must not change fixed-width module data or API lengths.

### Arena ownership and moving garbage collection

Allocate or accept a stable arena once at the owning API boundary. Preserve the
engine's no-general-purpose-allocation contract inside execution; the host may
allocate its arena, callback bookkeeping, module input copies, and returned C#
objects under separately documented limits. Measure these domains separately.

No C# movable object address may outlive its pin. Prefer owned unmanaged arena
storage with a deterministic lifetime, preserving Pinta's allocator and root
frames. Verify `PINTA_GC_ENTER`/`EXIT` macro control flow, early returns,
exception/status paths, native root arrays, relocation, cycles, and weak roots.
Raw pointers returned into the heap must not be treated as stable across a later
Pinta allocation/collection, even if the outer arena itself never moves.

API creation failure must release host-owned resources, including partially
registered handles, without dereferencing a failed allocation. Probe every
meaningful arena/heap/stack/output exhaustion boundary. A native crash or native
undefined behavior is an upstream defect to classify, not a result to reproduce
silently in managed code. Preserve references and document any reviewed repair.

### Module resolver and host callbacks

Implement `PintaApiEnvironment.file_open/file_size/file_read/file_close` with an
explicit context and handle table. Begin with a dictionary of module-name to
immutable bytes; each open gets an independent cursor, and the loader receives
owned writable storage as required. Define short reads, EOF, missing modules,
size overflow, close-on-error, and recursive module lookup/cycle behavior.

Preserve byte-length versus UTF-16-code-unit semantics for every API field;
decode module names according to the configured encoding. Do not assume that
`PINTA_API_ENCODING_C` universally means UTF-8. Reject unsupported host encoding
choices explicitly, and test the pinned implementation's conversion behavior.

Use canonical static managed function-pointer callbacks and rooted context tokens
with clear create/dispose ordering. Guest/Pinta function values are not CLR
delegates. Host native-function callbacks must keep their arguments/return values
rooted across allocations and obey Pinta's error conventions. Do not dynamically
load arbitrary functions or permit exceptions to escape through C callbacks.

### Managed API and execution semantics

Proposed generated API: class `Pinta`, namespace `Managed.Interpreters`, project
`TranslatedPinta`. Keep upstream `pinta_api_*` and core symbols available for
advanced consumers; the separate owning facade is provisionally `PintaEngine`.
No native ABI compatibility with the old wrapper is promised.

The facade owns arena/context lifetime and module-handle validity. Provide load,
set integer/string/null globals, execute, copy global strings, copy output bytes,
and decode output strings. Module handles belong to one engine and become invalid
when it is disposed. The upstream API exposes `unsafe_get_*` pointers; keep that
low-level behavior explicit while making copies the ordinary managed interface.
Audit exactly which calls mutate/allocate and therefore invalidate borrowed data.

Keep ordinary execute compatible with upstream. Optional budgeted execution may
use `pinta_code_step`, but must reproduce initial module invocation, output setup,
code-pointer advancement, finished/error state, and root lifetime. A cancellation
flag outside a blocking execute call does not establish cancellation support.
Treat pause/resume/budgets as a separately tested extension, not an invented
upstream status or part of the initial parity claim.

No secure hostile-bytecode sandbox claim follows solely from translating C into
unsafe C#. Preserve and test the engine's input/memory checks, bound malformed
tests in separate processes, and distinguish translation parity from additional
hardening or prevalidation requirements.

## Workspace and reproducible output

```text
pinta/
  docs/         PLAN, source, configuration, host-contract, blockers, validation, usage
  config/       pinned source/fixture manifests, preincludes, feature/ABI settings
  scripts/      fetch, native-oracle, probe, translate, build, test, dependency-audit
  src/          portable C adapters, BCL module host, owning managed facade
  tests/        upstream adapters, ABI, bytecode corpus, callbacks, managed consumer
  ref/          unchanged upstream sources and bytecode fixtures (ignored)
  generated/    staged inputs and raw/optimized C# with manifests (ignored)
  build/        native reference and .NET/NativeAOT outputs (ignored)
  artifacts/    diagnostics, corpus results, diffs, allocation/performance receipts (ignored)
```

Keep all interpreter-specific work under `pinta/`, not `examples/` or the upstream
project name. Generic dotcc/libc fixes remain in shared projects. Fetch only the
needed paths for builds, or fetch the repository archive and restrict the explicit
source closure; either way, unrelated upstream projects are not built.

Use `--emit=managedlib --nest-types --runtime=c --class-name Pinta
--namespace Managed.Interpreters --split=size --split-size=102400` after verifying
the current CLI contract. Emit `generated/TranslatedPinta/TranslatedPinta.csproj`.
Run the existing semantic postprocessor after emission, preserve the raw source
snapshot, and compare both forms. Scripts must resolve their own paths, record
source/config/compiler/output hashes, support offline reruns, and isolate temp
directories. Preserve upstream source; never hand-edit generated C#.

Platform/test adapters belong in authored files. Necessary upstream portability
or correctness fixes must be minimized, reviewed, and applied as hash-checked
patches to staged copies, with untouched and corrected native results distinguished.
Do not rewrite decimal/GC/interpreter algorithms to make compilation pass.

## Milestones and acceptance gates

### P0 — Pin the interpreter and establish a real native baseline

- [x] Record immutable archive/license/source and all 50 fixture hashes.
- [x] Freeze the 29-unit starting manifest, debug/release configurations,
      16-bit text profile, 64-bit host model, and platform/test adapters.
- [x] Build a portable native test driver that invokes the existing test groups,
      removes the interactive wait, and returns `sput_get_return_value()`.
- [x] Adapt test file resolution/message formatting without changing assertions;
      record each executed case, original-native failure, adaptation, and skip.
- [ ] Inventory bytecode fixture dependencies, enabled/skipped test registrations,
      and required internal functions; run compatible fixtures natively.
- [x] Record existing dotcc regressions and attempt actual-core translation.

**Gate:** reproducible native interpreter/test execution, explicit Unicode/ABI
contract, and real initial compiler diagnostics. Old Win32 project files or
checked-in fixtures alone are not a passing native baseline.

### P1 — Prove types, literals, and callback boundaries

- [ ] Compare native and actual emitted storage for API/core/heap/frame/module
      records, callback fields, flexible object tails, and root arrays.
- [ ] Verify wide literals, `PINTA_CHAR`/`PINTA_STRING`, decimal scale constants,
      unaligned byte decoding, and fixed-width file data.
- [x] Demonstrate arena creation and a real API table callback through a separate
      managed consumer, including forced CLR GC and creation failure cleanup.
- [x] Reduce observed compiler/header/runtime failures; add failing regressions,
      fix shared causes, and retry the full selected interpreter source set.

**Gate:** matching actual ABI/literal observations and callable rooted API seams
under JIT/NativeAOT; no handwritten mirror substitutes for emitted storage tests.

### P2 — Emit and build the full interpreter

- [x] Translate/link the complete configured manifest with core diagnostic hooks.
- [ ] Resolve actual macro/goto, aggregate/union, pointer arithmetic, integer,
      callback, linkage, and initialization defects structurally in dotcc.
- [x] Build raw/optimized libraries and whole-library-rooted NativeAOT consumers.
- [ ] Audit unresolved symbols, dependencies, allocation paths, and static state.
      No native Pinta library or old managed bridge may satisfy unresolved calls.

**Gate:** all selected code builds, real initialization executes, and affected
compiler/libc tests pass. Emission/build success does not establish VM parity.

### P3 — Qualify allocator, GC, values, and helpers

- [x] Run upstream memory/heap/GC/weak-reference tests, checking relocation,
      object graphs, roots, cycles, pinned objects, and allocation exhaustion.
- [ ] Run integer/decimal comparisons and arithmetic, conversion/rounding,
      formatting, sign/overflow/zero/division errors against the native engine.
- [ ] Cover arrays, objects/properties, functions/closures, buffers/blobs,
      strings/substrings/multistrings, encodings, JSON, and pattern operations.
- [ ] Add missing targeted edge cases with exact bytes/statuses and saved seeds;
      compare logical heap contents, not ASLR-dependent pointer values.
- [ ] Exercise native and CLR GC stress together, with canaries around owned
      arena/module buffers and exact allocation-domain accounting.

**Gate:** documented value/memory/helper corpus matches native in all four forms,
with upstream defects and omitted test cases separately visible.

### P4 — Qualify module loading and actual bytecode execution

- [ ] Run all compatible checked-in fixtures through actual loader/dispatch paths;
      include v2, globals, internal/native calls, closures, tail calls, imports,
      object construction, strings, binary output, and compare-numbers cases.
- [ ] Compare status, output bytes, globals, callback order, and module behavior
      with identical fixture bytes, arguments, resolver data, and memory budgets.
- [ ] Cover truncated headers/data, offsets/count overflow, bad tokens/opcodes,
      malformed variable-length integers/compressed data, stack limits, and
      unsupported module versions. Record native crashes/UB separately.
- [ ] Verify swapped-endian input and owned writable module lifetime, failed
      imports, repeated loads/execution, short reads, and callback cleanup.

**Gate:** execution parity for the documented corpus with case-level outcomes;
no source compiler rebuild or debugger dependency is needed to reproduce it.

### P5 — Deliver the owning API and separate consumer

- [x] Implement engine/module ownership, global access, execution, copied output,
      error mapping, typed callback registration, and disposal validation.
- [x] Demonstrate a checked-in receipt/configuration-style bytecode workload:
      resolve modules, supply globals, execute, and verify text/binary results.
- [x] Test cross-engine handle rejection, repeated creation/disposal, callbacks
      during failure, borrowed-buffer invalidation, independent concurrent engines,
      and rejected same-engine concurrent/reentrant calls unless explicitly safe.
- [x] Document low-level versus owning APIs and host allocations separately from
      the interpreter's bounded arena contract.

**Gate:** a standalone C# project consumes the generated interpreter using only
ordinary project references and BCL host services, under JIT and NativeAOT.

### P6 — Reproduce and qualify Linux (Windows deferred by user)

- [x] Run raw/optimized × JIT/NativeAOT on Linux x64, including
      actual Unicode/ABI, callback/GC, bytecode, and separate-consumer cases.
- [x] Record exact pass/fail/skip counts and platform/toolchain/source/output
      hashes; cross-building Windows does not qualify Windows execution.
- [x] Measure initialization, execution, GC/compaction, arena/host allocations,
      peak memory, output copying, and published size against native Pinta with
      equivalent inputs. Record diagnostic versus release profiles separately.
- [x] Audit the product for native Pinta/C++/CLI, parser/compiler/debugger
      dependencies, runtime code generation, hidden imports, and stale outputs.
- [x] Regenerate SQLite with the final compiler and rerun its JIT/AOT corpus;
      rerun affected picotls/MsQuic, Lua/chibi, shared unit/functional, and relevant
      Zig/WAT regressions after shared changes. Use serial builds/tests.
- [x] Publish clean-checkout fetch-to-consumer instructions and source,
      configuration, host-contract, blocker, coverage, and validation records.

**Gate:** full Linux parity/consumer matrix is repeatable with no native
interpreter dependency. Windows execution is deferred by user instruction and
must be qualified separately before making a Windows success claim.

Dependencies: P0 → P1 → P2 → P3 → P4 → P5 → P6. Resolver/API design can inform
earlier phases; it cannot replace validation against the real translated engine.

## Completion and optional extensions

The campaign is complete when the actual selected C interpreter runs the
documented bytecode corpus with native-equivalent values/status/output, preserves
bounded arena and GC semantics, exposes a usable owning C# API, and passes its
required raw/optimized and JIT/NativeAOT platform matrices. A native wrapper,
handwritten C# VM, or one successful sample does not satisfy the objective.

Optional follow-ups: budgeted execution/cancellation using the stepping API,
stronger hostile-bytecode validation, additional host architectures, relocatable
snapshot design, and integration with the fake-instance controller. Parser,
compiler, external debugger, and old wrapper projects remain outside this plan.

# Direct Zig implementation

Current product status (2026-11-06): the complete 92-root direct Zig engine
emits and links with the authored C boundary into 43 Managed.GhosttyVt source
files. Emitted/linked/built/ran is **1/1/1/1**: the full managed project and owning
consumer build with zero warnings/errors. The unchanged owning consumer passes
transcript/style checks, split UTF-8, alternate screens, resize/reset, concurrent
instance isolation and disposal. Module-constant vtable addresses now retain
static storage, and indexed zero-sized values preserve effects without physical
element memory accesses. Both raw and processed products now pass the owning
consumer, all 24 differential cases, and the allocation matrix under JIT and
whole-library-rooted NativeAOT. All 61 immutable getters use RVA data and allocate
zero managed bytes after warmup; 141 actual storage helpers have at most one
parameter (required maximum: two). Both delivery audits pass, including notices,
host-provider attribution and 92 source/PE entry bodies. The retained upstream
suite remains in progress: all original native 58 render and 2,564 core cases
pass with zero skips/failures/live allocations. The projected test runners also
pass all 2,622 cases natively with their original assertion bodies preserved.
Those full native receipts precede compatibility patch 0015; its focused native
checks pass, and the final full matrix refresh remains pending.
Managed execution remains in progress: all 58 render, 138 formatter,
31 logical-render, 16 Unicode, 43 protocol and one glyph-reset case pass under
raw JIT (287 accepted cases), with exact transcripts and zero skips, failures or
live allocations. See `artifacts/direct-resume-20261104/direct-core-managed/managed-accepted.json`.
The remaining managed cases and additional execution rows are pending. Scalar `u0`,
declaration membership, integer peer widths, generic callback bodies and empty
error-union storage now clear their reached blockers. The render allocator failure is resolved: source field alignment is preserved,
and pointer-cast operands execute once. Other reached groups currently stop at
source type-name evidence, discarded comptime reflection results, or optional
reflection captures. The latest complete integration again passes 1/1/1/1 with zero C# build errors
and the unchanged owning consumer passing. The missed non-null projection after
an orelse exit and explicit packed-union object contract are corrected. The
published products remain qualified; a final refresh follows upstream closure.
The managed test runner has a separate native-entry name while native runners,
selected test bodies and assertions remain unchanged. The original translated C test
host provider already links and executes with direct Zig allocation/alignment/
ownership controls. External native-library
delivery remains outside this product; required source host services and the
managed boundary remain real dependencies. Reproduction and latest stage
commands: `artifacts/direct-resume-20261104/managed-build-repair/integrate.py`,
`integration-stages.json`, `actual/engine.json`, and `joint-build-summary.json`.
Subsequent sections retain historical failure evidence; this status and the
latest actual receipts govern the current frontier.

The accepted C route is preserved at commit
`3592b42929380f9cbd3209d8729f600629e85999`. The user authorized the direct route
after that commit. This phase uses actual pinned Zig engine sources as DotCC
inputs; generated C remains independent comparison evidence.

The selected terminal functionality, 92 selected managed entry methods, owning API,
`Managed.GhosttyVt` name, aggregate data option and maximum factory arity of two
remain required. The retained 58 render and 2,564 core upstream cases resume in
this phase. The direct product's four-row owning/differential/allocation matrices
pass; the retained upstream matrix and final campaign acceptance remain pending.
The qualified direct source products are now the ordinary defaults at
`generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj` and
`generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj`. The published
source bytes match the previously qualified four-row products. The normal
`ManagedConsumer.slnx` reference chain is sample → owning API → canonical direct
library; no `TranslatedProject` override is needed. The accepted C products were
moved byte-for-byte to `generated/c-route/` and remain available by explicit
project override. Normal regeneration selects the `direct` recipe profile and
preserves these canonical paths. The C route requires explicit `--profile scalar`.
The default direct verification command is an owning-consumer smoke check;
full upstream managed qualification remains pending.
Migration, evaluated references, builds, consumer output and current PE/RVA checks
are recorded in `artifacts/direct-resume-20261104/direct-default/`.

Reproducible direct source delivery now has dedicated commands. Run
`libghostty/scripts/translate_direct.py --compiler <private-dotcc.dll> --zig <pinned-zig>
--work /mnt/storage/dotcc-libghostty-disposable/<task>
--evidence <receipt-directory>` from the repository root. It regenerates the actual upstream module graph and tables,
emits Zig directly to an object, links the authored boundary, and builds raw and
postprocessed `TranslatedGhosttyVt` projects with notices and source ownership.
The native build supplies generated Zig inputs and an oracle; no C engine is
emitted or consumed. An existing fresh graph can be supplied with `--graph`.
Apply the explicit compatibility chain with `libghostty/patches/direct/apply.py` first when
starting from a pristine pin.

`libghostty/scripts/verify_direct_consumer.py --raw-project <raw.csproj>
--processed-project <processed.csproj> --work <disposable-directory>
--evidence <receipt-directory>` runs the unchanged owning consumer in all four
rows. The current products and exact commands are recorded in
`artifacts/direct-resume-20261104/direct-delivery/products.json` and the adjacent
`direct-delivery-consumer`, `qualification24`, `factory-qualification`, and
`direct-delivery-audit` receipts. Historical receipts retain their original paths;
the migration receipt maps those qualified bytes to the new canonical defaults.
Accepted C sources remain in the explicit fallback directory while final upstream
qualification continues.


## Governing managed scope correction, 2026-11-04

The user clarified: “Dotcc does not care about C/Zig interop - we are creating
C# (while technically it is possible to expose C usable exports from native
aot, this is not my goal for this iteration)”. The product is the selected
managed C# engine and owning API translated directly from original Zig.
Working libghostty governs: partial C-FFI, callback, layout and reflection
semantics are required wherever actual engine/consumer dependencies need them.
The user's later clarification supersedes any blanket FFI exclusion. Native C
library delivery and unrelated full native interoperability qualification are
not independent goals. Internal translated callbacks must execute correctly;
managed function pointers are the current target. Managed-consumer NativeAOT
remains a separate retained execution row. Native Zig is an oracle.

Limited targeted compatibility patches to pinned libghostty are explicitly
authorized when behavior-preserving and materially shorter than large general
compiler work for an incidental idiom. Keep patches minimal, reproducible,
documented and validated; do not replace engine algorithms or remove required
functionality. The applied compatibility patch is recorded in `patches/direct/manifest.json`.

The direct graph now selects `function_pointer_abi: managed`. This changes target
lowering, not engine sources, feature selection, source layouts or algorithms.
Zig `@export` names become managed entry methods. Existing native-interop work
and the accepted C products are preserved without extending those qualifications.
The reproducible managed graph is retained under
`artifacts/direct-resume-20261104/module-graph-managed.json`.

### Current engine attempt, 2026-11-05

The unchanged Lib/fast/source managed graph now passes value-if by-reference
captures, comptime enum receivers, optional slice generic arguments, inline tuple
iteration, computed enum member lookup and the style writer's vector bitcast.
Imported mutable callback state, on-demand extern prototypes (including private
namespace aliases), source `c_longdouble` storage and selected empty-block aliases
now pass as well. The nested `orelse return` in the third argument to
`m.map.append` at original `terminal/formatter.zig:404` now lowers with ordered
receiver/argument spilling. Earlier reads and effects run before a later
argument's conditional early return; lazy arms retain their own statements.
Union value switches now select their discriminants, including payload captures
inside larger expressions. Unicode's catch-based compile-time array bound, a
provably dead final error cleanup, and the keyboard flag stack's source-proved
zero array default also pass. Checked `@shlExact` now precomputes tabstop masks.
Typed container constants can initialize real nonzero array fields through the
existing statement initialization path; palette copies retain independent inline
storage and a shared original pointer. Pointer-to-array field indexing now
dereferences the pointer instead of addressing the pointer field's storage.
Error-valued catch peers now preserve errors, lazy fallback effects and cleanup.
Enum declaration aliases resolve in their definition scope without adding tags;
optional result locations resolve associated calls on their payload container.
Nonexhaustive enum `_` switch cases now cover unknown backing values after
checking every named tag is explicitly covered. Enum mode survives module imports.
The engine passes cursor-style and clipboard callback switches. Source opaque
`AnyFuture` now retains nominal pointer identity with no invented value layout.
Reflection demands a pending named declaration before classifying its type;
`std.Io.Operation` is now correctly recognized as a union. Reflected `@Union`
now accepts the explicit source tag enum and preserves its identity, values,
reflection and payload storage. Comptime optional-type fallbacks now resolve
`Dir.Permissions` through `File.Permissions` with lazy branch selection and
preserved definition/type metadata. Local containers in comptime initializers
use their reified owner's identity, clearing MultiArrayList's duplicate `Data`.
Needed whole-width integer/pointer atomic loads/stores use existing managed
same-width primitives with full barriers. Source std functions can be referenced
as values, and direct sibling method aliases resolve in their defining container.
Wide storage-vector integer bitwise operations, Eq/Ne and bool-mask bitcasts
now pass the actual printable-codepoint scan. The direct input applies ten
compatibility patches. First, `Action.Value` forwards to the same `c_union.Value`
helper through a function declaration. Native checks confirm the payload type
for every actual action tag, and reduced managed source/object controls pass.
See [patch rationale and reproduction](../patches/direct/README.md). The latest
engine now passes the integer compare-exchange slot claim. Comptime boolean seeds
retain their boolean type; explicit comptime optional-integer calls preserve
optional provenance. Optional enum backing conversion propagates its payload
result type, and statement-block breaks accept their void value while preserving
effects and cleanup. These clear subsequent stream parsing/dispatch failures.
Imported optional slices now retain their declared type for null initializers,
clearing the build-info API. Exact-byte `@embedFile` now embeds the real X11
resource. A second compatibility patch makes its table dimensions explicit
scalar comptime calculations and uses a data-derived bounded name buffer. Native
Zig validates all 782 names, terminators and RGB values against `rgb.txt`, plus
every lookup through the original map. The managed interpreter now also validates
the full actual table and all lookups. Source mode now uses the real `std.Target`
and typed OS default range; preserving named aggregate constants removes the
false Windows PEB demand. Source target IR/value-copy proofs pass, and the owning
actual-target ManagedLib now compiles and returns 42 through both source and
object/link execution. Backend fixes preserve void-value effects, constant loop
conditions, nested sequence scopes and boolean switch patterns. Evaluating value-if
conditions in source comptime initializers now clears the libc feature-query
function/void peer failure. Lazy file namespaces keep TinyIo static helpers from
demanding unused Threaded/Csprng layouts; extern source widths and subsequent
bitcast/flow fixes clear its positional read. A third compatibility patch widens
ASCII vector lanes into the same output individually, avoiding a 256-byte vector;
4,112 native decoder comparisons agree with original source. Tuple source-width
handoff fixes clear the formatter instances. Reflected field/type-array widths,
typed constants and agreeing switch arms now retain their source evidence, and
integer atomic RMW uses existing exact-width helpers. Pointer CAS/Xchg now keeps
an independent outer optional presence flag, including failure observing null;
source-proven packed allocator atomics also pass. Selecting comptime type-info
switches before function-alias probing clears MultiArrayList.Slice.set. OSC
tag-name string arguments, device-status iteration over a known comptime slice,
parallel-loop source widths and the mode table's computed key slice now pass.
Style offset assertions accept computed comptime field names through the existing
name resolver. Numeric-vector integer OR reduction and the source-proven packed
row/vector load now clear lowering of the eight-row dirty scan. A fourth compatibility patch
expresses packed field-path traversal recursively, with the same offsets,
reflection, widths and mask operations. Native original/patched masks agree for
all actual Row/Cell paths. Required source-width propagation through @FieldType
is repaired; the managed nested-field mask reduction also passes. Known byte
slice captures and local windows retain exact comptime bytes. Parser action-array
elements now use the existing ordered operand spill path, and optional-element
array copies compile through a raw byte-copy overload. Narrow field reads use
their recorded source width. Mutable comptime aggregate field stores now use
the existing interpreter after verifying a mutable comptime owner; runtime and
const owners remain rejected. Finite reflected error sets and captured-function
scope now clear tripwire.Error. Source-order ordinary comptime name loops clear
program_status queries. The fifth compatibility patch shares the original
color-scheme byte selection with its maximum-length calculation, preserving the
real Writer encoder and errors. Optional function-pointer value-if captures and
enum namespace value constants clear subsequent version/DECRQSS analysis.
File-struct methods now specialize comptime value receivers without passing them
at runtime. Source-order comptime loops retain aggregate/index/reference captures;
typed string seeds keep their slice type and logical length. The sixth patch uses
exact byte operations for incidental comptime XTGETTCAP encoding, retaining the
original map builder and runtime lookup. All 273 native map entries and an edge
map match original bytes/terminators; actual managed source/object lookups run.
Runtime &.{...} response records now use local storage with preserved evaluation
order. The ninth patch derives the exact 12-byte maximal mode report bound from
the original maximal Report/framing/digit counts; runtime encode stays unchanged.
Native original/patched encodings agree for all 655,360 ID/state/ANSI combinations.
Differing byte-string conditional extents now peer to a byte slice, fixing an ANSI
NUL in real managed encoding. Pure computed byte constants now join printSentinel
formats correctly. The tenth patch binds DCS tryHook's unchanged switch to its
optional Hook payload before the existing return; native allocation/failure/
ownership behavior agrees over133,120hook cases. Proven u5 complement widths
survive their cast back; actual FlagStack runs source/object, including corrected
u3 push/pop wrapping. Reflected names become real values and typed bounded-slice
array copies retain independent storage, clearing StaticStringMap.initEnum.
LZ4's chained optional orelse now retains its optional peer and lazy fallback;
unsigned bit-count division and contextual comptime-integer C# conversions also
pass. Actual unchanged LZ4 emits, links, builds and executes through both managed
source and object routes. All54 exact decoded roundtrips and their encoded
checksum12153282228646284172 agree with native Zig. This is a component result.
The original TinyIo statx, futex and getrandom dependencies now bind to real
managed host services with source identity and guest handle ownership preserved.
Neither Threaded.io nor its singleton is required by this path. ComptimeIntSet's
anonymous comparator retains its lexical parent and specialization-specific
captured keys; indexed-value narrowing consumes existing source widths. The
minimal eleventh source patch retains duplicate-key comptime rejection via
unreachable, changing only that diagnostic. Two exhaustive u16 sets and signed
keys pass managed source/object and native controls; duplicates still reject.
Symbolic callback values and multidimensional comptime arrays now preserve
actual std.base64 Codecs initialization and all four native-equal managed codec
controls. Runtime tag names retain exact source names. Known false immutable
std.options fields prune the unused debug-I/O branch without touching its guard.
Indexed sequence values now retain declared element sentinel evidence through
the existing nested source metadata; the reached absorbSentinel query clears.
Resumed saved Writer work now retains mixed-tuple comptime integers through
copies and forwarding; distinct values specialize separately. Comptime array
validation runs through existing source iterator/capture binding. Array sentinel
identity distinguishes generic specializations, and inline numeric switch labels
match the promoted subject type. Source error-set type values resolve/validate
members, bit-count locals keep their unsigned source type, and source-proved
packed values bitcast into equal-logical-width arrays. Original Style.hash clears.
The PageList row-copy loop now lowers a source-proved many-item destination
pointer using its bounded companion, with ordered once-only operand snapshots.
Subsequently reached vector comparisons, lane shifts, widening, array copies and
packed-cell stores also clear. A twelfth explicit compatibility patch gives the
existing cursorCopy hyperlink-ID switch its optional slice result type. Source
hash integer widths, large literal truncation, scalar switch captures and
container-alias sentinel metadata are retained; the original string decoder
builds and runs through both source and object routes. Forty integrated checks
pass for that batch. Tagged-union payload declarations/captures now preserve
ordinary versus sentinel slice types, clearing hyperlink cleanup. Generic
signature metadata is captured while its comptime arguments remain in scope,
preserving response-buffer sentinel values. These are focused compiler controls;
full-engine execution remains unqualified. The full original 92-root engine now
emits a 30 MB object. Two further explicit patches preserve RGB formatting with
ordered positional arguments and wide-cell interleaving with fixed lane stores.
Subsequent compiler fixes retain enum backing widths, tuple null/pointer peers,
optional enum return locations, empty optional payload presence, packed boolean
bitwise stores and imported comptime option fields. The engine alone links into
42 managed source files. Joint engine/C-boundary linking currently rejects
nominal Zig/C enum contracts at ghostty_cell_get. The engine-only managed build
still reports 617 C# errors; it has not built or run. Current commands, limits and
receipts are in artifacts/direct-resume-20261104/rgb-writer/.
The standalone
actual Writer reduction now emits, links, builds and runs42 in both source and
object forms through ordinary C# hosts, with the exact formatting assertions
unchanged. Its required real Reader declaration and bounded optional storage
dependencies are complete. No full engine output is implied by this control.
The earlier optional wide-vector bitwise regression is closed with native-proven
128-byte payload/256-byte optional extent and 128-byte alignment. Source/object
controls check nulls, copies, addresses and by-reference upper-lane writes;
single-item vector pointer stores preserve receiver/RHS order and reject const
storage. Commands are in `optional-wide-vector/REPORT.txt`. The seventh/eighth
patch files add caller-owned Parser action output and use it in the actual stream
caller, retaining public next and the original expressions/deferred state update.
Native transition/payload comparisons and bounded managed source/object controls
pass. The actual emitted engine's wrapper reachability and other array returns
remain delivery checks; generic array-return allocation is not repaired.
The earlier cursor switch rewrite remains withdrawn; patch 0012 only adds the
existing optional result type to the original local.
Object/joint-link/build/run is now **1/1/0/0**; full managed execution is pending.
No engine method or upstream case is accepted from focused regression results.
Commands, validation and current receipts are retained in
`artifacts/direct-resume-20261104/comptime-feature-condition/REPORT.txt` and
`actual-format-width-worker/REPORT.txt`, `actual-pointer-cas-worker/REPORT.txt`,
and `selected-comptime-paths/REPORT.txt`. The latest actual receipt is
`actual-writer-named-argument-context/engine.json`. Current batch commands and limits
are in `comptime-enum-and-reporting/REPORT.txt` and `reproduce.sh`. Further commands
are in `runtime-struct-temporary/REPORT.txt`, `field-mask-path/REPORT.txt` and
`reproduce.sh`. Writer product commands
are in `actual-writer-managed-build-worker/REPORT.txt` and `reproduce.sh`.
The Source/mode/format continuation's current commands and validation are in
`actual-mode-report-size-worker/validation-commands.json` and `validation.json`.
Current flag/map/wrapping evidence and limits are in
`flag-complement-width/REPORT.txt` and `actual-flag-stack-wrapping-worker/evidence.json`.
LZ4 commands/results are in `actual-lz4-optional-fallback-worker/REPORT.txt`
and `actual-lz4-managed-literals-worker/REPORT.txt`, `reproduce.py` and
`validation.json`.

## Inputs and compiler work

`tests/direct/prepare.py` provides `prepare_direct(ctx)`. It builds the original
native library offline with private caches and captures the actual ordered
module closure. Seven preparation controls pass. The staged entry preserves all
original root bytes and appends only the existing ABI accessor import. All 1,025
copied source files and 25,660 acquired files were preserved in the first run.
The strict compiler graph has 11 modules, including generated options, Unicode
inputs and uucode's scoped dependencies. Native target/optimization options stay
in a separate audit manifest; their presence is not a claim that DotCC applies
them.

The relevant true-upstream frontend at
`d3678b5c50f86bf65d831ee4efda6134a7894116` is being integrated with its necessary
compiler/runtime dependencies. It uses the 4.11 Zig lexer while preserving the
local C lexer. Existing C payload serialization and canonical function-pointer
behavior must remain qualified. Unrelated upstream features are outside scope.

The supervisor's invocation-local `ZigCompilationOptions` and
`--zig-modules-file` preserve module-local aliases, relative-file ownership and
the actual root import. Twelve unit cases and both source/object executions
pass in isolated compiler snapshots. Native Zig rejects assigning one physical
file to two modules; the graph rejects it as well.

## Actual source retries

| Snapshot | Observed result | Evidence under `build/` |
| --- | --- | --- |
| `direct-compiler-snapshot-01` | Compiler builds with zero warnings/errors. Actual engine root fails resolving union payload alias `SemanticPrompt`. | `actual-root-{command,result}.json`, `actual-root.log` |
| `direct-compiler-snapshot-02` | Registering union aliases before layout advances the actual root to `Enum`'s ordinary typed parameters in a type-returning function. Native/source/object union reduction returns 42. | Same receipt names; `union-forward-alias/before-result.json` |
| `direct-compiler-snapshot-03` | Implicit comptime argument support reaches value evaluation for `Enum`'s `target`. That value is not yet resolved. The small anytype fixture also remains failing. | Same receipt names; `enum-control-result.json` |

Later private snapshots execute native-backed bottom-flow, top-level comptime,
function exports (including the formatter's 56-byte options aggregate), actual
upstream Enum destructuring/mutation, lazy imported constants, integer
`@divCeil`, optional result typing, pointer tuples and actual-data bitcasts.
Their focused controls pass; this does not establish full engine acceptance.
The upstream compiler-test candidate also exposes remaining semantic issues and
has not been accepted wholesale.

The native source compilation is `build-lib`, independently of DotCC's
intermediate object output. The graph now records `output_mode: "Lib"`; optional
per-invocation `ZigSourceOutputMode` feeds synthetic `builtin.output_mode`.
Unconfigured callers retain the previous executable context. Native library and
executable guard controls, root-module identity, and managed source/object export
controls pass. Target options remain unapplied audit data; the configured
optimization context is described below. The original preparation graph is
preserved; retries use a derived graph.
Snapshot 10 reached the unmodified standard library's
`debug.assert(@import("std") == @This())` at `std/std.zig:247`. Resolving the real
standard-library import and comparing canonical module identities in value
positions now passes that assertion. Native execution returns 42; 18 focused
source/object context, identity and bottom-flow cases pass. Snapshot 12 reaches
`Command`'s reified `union(Key)` tag lookup in `terminal/osc.zig`. Resolving the
tag through its canonical enum and declaring owner advances the actual root to
`Page.styles: StyleSet`, an alias of the upstream reference-counted-set generic.
The tag repair passes native valid/invalid controls and source/object execution;
the next type-resolution wall remained open. Inline-container and deferred-alias
repairs now reach `Style.flags`, then fail on the quoted SGR variant
`256_underline_color`. Exact source/member identity is being separated from its
CLR spelling. Module equality also distinguishes nested container types, and
object metadata retains the distinct noreturn descriptor.

These are attempted gates, with zero accepted engine exports. The existing
canonical C products and root compiler binaries remain unchanged during these
private probes. Nonbyte bitcast widths now have explicit metadata; missing-proof
routes and general slice dereference/array destinations remain open. The reviewed
member-name repair preserves exact valid UTF-8 identities separately from CLR
names. Quoted declarations and methods remain outside that release; rejected
normalization controls are retained rather than counted as passing semantics.

## Remaining acceptance

Complete actual-source emission and canonical transactional publication first,
then execute the owning consumer, physical ABI/differential qualification,
factory RVA/allocation checks, import/export audit and retained upstream matrix
in raw/processed JIT and NativeAOT. Reproduce offline warn/off runs with authored
preservation and run affected shared/downstream regressions before accepting or
committing this route. Native SharedLib alias-address equivalence is not
established by managed public forwarding methods.

## Combined validation and remaining representation gates

Snapshot 13 passed 285 focused unit checks. Its combined execution run retained
113 passes, eight optional-result regressions and one known quoted-name skip;
it is not an accepted combined build. The width envelope had incorrectly
replaced wrapper storage types with payload types. A separate supervisor repair
preserves wrapper result locations, projects payloads only at legal builtin sinks
or explicit unwraps, and rejects raw wrapper bitcasts. Snapshot 15 passes 50
focused executions and 43 unit checks, including native-backed optional/error
payloads and four raw-wrapper rejection cases. The original failure receipts and
independent review are retained under `build/direct-result-location-wrappers`
and `build/direct-bitcast-independent-review-r45`.

The bounded packed-width helper proves logical widths from source declarations;
it does not establish CLR storage parity. Nested packed storage and zero-sized
embedding have independent measured differences and remain required repairs.
The native optimization context also needs the pinned compiler's `fast` tag;
the older synthetic `ReleaseFast` spelling is retained only for unconfigured
legacy callers. The configured context now passes 38 unit checks, 10 executions,
seven preparation controls and 16 native compile controls. Only the captured
root/shared-std optimization context is applied; this does not establish target,
libc, per-dependency mode or native code-generation parity.

The private quoted-member implementation passes the earlier SGR identity wall.
The actual Lib+fast graph next requests `RenderState.Row` fields before that
nested body's registration. Publishing body work before parent layouts and
consuming it through the existing owner-scoped mechanism repairs the native-backed
reduction. Publishing function-body work before global initializers also lets a
global type constructor evaluate an ordinary helper on demand. Six source/object
cases and 12 affected executions pass; 37 selected unit checks pass. The next
actual-source attempt reaches `PageList.MemoryPool` and rejects the unsupported
allocator layout. This is recorded in `build/direct-renderstate-triage-r50`;
accepted direct engine exports remain zero.

Allocator layout needs its own repair: the curated type reports 24 bytes while
its current runtime stores a context plus four function pointers (40 bytes on
x64); native Zig's allocator uses two pointers (16 bytes). A fabricated layout
descriptor would hide that difference. The separate upstream C allocator is an
extern record and must retain its existing ABI. Native controls also show that
copies share the vtable address and observe table mutation; the current runtime's
by-value table does not. Alignment is a one-byte logarithmic native enum rather
than the runtime's eight-byte byte count. A private, explicit source-type policy
is being validated privately for the pinned allocator family while preserving
legacy callers. The bounded packed/zero source repair has been integrated after
private combined validation; general auto-layout parity is still unproved.
The selected engine also requires packed unions inside 64-bit cells and a
128-bit packed style. A first bounded 1–64-bit carrier implementation does not
remove either requirement. Constant payload serialization for those storage
representations also needs proof before using the RVA path.

One accidental root build was cancelled during restore, before binary emission.
The accepted compiler DLL hashes remained unchanged. Its restore-only attempt
and cache changes are retained in the R46 incident evidence; subsequent builds
use explicit private project paths. No direct-route acceptance follows from it.

Snapshot 16 passes 481 selected unit checks and 179 execution checks with no
skips. The execution receipt does not include the private extra R46 reflection
class: its two void-list readiness failures remain open in separate evidence.
The unchanged actual graph still stops at allocator layout, with zero accepted
engine exports. At that snapshot, the private source-allocator policy and logical
enum-backing controls had not passed the managed physical allocator gate; eager
analysis of an unused standard-library conditional alias blocked it. Later
results are recorded below.

A separate callback review found native Cdecl calls targeting ordinary managed
methods. Normal execution and NativeAOT passed, but both source and linked-object
controls crashed under GCStress. A standalone native-entry prototype passes
real C callers, GCStress and NativeAOT using separate native entrypoints and
matching Boolean adapters. This is mechanism evidence, not a compiler repair.
Coherent C/Zig pointer representations and source/object contracts remain required
before the actual callback bridge can be accepted. Private compiler integration
now passes real C callers under GC stress for selected, directly typed source
addresses. Object and opaque flows still reject in that implementation; the
original opaque callback failures remain separate open gates.

Snapshot 17 combines the packed-storage repair with the final member-name and
layout-path codec changes: 484 unit checks, 179 execution checks, eight focused
source/object replays, 22 metadata rejection controls and six positive object
links pass. Physical bytes, alignment, stride, neighboring storage, GC and
allocation measurements match the pinned native controls in those bounded
replays. NativeAOT was not repeated after this merge; earlier standalone R49
NativeAOT receipts remain separate evidence. Source promotion preserves the
accepted C compiler binaries, canonical output files and index bytes.

A fresh true-upstream review on 2026-10-30 found main still at the previously
reviewed `d3678b5c5`; the checked Zig feature branches are already represented on
main. The earlier selective integration helps generics, imports and comptime
evaluation, but upstream has no complete fix for the current allocator, packed
layout or callback representation gaps. It does contain void initializer/member
dependencies omitted from the local backend integration. The initializer repair
is now being validated privately, with effectful values rejected rather than
discarded. The [blocker review](../build/direct-upstream-blocker-review-r57/REPORT.txt)
records exact commits, branch identities and primary source links.

Snapshot 18 combines the source-type policy and conditional declaration deferral
with the bounded upstream void-initializer dependency. The unchanged physical
allocator and borrowed-table callers execute 42. An independent unsafe observer
confirms allocator size 16, alignment 8 and stride 16; the vtable is 32 bytes with
slots at 0, 8, 16 and 24; Alignment is one byte containing exponent 3. Copies
retain context/table identity and observe table mutation. These are physical
checks, not general automatic-layout or callback-safety acceptance.

The unchanged full Lib/fast/source engine retry then reaches `SafetyLock` and
fails because synthetic `builtin.mode` is an untyped `.fast` enum literal, so
the real standard library's `.runtimeSafety()` method cannot resolve. The first
producer-only annotation candidate failed type-identity controls and incorrectly
accepted a missing declared type. Imported annotated enum constants now use
their declaring owner's existing typed value materialization. Forty focused
checks and four native controls pass, including missing-type rejection and the
real `std.lang.Optimize.runtimeSafety` body. Eight additional full-stdlib runtime
controls stop at unescaped `default` enum members in generated C#. This is a
binding repair with an explicit remaining execution gate. The retry's launcher
lost the numeric exit status after reaping; its exact diagnostic and absence of
engine output are recorded without inventing an exit code.

Separate private repairs preserve struct initializer order across inline-array
copies and remove runtime void-result storage. Their native-backed controls
pass. A subsequent scalar comptime repair eliminates speculative initializer
lowering: a mutation formerly ran twice and returned 44 instead of native 42.
The proposed void comptime success path was withdrawn after it exposed silent
acceptance of an unknown loop predicate. Valid void comptime blocks therefore
remain an explicit rejected case pending evaluator safety work. None of these
private results accepts the engine or its 93 exports.

Snapshot 19 combines those frozen repairs and passes 491 unit checks. Its
unchanged physical allocator caller emits successfully, then encounters the same
`default` keyword error during C# compilation. The next engine retry is held
until the Zig member-target repair restores that physical gate; the shared C
identifier helper and accepted C tools remain unchanged.

Snapshot 20 applies that one-method Zig repair. The exact combined compiler
passes the unchanged physical caller (emit/build success, execution 42), and all
18 full-stdlib and existing mode/output runtime controls pass. Eighteen ordinary
C output comparisons remain byte-identical. The unchanged actual engine then
exits 2 at `RenderState → PageList → MemoryPool`: unresolved identifier `T`.
No engine object is produced; 1,922 recorded source/graph inputs and four tool
DLLs remain unchanged. A separate diagnostic API run records the live generic
and container-constant stack without changing compiler sources.

The broader snapshot-19 run retains 221 passes and four failures: two known void
comptime rejections and two new calling-convention annotation failures in the
existing legacy curated-mode fixture. The latter requires separate type/tag
contract review and blocks acceptance. Private strict-loop work also rejects
unknown conditions and discarded unknown effects before allowing successful
void results; its narrower successes do not waive remaining nested-value,
defer, or lowering-time loop failures.

Snapshot 21 adds strict reached-loop conditions and discarded-expression checks
before allowing successful void comptime results. Its 591 selected unit checks
pass; 245 of 247 executions pass, with the two calling-convention failures
retained. Snapshot 22 adds owner-aware generic dispatch for declaration-literal
calls. The actual missing `T` was present in the captured environment; the old
call path incorrectly lowered a comptime type argument as a runtime value. The
unchanged engine then reaches a computed alignment argument that the restricted
constant evaluator cannot execute.

Snapshot 23 combines a bounded legacy convention-eligibility repair with
enum-only argument evaluation through the existing typed interpreter and body
demand callback. That convention repair supplies no source type, value, payload
or layout substitute for a configured standard library. The combined compiler
builds without warnings or errors; 699 of 701 selected unit checks pass. The two
retained failures require unsupported scalar-pointer interpretation. The
unchanged allocator caller emits, builds and executes 42.

The combined execution set has 297 passes and four retained failures, with no
skips. Both original calling-convention fixtures pass again. The four failures
are the scalar-pointer interpreter control and an existing enum-seed return
emission limit, each through source and object routes; their unchanged
assertions and generated-code diagnostics remain in the snapshot receipt.

The single unchanged Lib/fast/source engine retry exits 2 at
`RenderState → PageList → MemoryPool`: the generic packed type
`size.Offset(page.Row)` lacks the required owner/AST storage proof. All 1,922
recorded inputs and four tool DLLs remain unchanged. No engine object is
produced. The [snapshot receipt](../build/direct-compiler-snapshot-23/actual-root-result.json)
records this boundary; broader storage, native callback and all-export gates
remain open. Native-entry mechanics stay private while independent review
findings are repaired. Identity-before-layout indexing is a separate design
round, with no production laziness claim.

A subsequent private one-method repair preserves the declared enum type when a
comptime seed becomes a runtime literal. It also retains the exact backing type
for interpretation. The original nested enum-return source/object controls now
execute 42. Six native fixtures and all 12 corresponding managed routes pass;
the selected run has 699/701 unit and 311/313 execution passes, retaining only
the scalar-pointer interpreter limitation. Eighteen ordinary C output pairs
remain identical, and a source-only replay reproduces all 12 project/object
emissions. This [private result](../build/direct-enum-seed-values-r73/result.json)
has not been promoted or used for another engine retry.

Private snapshot 24 combines that enum-seed repair, the independent source
identity query API and the reviewed native-entry mechanics. It builds cleanly,
passes 857/859 selected unit checks and 311/313 executions, and passes the
unchanged allocator prerequisite (emit/build success, execution 42). The two
failures in each set remain the scalar-pointer interpreter controls. The identity
API has no production consumers. Opaque callback/object flows, native aggregate
entries and opted shared-library output still require further work; the original
opaque callback GCStress failures remain open. No engine retry or root promotion
was performed. See the [combined checks](../build/direct-compiler-snapshot-24/combined-checks.json).

The packed-evidence extension stopped before production edits. Existing capture
can freeze a same-named caller type or value into an unrelated callee, where
native Zig reports an undeclared name. Reinstalling that captured value would
preserve the error. A definition-owned binding-frame contract is therefore a
prerequisite to enabling the new packed proof; declaration metadata alone does
not certify lexical visibility. The [stop report](../build/direct-packed-reified-evidence-r76/REPORT.txt)
retains the native and IR evidence. The last actual-engine failure remains the
snapshot-23 packed proof boundary, with zero accepted exports.

## Resumed implementation, 2026-11-04

Cleanup removed the private compiler snapshots and their patch files. The root
compiler source and retained original eleven-module preparation graph remain.
The resumed compiler builds into
`/mnt/storage/dotcc-libghostty-disposable/resume-20261104/compiler`; the pinned
Zig tool, caches, test products and engine attempts also stay under that campaign
directory. Canonical accepted C products and root compiler binaries are preserved.

The original `src/lib_vt.zig` is retried through `scripts/retry_direct.py` with
`Lib`, `fast`, `stdlib_types: source` and `--aggregate-data`. Its latest exact
command, original root hash, graph and emit result are recorded in
`artifacts/direct-resume-20261104/actual/engine.json`; the diagnostic is in
`engine.log`. A failed emit is zero engine objects and zero accepted exports,
not a working translated engine.

The resumed implementation repairs definition-scoped packed generic captures,
source-backed packed unions and nested layout demand, source standard-library
selection, owner-scoped generic calls and nested aliases, typed imported enum
and optional constants, and source-selected function constants. Actual engine
retries have passed the former `size.Offset(page.Row)` missing-proof failure,
RenderState and PageList.MemoryPool. They then reached `Handler.Effects`, which
requires Ghostty's `lib/struct.zig` reflected C-layout constructor. Labeled type
blocks now execute their comptime statements, including mutable type/attribute
arrays and parallel reflection loops. Array element pointers retain the actual
comptime backing storage. The reduced reflected-struct fixture emits, links,
builds and returns 42 through both source and object routes; pinned native Zig
returns 42 too. The focused packed/deferred execution selection passes 16 cases.

The full engine now reaches `Handler.UnknownSequence` and currently stops
resolving its `Osc = osc.Command.Unknown` payload type. It has not emitted an
object, linked, built or run. Continue from that concrete source failure; the
remaining callback ABI and final direct qualifications are still outstanding.

Further unchanged-engine retries passed `UnknownSequence.Osc`, the reflected
`modes.Mode` enum and packed `ModePacked`, the root `FullPanic(tinyPanicImpl)`
function argument, deep module type aliases and imported/inline error-set
merges. The focused deferred execution set now passes 16 cases (including
reflected enums, packed defaults and error merges), in addition to the eight
packed execution controls. Native reflected enum, reflected packed defaults and
error-merge controls each return 42. These reduced products run; the full engine
still has zero emitted objects and zero accepted exports.

The current full-engine work is `std.Options.logFn`: a generic function type with
comptime and anytype parameters. The compiler now retains the original function
type AST and template identity as comptime metadata, validates assigned source
signatures and models function values in the comptime evaluator. It does not
manufacture a runtime function-pointer signature or address for that type.
The options initializer and its uninitialized comptime storage are the next
actual-source boundary being tested. The latest receipt remains authoritative.


The next unchanged-engine retries passed the original x64 memset wide-vector
stores and its empty volatile memory-clobber assembly. Storage vectors retain
all 128/4 bytes and unaligned load/store/value-copy semantics; the exact empty,
operand-free memory-clobber form emits a managed full fence. Other assembly
remains unsupported. The original memset loops have not been replaced.
Conditional import aliases now navigate from top-level comptime blocks, which
reaches the public C export bodies. Subsequent fixes cover inline exhaustive-enum
value switches, comptime receiver type-returning methods, declaring-module width
transport for reflected packed backings, optional peer results for `catch null`,
unevaluated `@TypeOf` member access, enum-literal generic seeds and union tag-type
unwraps. Focused source and object-linked products execute; the unchanged engine
still has zero emitted objects, links, builds, runs and accepted exports.

The active path is Ghostty `lib/union.zig::TaggedUnionOptions` and
`TaggedUnion`, demanded by `terminal/c/sgr.zig`'s C attribute representation.
Typed type-valued local constants and null defaults in type bodies now evaluate.
Its current actual diagnostic and exact command are in
`artifacts/direct-resume-20261104/actual-reflected-union/engine.{log,json}`.
The original graph and root are unchanged. Continue from this reflected-union
path; the callback ABI and all final direct qualifications remain outstanding.


The reflected-union worker implements actual type-valued option fields,
`@Union` with source field types/alignments, `@unionInit`, custom C aliases and
inline union payload/tag specialization. Original `lib/union.zig` controls
return 42 in pinned Zig, source emission and object/link execution, including
64-byte CValue and 72-byte tagged C storage. The unchanged engine passed that
construction; its receipt is under `actual-reflected-union-worker`.

Subsequent actual retries exposed failed source registrations hidden by opaque
pointer rewrites. Source pointers now retain their canonical declaring type and
raise their original layout failure when a field is demanded; source mode also
rejects the old withdrawn-parameter runtime-trap fallback. Empty imported source
files retain their zero-size struct type, file constants and ordinary module
re-exports. Source `pub fn` visibility no longer creates unintended native
exports; native entries remain explicit `export fn`/`@export` declarations.
Inline enum prongs now isolate hoisted statements and side effects per selected
case. Reduced controls exercise guarded prongs and single evaluation.

The reached circular-buffer default is explicitly `undefined`. Source undefined
values now retain undefined comptime fields/elements and capture identity, with
assignable aggregate storage; a comptime condition/arithmetic use cannot silently
become zero. Managed runtime initialization may choose zero bits for undefined
storage. Comptime-only narrow `simd.iota` vectors retain the existing interpreter
array path. Latest focused verification passes 23 units and 44 executions; this
is reduced correctness evidence, not full-engine acceptance. Latest engine work
is demanded `ModeEntry` registration before its source struct literal. Receipts
remain zero engine objects, links, builds, runs and accepted exports.


The runtime graph selects the existing inventory's 92 terminal/API methods.
On the user's scope clarification, a targeted dependency check found no terminal
runtime or owning-consumer use of `ghostty_type_json`: its only implementation
returns a standalone comptime layout manifest, aliased and exported by the root.
The manifest is retained separately because no selected runtime behavior depends
on it, not because FFI is prohibited. A 93-root probe currently fails optional
comptime type binding in `TypeDecl.initUnion`; receipt and exact graph are under
`artifacts/direct-resume-20261104/actual-type-json-dependency-probe`. Restore it
to the runtime graph if a selected consumer needs the manifest. The other 92
methods remain rooted; no full 93-method acceptance is claimed. The generic
`managed_roots` setting contains no Ghostty-specific compiler logic.


Managed source continuation, 2026-11-04: namespace integer literals now keep
comptime_int and reflected member-array lengths keep usize. The reached log2
width failure is cleared without guessing a width from its CLR carrier. Source
top-level type constructors are deferred until demanded; type classification
selects a comptime if branch before inspecting it. This fixes actual eager
analysis of unused Linux aliases and an unselected POSIX cmsghdr declaration.
Source builtin ABI metadata now names std.Target.Abi and uses its real methods.
Imported type values retain Zig's meta-type. Optional pointer aliases and
orelse-return bindings carry their declared pointer class into generic calls.

Latest engine receipt: artifacts/direct-resume-20261104/actual-optional-pointer-class/engine.json.
The unchanged Lib/fast/source managed-root engine passes OSC allocator pointer
reflection and stops at a requested aggregate layout containing CType.Optional
(`Unsupported or incomplete layout: unsupported:Optional`). Object/link/build/run
is still 0/0/0/0. No full managed product entry methods are accepted. The current
layout representation remains an implementation blocker, not an external C ABI
qualification gate. Isolated CLI build passes with zero warnings/errors; 21
source-resume units and 14 source/object execution controls pass. Native Zig
controls for namespace types, selected branches, integer provenance, pointer
classes, and ABI method lookup execute 42. These reduced results do not certify
the engine. post-api-reproduce.sh includes the managed graph and current controls.


Allocator/render continuation after the optional-layout worker: demanded optional
allocator storage is modeled as 24 bytes/alignment 8. Source pointer/slice types
now retain alignment attributes (unspelled versus explicit), and source pointers
retain their single/many/C class through returned values and fields. Generic
keys distinguish these attributes. Unknown reflection evidence stays explicit.
Typed container constants publish before dependent type aliases, fixing
MemoryPool.item_alignment; sizeof/alignof demand forward by-value bodies, fixing
PageList.Pin. Method demand surfaces cached source record failures.
Pointer-valued error unions use the existing managed nuint carrier consistently
for constructors, reads, catches and propagation. No native export qualification
was added. Native controls for alignment attributes, two independent reified
alignments, forward layouts and pointer success/error/try paths return 42.

The actual unchanged engine now reaches Terminal allocation layout and rejects
Terminal.flags because the recorded packed-storage implementation admits only
1..64 logical bits. Latest receipt:
artifacts/direct-resume-20261104/actual-pointer-error-union/engine.json.
Engine emitted/linked/built/ran remains 0/0/0/0. This packed field is engine
storage in the selected source graph; extending its proven representation is
necessary for managed allocation and execution, not for external C interoperability.


Continuation after wide packed storage: original Terminal.flags is now proved
u85, size/alignment16/16 and admitted by the bounded UInt128 implementation.
Nested empty source records now carry zero-member maps and retain initializer
side effects; original TinyIo and nested wrappers are checked natively. This
clears TerminalWrapper.io. Derived slices retain/reduce source alignment using
known byte offsets or an element stride for runtime offsets. Result-located
source comptime evaluation now keeps enums and supported aggregates; optional
comptime enum payloads use their declared result location. @ptrFromInt now
lowers integer addresses to the required pointer type. A shared initializer bug
was corrected: an untyped comptime integer was incorrectly classified as bool
by the condition-fold probe. It now preserves its type. Labeled void blocks in
switch prongs use statement lowering, including early exits/defer cleanup.

Latest CLI build succeeds with zero warnings/errors. Twenty-two source/object
execution controls pass (labeled-prongs-functional.log in disposable campaign).
Native controls return42 for nested empty effects/guards, slice alignment, typed
comptime enums/optional captures, integer/pointer round trips and labeled prong
cleanup. Native TinyIo/nested zero layout assertions also pass (durable native
probe/command under actual-zero-wrappers). These remain reduced controls.

Actual engine receipt: actual-capture-expression-origin/engine.json. It advances
to original terminal/osc/parsers/semantic_prompt.zig:328, parse: a by-reference
value-if captures parser.capture and the else branch marks invalid then returns
null. Current lowering rejects that form. No engine object/link/build/run exists
(0/0/0/0), and no complete managed product methods are accepted. This capture
expression is the next implementation task, not an external ABI qualification.

Latest host controls and generic lookup evidence: `artifacts/direct-resume-20261104/statx-host-worker/REPORT.txt` and `computed-index/REPORT.txt`.

Symbolic callback/Base64 and indexed sentinel evidence: `artifacts/direct-resume-20261104/symbolic-callback-worker/REPORT.txt` and `indexed-sentinel/REPORT.txt`.

Resumed Writer/array/mode controls and error/count/packed repairs: `artifacts/direct-resume-20261104/resume-error-types/REPORT.txt` and `reproduce.sh`.

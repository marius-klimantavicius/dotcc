# Observed blockers

These are observed campaign results. A passed native baseline is not translated
execution; P1–P6 remain open.

## B001 — Comma-separated bit-field declarations (fixed)

Untouched `blink/x86.h:37` contains valid C `uint8_t omode : 2, genmode : 6;`.
The first dotcc decoder attempt rejected its comma. A reduced native-checked
fixture now lives at `DotCC.FunctionalTests/Fixtures/bitfield-declaration-list`.
The generic parser and IR now preserve named/anonymous bit-field lists and their
widths. All five focused bit-field functional cases pass. The full affected
Release build passed, with 2218 unit tests and 491 functional tests passed;
1011 functional rows were skipped.

## B002 — Native header coupling in decoder inclusion (probe adaptation)

`x86.c` includes `modrm.h` only for the selected profile's `Mode` macro, which also
pulls the entire Machine/POSIX host header closure into the decoder. The initial
attempt reports unresolved `sys/uio.h`, `termios.h` and a `signal.h` parse error.
`stage-decoder.py` verifies source hashes then replaces this include in staged C
with the exact upstream DISABLE_METAL `Mode` expansion. Untouched and staged
native decoder probes agree on all 11 cases and ABI output. This scoped decoder
seam does not resolve the complete core's POSIX dependencies.

## B003 — Multidimensional and unnamed array parameters (fixed)

The staged decoder reaches `xed_set_simmz_imm_width_eosz` with parameter
`const u8 eosz[2][2][3]`; dotcc rejects the second array dimension. Generic parameter declarators now preserve pointer-to-row stride and permit
unnamed array prototypes. Both native-checked reduced fixtures pass; the actual
decoder passes raw/optimized JIT/NativeAOT with matching outputs. Full post-fix
Release build, 2218 unit cases and 494 functional cases pass; 1017 functional
rows remain skipped.

## B004 — Restricted CPU feature advertisement (open)

Upstream extended CPUID leaf 0x80000001 advertises FPU even under DISABLE_X87.
The selected managed product needs an explicit reviewed capability mask and
execution tests for every advertised bit. See HOST-CONTRACT.md.

## Platform and packaging limits

Linux x64 is the only observed execution host. Windows execution and the complete
raw/optimized × JIT/NativeAOT matrix have not run. Native host GCC/binutils/.NET
binaries are recorded by version/hash; their distribution packages are not yet
archived as hermetic toolchain inputs.

## B005 — Function specifiers preceding tagged return types (fixed)

After staging core translation units away from the upstream header directory,
actual translation reaches `static inline struct Dll *dll_last(...)` in dll.h.
The grammar supports `inline` before builtins/typedefs but missed struct/union/enum
tags. A native-checked reduced functional fixture and generic specifier handling
pass, and the actual core retries past this declaration. The full shared suite
passes (2218 unit, 494 functional, 1017 functional skipped).

## Include staging requirement

Dotcc adds each input translation unit's directory to its include map. Passing
`ref/.../blink/*.c` directly consequently lets `blink/string.h` and `blink/signal.h`
shadow system `<string.h>` and `<signal.h>`. Core/decoder scripts stage verified
source copies in a directory without those colliding headers. Initial signal
parse messages must not be misreported as proven signal-language defects.

## B006 — CLR references in unmanaged jump buffers (fixed)

The allocated jump-buffer reproducer emitted CLR `LongJmpToken` references inside
native C storage, which the GC cannot trace. Generic ordinary jump storage now
contains a numeric identity and handlers capture a fresh identity per execution.
The independent compacting-GC consumer matches native behavior under raw/optimized
JIT/NativeAOT. Signal-aware mask semantics use the separately measured adapter;
see NONLOCAL-JUMPS.md. Full ordinary-jump/declaration regression passes 2222 unit
and 500 functional tests, with 1025 functional rows skipped.

## B007 — Function-form parameters and global array typedefs (fixed)

Actual syscall prototypes use parameters written as functions, which C adjusts
to function pointers. Actual debug storage uses a global array typedef. Generic
parameter adjustment and global/TLS array storage now preserve those declarations.
Native-checked fixtures and direct/object-linked thread/GC tests pass, and the
actual sources retry beyond both defects. Validation shares the B006 full suite.

## B008 — Negated assignment setjmp guards (fixed)

Actual machine.c and syscall.c contain valid controlling expressions such as
`if (!(rc = sigsetjmp(m->onhalt, 1)))`. Generic capture recognition now preserves branches, loop exits, late/repeated
jumps and the assignment target's old value while evaluating the buffer. Native
fixtures and forced-GC raw/optimized JIT/NativeAOT consumers pass; actual machine.c
and syscall.c both emit past this guard.

## B009 — C System type shadows the BCL namespace (nested library fixed)

The actual Machine/System storage probe emits but fails C# compilation because
Blink's `struct System` shadows BCL namespace references. Authored runtime sources and backend-generated BCL references now use global
namespace qualification. The C type name is preserved in planned nested library
output. Direct/object-linked separate consumers and the actual 236-row core ABI
probe pass raw/optimized JIT/NativeAOT. Global executable output still collides
with the root namespace and is not the selected Blink library layout.

## B010 — bare negated jump guard (fixed)

Actual debug.c uses `if (!sigsetjmp(...))`. Generic lowering now protects the
complete block tail and preserves later/repeated recovery jumps. Native and
forced-GC four-mode jump tests and repository regressions pass (`6ceff18`).

## B011 — nested string rows and omitted outer bounds (fixed)

Actual disarg.c needed target-typed character string rows; disspec.c then exposed
`table[][8][8]`. Generic initializer and grammar fixes preserve retained inner
bounds and infer only the omitted outer extent. Native reductions and the full
repository suite pass; both actual sources emit (`6ceff18`, `3eccb02`).

## B012 — implicit preprocessor endian comparison (fixed)

Undefined endian macros compared as zero and selected swapping for aligned
16/32-bit upstream reads. The explicit measured little-endian storage profile
fixes selection without declaring a native CPU or OS. Independent literal-byte
load/store tests, decoder and actual core ABI matrices pass (`7b36060`).

## B013 — missing borrowed va_list formatter (fixed)

Actual log.c could not call vsnprintf because the generic header/runtime lacked
it. The new bounded cursor formatter passes native and all four managed modes;
log.c emits, and the full suite passes 2234 unit/509 functional rows (`1e10b72`).
Wide character/string and long-double cursor conversions reject explicitly.

## B014 — authored callback function-pointer owner (fixed)

Direct calls resolve authored partial Blink methods, but address thunks route
undefined callbacks to Blink.Libc, producing CS0117. The termination boundary's
native direct/indirect probe passes, while its preserved generated consumer fails
at compile time. Generic lexical ownership resolution now passes direct/object-linked callback
and runtime fallback tests. The complete termination guard matrix passes
raw/optimized JIT/AOT, including real C function pointers; no emitted C# was patched.

## B015 — declaration specifier order (fixed)

Actual memorymalloc.c declares `_Noreturn static void PanicDueToMmap(void)`.
The parser currently rejects that legal ordering. It is a declaration-order
defect, not an array-parameter defect. The generic reduced repair and full
repository suite pass, and actual memorymalloc.c emits.

## Current closure and runtime gates

Full source emission/linkage remains in progress. Measured terminal, stat, limits
and statvfs declarations remove source-level gaps; unimplemented operations stay
isolated rather than binding generic host services. The private host components
have qualified focused consumers, but the full generated interpreter has not
yet executed guest instructions. P1–P6 remain open. The B004 exclusion mismatch
is corrected and tested as documented in HOST-CPU.md; broader CPU/host capability
qualification remains separate.

## B016 — static-local array multi-declarators (fixed)

Actual strace.c declares `static char abuf[64], sabuf[80]` after its TLS marker
is intentionally removed by upstream DISABLE_THREADS. Generic multi-declarator lowering now reuses rooted pinned storage separately
for every array, preserving nested dimensions and initializers. Block-scope TLS is separate and is not required by this
observed profile. Native/direct/object tests pass across forced GC, actual
strace.c emits, and the full repository suite passes 2235 unit tests and
517 functional tests, with 1039 explicit skips.

## B017 — physical shared-header identity (fixed in orchestration)

The first frozen full core build emitted 90 objects but could not link anonymous
signal-info structs, because each TU used a distinct physical copy of abi.h.
Anonymous names include header paths. The staging helper now shares one canonical
content-addressed header tree and keeps stable per-source paths. Actual two-object
and 17-object links pass. All 92 objects emit, but full linkage reports a distinct
linger_linux conflict (address.c/describesignal.c), subsequently resolved by B021. This was campaign
staging identity, not an upstream instruction or compiler algorithm change.

## B018 — generic Libc System.IO.Path shadowing (fixed)

The first HostProcessPolicy fixture defined a C helper named Path. Its generated
owner member shadowed System.IO.Path references in embedded Libc, producing
CS0119. Renaming the authored test helper SelectExecutable makes that fixture
qualify. The five generic runtime call sites now use global::System.IO.Path;
the native-checked Path-named fixture passes direct/object and flat/nested
consumers. See RUNTIME-PATH-IDENTIFIER.md and the full regression result below.
No corresponding pinned-core definition has yet been observed.

## B019 — explicit TLS arrays lose storage flag (fixed)

The private signal registry concurrency test showed that explicit `_Thread_local`
scalar/record arrays were shared, while scalar TLS and typedef-array TLS worked.
IR construction now carries the storage flag through ordinary, extern and
function-pointer array declarations into the existing pinned per-thread backend.
Unsupported initialized TLS arrays fail explicitly instead of becoming shared.
Native, direct and object-linked worker/GC reductions pass. No registry storage
reshaping conceals the defect.

## B020 — object-linked global members deduplicated by line (fixed)

The TLS reduction then exposed repeated getter braces and ThreadStatic attributes
being discarded during object linking. Global sections now merge complete
backend-generated members, preserving attributes and bodies while retaining
identical-definition deduplication. Actual signal registry object-link all4 passes.
Combined full suite: warning-free build,2238 unit and520 functional tests pass,
1041 explicit skips; artifacts/repository-tls-global-member.log.

## B021 — binding macros change aggregate identity across include orders (fixed)

A socket tag macro rewrote upstream linger_linux.linger only when socket.h was
included first. The authored socket header now declares its actual struct linger
tag without that token macro. Four other aggregate mismatches came from the
embedding driver including upstream types without the shared binding preamble;
the driver now includes host-bindings.h before its probe. No upstream guest
record or instruction algorithm was changed.

Opposite include orders pass native, separate object linking, raw/optimized JIT
and NativeAOT (header-order/attempt-017h8krz). Actual address.c, describesignal.c
and managed-driver.c also link with zero conflicting aggregate signatures:
objects/a5dbe30aa6538f86024de177c5c44ce54c32838083428c18a3cffd1461ecff15/three-object-link.json.
The new 95-source frozen full profile is building; full managed execution remains
an open gate, not implied by this reduced linkage.

## B022 — full containing class conflicts with upstream Blink function (fixed)

All95 objects emitted, but explicit --class-name Blink failed valid class-name
checking because machine.c defines the actual Blink function. Linking the exact
same objects as BlinkCore succeeds with no aggregate conflicts. The upstream
function remains unchanged; this is a campaign container selection correction,
not a compiler defect. Authored bridges now select BlinkCore under BLINK_FULL_CORE
and retain Blink for focused fixtures. Default conditional projection reproduces
every previous bridge source exactly; no method body or generated C# was edited.
The full consumer must define that symbol and use the matching container.

## B023 — nested generated C# names collide (fixed)

The first complete raw C# build reports CS0102 for DescribeFlagz, DisArg and
sysinfo_linux in BlinkCore. Actual C has separate tag and ordinary identifier
namespaces. The generic output layout now moves only colliding tags into a
collision-safe nested scope and preserves their type aliases, including enums
and escaped names. Native/direct/object/split/GC regressions pass, and the actual
derived full link clears these collisions; see TAG-NAMES.md.
Baseline: artifacts/core-execution/attempt-68vow5of/raw-build.log.

## B024 — static-local aliases collide across translation units (fixed)

That same raw build reports duplicate once__s0, b__s0 and buf__s0 members in
BlinkCoreGlobals. Per-object local-static numbering restarts; static-function
qualification does not also qualify these local-static symbols. Identical
declarations can silently deduplicate storage. The IR now tracks hoisted symbols
and applies the existing source-identity suffix before object emission. Native,
direct/object and GC regressions pass. Twelve affected actual objects re-emit
with21 distinct qualified locals, and their derived full link clears all CS0102
errors; see STATIC-LOCAL-OBJECTS.md. Full guest execution remains open.

## Next actual full-build failures

The explicit mixed-producer diagnostic replay at
artifacts/core-execution/attempt-0imxwoxa clears all seven original CS0102 errors
and reaches355 later diagnostics. It is not a fresh uniform-compiler emission or
managed runtime result. The summary and exact generated/source matches are
preserved beside that receipt.

- **B025 — unsigned coercion of C boolean results (open):** calls, stores and
  returns omit conversions from emitted CBool to uint/ulong. Native reduction
  returns5; the current C# build fails.
- **B026 — array-to-pointer member typing (open):** expressions such as
  `m->opcache->field`, where opcache is a one-element record array, infer int
  because member-type lookup peels pointers but not decayed arrays. The actual
  record declarations are correct; this also affects decoded instruction fields.
  Native reduction prints `40 3 2` and reproduces four conversion failures.
- **B027 — fractionless floating literals (open):** valid C `1.f`, `2.` and
  `3.e1f` are emitted with invalid C# spelling. Native reduction returns42.0.
  The apparent SSE `.f` member errors came from these literals, not union fields.
- **B028 — unreachable profile branches retain undefined names (open):**
  constant-false disabled-JIT paths still render Jitter and other unselected
  helpers. Generic removal must preserve C short-circuit effects and label entry;
  neither generated-source rewriting nor helper placeholders qualify a fix.
- **B029 — remaining selected source/header/host bindings (open):** genuine
  upstream and libc helpers, constants and29 isolated host operations remain
  unresolved. A source/binding ownership inventory is in progress. Native loader
  extraction independently adds five upstream files; service execution is still
  unqualified.

The three type/literal reductions and compiler identities are recorded at
artifacts/core/reduced/typing/receipt.json. B018/B023/B024 have passing focused
repairs and the centralized repository run passes:2239 unit and535 functional
tests,1047 explicit skips (artifacts/repository-tag-static-path.log). The build
has zero errors and17 analyzer warnings in existing unchanged test files.

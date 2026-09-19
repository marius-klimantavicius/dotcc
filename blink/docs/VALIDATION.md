# Validation ledger

All observed executions below are Linux x64. Windows execution has not run.
Every skipped/unrun form remains open; a decoder pass is not CPU execution.

## Latest corrected-profile qualification

| Gate | Latest receipt under `artifacts/` | Result |
| --- | --- | --- |
| Final delivery | `translation/attempt-xxngak_0` | 109 sources, 108 verified reused objects and one fresh NEG-corrected ALU producer; raw/postprocessed builds and final manifests pass. |
| Canonical core / publication | `core-execution/attempt-273a6hks`, `publication-audit/attempt-og66g378` | All four managed forms and exact AOT publication inventory pass. |
| CPU | `cpu-conformance-managed/attempt-disfjyq2` | 504 normal cases per form, 2,016 total; exact defined-state/invariant agreement, 46 custom fault cases excluded. |
| Valid ELF / TLS | `elf-loading/attempt-qclmm2fz`, `tls-loading/attempt-m6fpvl1m` | Native and all four managed forms pass bounded valid loading and explicit TLS startup. |
| Clean public sample | `clean-delivery/attempt-ru82jd14` | Commit 497ce69; fresh compiler and all 109 objects (zero reuse), final solution and actual JIT/rooted-AOT sample pass; final checkout clean. |

Actual guest mapping/page-table lifecycle checks now pass at
`guest-memory/attempt-gbyg6j74`: native/all-four exact state, two normal cycles,
growth/cross-page copying/protection metadata/remap/cleanup, and stable retained
host-pool accounting. The final CPU expansion and four targeted NEG repair
regressions pass all four forms, closing the finite selected-profile P3 gate.
CPU receipt SHA256: `e3b4a964d69e0bced3d2896ea093f66c535008709fbd196318bc0fe7b99aa72e`.
Only four ALU NEG bodies changed after the memory/loader runs; the other 108
producer records and Host/header/compiler inputs are identical. Those component
receipts are retained evidence, not post-NEG reexecution. The clean sample row
qualifies its stated pre-NEG revision. Further P6 regressions/performance remain
unrun. No service worker or Windows execution is qualified.

## Earlier post-libsmb2 refresh history

The current Release compiler build is recorded at
`resume-current-compiler/attempt-9_82zbv2`. The delivery invocation
`translation/attempt-rv0jhxuv` passes all 109 selected sources, pinned native 25,
raw build, semantic postprocessing and final build. The first invocation emitted
109 objects afresh with zero reuse; later reuse has exact compiler/profile and
producer provenance. The final bundle and immutable raw manifests are verified.

`ManagedConsumer.slnx` and the normal owning sample pass JIT and rooted Linux
NativeAOT, matching native/profile observations. Receipt
`attempt-0quh0jc1/qualification.json` preserves the first NETSDK1047 publish
failure and the successful identical-command retry. No source change was needed;
the cause is unproven. Full core execution at `core-execution/attempt-8vbjtywv`
passes raw/optimized JIT/NativeAOT, and `publication-audit/attempt-b0wppzl8` passes
for those exact binaries. Direct IL audit limits remain unchanged. All 236
ABI/register rows pass in all four forms at `core-abi/attempt-ezjynh7t`. Valid
pinned ELF loading passes all four forms at `elf-loading/attempt-r3eglgkk`; it
executes no guest instructions and the image has no PT_TLS.

The reviewed integer correction is now present in stable delivery
`translation/attempt-w78n0i5u` and profile `attempt-7byl9v1e`. Its fresh full-core
matrix passes at `core-execution/attempt-yzck8kpr`, with publication inventory
`publication-audit/attempt-e0l80fe1`. The expanded CPU matrix below also uses this correction. Valid ELF
`elf-loading/attempt-qclmm2fz` and explicit TLS `tls-loading/attempt-m6fpvl1m`
now pass native and all four managed forms against the same assembly, retaining
their bounded fixture limits. The clean consumer workflow still predates this
correction until its refresh is recorded.

The following table and older sections preserve historical evidence. Rows are
not current-compiler passes unless explicitly refreshed above or in the current
progress entry. Historical custom fault-injection and invalid-ELF cases are
excluded from new runs; only existing pinned upstream cases are permitted.
Service startup/worker, broader CPU/memory coverage, current dependent-campaign
regressions and Windows execution remain open. The normal sample is partial P5.

Clean public workflow from committed `f38de7f` passes at
`clean-delivery/attempt-0gfaz9m3`: fresh compiler, 109 fresh objects with zero
reuse, pinned native 25, immutable raw/final manifests, final project/solution,
and actual normal sample under JIT and rooted NativeAOT. Exact commands and
binary/tool/source identities are preserved; checkout and compiler identities
remain unchanged. Only the pinned archive was copied. Installed tools/NuGet
cache remain shared; Windows and service-worker execution are not claimed.

The refreshed normal-only CPU witness passes 449 selected cases in each managed
form (1,796 total) at `cpu-conformance-managed/attempt-3r1msqyr`; its native
reference is `cpu-conformance/attempt-7j03qmvz`. Hardware capture returns normally,
without the old INT3 sentinel. All 46 custom fault cases are excluded explicitly.
The reviewed scalar correction is still required; original-native differences
remain preserved. This bounded matrix is not full P3 coverage.

Normal host-memory lifecycle passes staged native and all four managed forms at
`host-memory/attempt-16k81wpo`; normal file/stream callbacks pass native and all
four forms at `host-io/attempt-f61wdrei`. Both use the current compiler. Historical
injected/invalid-operation cases are excluded explicitly; these standalone
boundary checks do not establish all guest memory algorithms or service startup.

Valid explicit TLS runtime startup passes Linux/native CLI assertions and the
native-adapter/four-managed-form state matrix at `tls-loading/attempt-4arxvilg`.
The loaded PT_TLS header, private RW/NX runtime block, ARCH_SET_FS, initial/zero
TLS values, update/sum, exact 25 instructions and normal exit are checked. This
fixed positive-offset layout does not qualify a general libc/dynamic TLS ABI.

SQLite's owning consumer passes all four current-compiler forms at
`sqlite-regression/attempt-71n3t9tf`. Five non-injection C corpora pass native and
all 20 managed results at `sqlite-corpora/attempt-f3xzr2_h`; allocation/VFS mixed
injection suites are excluded explicitly, not counted as passes.

The first appended CPU expansion exposed three defined hardware mismatches
in original/scalar-staged native Blink (`cpu-conformance/attempt-cjel3vz8`): INC
auxiliary carry and CMPXCHG8B nonmatch upper-register clearing. The reviewed
integer source correction plus INC8/16 coverage now passes all 468 normal native
cases (`cpu-conformance/attempt-lc9j96ag`). Delivery `translation/attempt-w78n0i5u`
incorporates these corrections, with 107 verified reused objects and two fresh
emissions. Expanded managed CPU qualification passes at
`cpu-conformance-managed/attempt-sgren8zu`: 468 cases in each of four forms,
1,872 total, with exact corrected canonical integer producer proof. B033 is
closed for these cases. The 46 historical custom fault cases remain excluded.
Older 449-case and pre-correction core/consumer receipts qualify only their
recorded inputs.

Scoped picotls passes at `picotls-regression/attempt-fx7grn1c`: pinned native
upstream oracle, fresh translation, copied-source/92-field ABI checks in all four
forms and 220 normal/authentication peer cases. One exact forced-protocol call
and mixed custom-injection targets are excluded explicitly. This is a partial
current regression, not the old full-campaign/dependency-audit pass.

## Historical qualification ledger

| Surface | Observed result | Reproduce |
| --- | --- | --- |
| Immutable source inputs | Archive/file/license hashes and offline verification pass | `scripts/fetch.sh --offline` |
| Native Blink interpreter | JIT/linear-memory-disabled build; 25 upstream assembly cases agree with direct Linux | `scripts/native-oracle.sh` |
| Native static musl HTTP guest | Six exact-wire request cases pass on direct Linux and native Blink; 18 guest syscall names observed | `scripts/build-guest.sh`, `scripts/test-native-service.py`, repeat with `--blink build/native/blink` |
| Native archive/link audit | Relink byte-identical; 89 of 147 archive members, 185 dynamic imports | `scripts/audit-native-dependencies.py` |
| Actual upstream decoder | Native/staged/raw JIT/raw AOT/optimized JIT/optimized AOT outputs agree for 11 encodings and ABI fields | `scripts/probe-decoder.sh` |
| Actual interpreter embedding | Native arithmetic, memory/faults, seven-step budget, exit and exit_group traps pass twice | `scripts/probe-core.sh --native-only` |
| Actual core ABI/register storage | 236 outputs match native profile in raw/optimized JIT/AOT, separate consumer and whole-library AOT roots | `tests/CoreAbi/run.py` |
| Namespace / assignment-guard repairs | Warning-free build; 2225 unit pass; 505 functional pass, 1027 skipped | `scripts/test-repository.sh` |
| Negated jump / nested string repairs | Warning-free build; 2228 unit pass; 507 functional pass, 1031 skipped | `scripts/test-repository.sh` |
| Inferred outer-array dimensions | Warning-free build; 2231 unit pass; 508 functional pass, 1033 skipped | `scripts/test-repository.sh` |
| Borrowed va_list formatting | Native/four-mode formatter matches; build clean, 2234 unit/509 functional pass, 1035 skipped | `scripts/test-repository.sh` and `artifacts/vsnprintf/attempt-cxeunj4b/receipt.json` |
| Declaration order / external pointer ownership | Build clean; 2235 unit/514 functional pass, 1037 skipped | `scripts/test-repository.sh` |
| Managed interpreter embedding | Native/configured ABI and instruction/fault/exit rows match raw/optimized JIT/AOT; all 109 sources linked and AOT rooted | `tests/CoreExecution/run.py`; receipt attempt-n9ligxbc |
| Private memory filesystem | Four independent assertion groups pass Linux JIT/AOT; guest callback integration pending | `scripts/test-host-files.sh` |
| Private TCP namespace | Four assertion groups pass Linux JIT/AOT, including real backpressure/cancellation and same guest port in two instances | `scripts/test-host-sockets.sh` |
| Unified instance I/O | Four groups pass Linux JIT/AOT: common fd limits, dup lifetimes, bounded streams and disposal | `scripts/test-instance-io.sh` |
| Host storage ABI | 202 measurements match native in raw/optimized JIT/AOT; timer14, resource28 and times6 additional rows pass | `tests/HostAbi/run-managed.py` |
| Signal-aware virtual mask jumps | Native POSIX/virtual adapter agree with raw/optimized JIT/AOT; host OS mask unchanged | `tests/HostSignals/run.py` |
| Unmanaged ordinary jumps | Native output matches raw/optimized JIT/AOT with forced compacting GC | `tests/JumpStorage/run.py` |
| Jump / function parameter / array typedef repairs | Build pass; 2222 unit pass; 500 functional pass, 1025 skipped | `scripts/test-repository.sh` |
| Bounded anonymous host memory | Native staged InitMap/allocator and raw/optimized JIT/AOT agree; staged actual native core also passes | `tests/HostMemory/run.py` |
| Owned diagnostic reads / explicit byte order | Native untouched/staged and four managed modes agree; independent actual upstream load/store byte checks pass | `tests/HostMemory/run-diagnostic.py` |
| Authored clock / entropy callbacks | Native C invariants and four managed modes pass, including injected providers and failures | `tests/HostEnvironment/run.py` |
| Private file/vector C callbacks | Native C and four managed modes pass; 128 KiB bytes, sparse files, captured streams and independent workers | `tests/HostIo/run.py` |
| Translated C TCP callbacks | Native C and four managed modes pass; real clients, two private ports, exact 128 KiB response and canceled accept | `tests/HostNetwork/run.py` |
| CPUID exclusions | 16 queries match four managed modes; eight native configurations and seven native instruction probes pass | `tests/HostCpu/run.py` |
| Terminal storage declarations | 11 native rows match raw/optimized JIT/AOT; ioctl remains isolated/unimplemented | `tests/HostAbi/run-managed.py --ioctl` |
| Private stat metadata | Native layout/invariants and four managed modes pass; old file/descriptor regressions pass | `tests/HostFileMetadata/run.py` |
| Private poll readiness | Native and four managed modes pass; real TCP, timeout, FIN/HUP, close and infinite-empty disposal | `tests/HostReadiness/run.py` |
| Private process identity | Native C invariants and four managed modes pass; private IDs, errno and concurrent owners | `tests/HostIdentity/run.py` |
| Private openat / fcntl | Native/four-mode control and metadata pass, native exec oracle and existing fd regressions pass | `tests/HostFileControl/run.py` |
| Unexpected host termination | Native direct/indirect kind/status and four managed modes pass; controllers survive | `tests/HostTermination/run.py` |
| Managed CPU corpus | Reviewed scalar correction and narrowed CPUID profile: 495 cases pass each raw/optimized JIT/AOT, 1980 comparisons; untouched upstream failures retained | `tests/CpuConformance/run-managed.py --staged-fp`; receipt attempt-jwuzr1go |
| Valid service ELF loading | Exact file/BSS/permissions/stack state and cleanup match native in all four forms | `tests/ElfLoading/run.py`; receipt attempt-e1zyczdz |
| Managed controller protocol | Actual subprocess fixtures pass JIT/AOT, including bounded stop and inherited-pipe drains | `tests/InstanceLifecycle/run.py`; receipt attempt-p4b8juxr |
| Managed service worker | Automatically blocked during implementation; partial files uncompiled and unqualified | P4/P5 open |
| Clean-checkout reproduction | Fresh compiler/native25 and all109 objects with zero reuse; all four core forms, ABI/direct IL and publication pass | `scripts/test-clean-reproduction.py`; attempt-6pfwe3d4; shared SDK/NuGet, not hermetic |
| Fixed-loop interpreter throughput | All 75 samples / 75 million instructions pass exact semantics; five modes, fixed warmup, isolated campaign timing window | `tests/CoreThroughput/run.py --prepare-only`, then `--measure-existing`; attempt-h4zo2vxu |
| Fresh SQLite consumer | Raw/optimized JIT/AOT SQL, WAL, JSONB/FTS5, callbacks and GC pass | `scripts/test-sqlite-regression.py`; attempt-1nqhp4wq |
| Fresh SQLite seven C corpora | Native and all four managed forms pass core/API/VFS/vtable/allocation/upstream/FTS5, 28 managed runs with exact transcripts | `scripts/test-sqlite-corpora.py --cache <verified-archives>`; attempt-k83q6spa |
| Fresh picotls campaign | Full raw/optimized JIT/AOT suites and 224 peer executions pass | `scripts/test-picotls-regression.py`; attempt-ymi5de8u |
| Fresh MsQuic product/public consumer | Native host/public ABI, rooted builds and 32 public transport/authentication cases pass across four forms | `scripts/test-msquic-regression.py`, then `--finish <attempt>`; attempt-m_bkw5xt |
| Fresh Lua/chibi conformance | Lua user-test final success; chibi 1225/1225 and 18/18 with native-baseline transcript match; JIT only | `scripts/test-language-regressions.py`; attempt-vqeg8yjo |
| WAT execution regression | 146 oracle cases pass using wat2wasm and Node | `DOTCC_RUN_WAT=1 dotnet test DotCC.FunctionalTests -c Release --no-build --filter FullyQualifiedName~WatOracleTests`; attempt-4uflu_5h |
| Zig execution oracle | All 205 cases pass, zero skips, using CI-pinned Zig 0.16.0 and its real standard library | `scripts/test-zig-regression.py --offline`; attempt-lmenyhyn |
| Repository baseline | Build pass; 2218 unit pass; 490 functional pass, 1009 skipped | `scripts/test-repository.sh` |
| Bit-field repair regressions | Build pass; 2218 unit pass; 491 functional pass, 1011 skipped | `scripts/test-repository.sh` |
| Array parameter / tagged return repairs | Build pass; 2218 unit pass; 494 functional pass, 1017 skipped | `scripts/test-repository.sh` |

The decoder verifies actual `XedMachineMode` byte storage, `XedOperands` and
`XedDecodedInst` sizes, `op` offset, decoded instruction length, packed dispatch
encoding, immediate and displacement. Encodings cover NOP, immediate MOV,
register ADD, REP MOVSB, SSE XOR, SYSCALL, UD2, SIB/displacement addressing,
short backward branch, sign-extended immediate and overlength prefixes. The
case set is intentionally small; no execution/flags/CPUID compatibility claim
follows. Current common output SHA-256:
`de5ecc0473fd388b9455569d8b80b23d4ab57e452ba34352903690548da54c92`.

Decoder receipts record compiler/config/adapter/source/generated hashes under
`artifacts/decoder/receipt.json`. The raw generated snapshot remains separate
from the normal semantic postprocessor output. The postprocessor changed 129
boolean-conversion calls, 18 boolean comparisons and four standalone empty
blocks in the observed run; final output comparison remained exact.

Native core layout under the measured native profile is `Machine=22432`,
`System=3016`, `ax=24`, `ip=0`, `flags=12`, `onhalt=1264` bytes. This is not an
emitted-layout result. Core translation attempts retain diagnostic/history and
source/compiler/profile hashes; host profile storage checks do not implement
callbacks or make native process services safe for managed execution.

At this historical point P0–P2 had passed and P3–P6 remained open. The latest
ledger above supersedes the old CPU/ELF/memory status; actual service startup,
worker lifecycle, Windows and remaining P6 qualification remain open. The dependent-campaign
regression item has historical passing Linux evidence below and requires refresh
on the final shared compiler under the current test scope. Whole-library-rooted
AOT has passed for the complete selected core on Linux x64. The service-worker
task was stopped by automated review and remains unqualified. Malformed-ELF
handling and qualification are excluded by explicit user direction; historical
attempts remain preserved and are not counted as passes.

## Actual complete-core Linux x64 gate

P1/P2 now pass using core-execution/attempt-ny3j_02m: all109 objects link,
raw/optimized JIT/NativeAOT match the native bounded instruction/fault/exit
witness and configured ABI probe; AOT roots the complete library. Independent
raw/optimized direct IL audits report zero traversed native imports with their
limitations preserved in DEPENDENCIES.md. This historical receipt predates the
selected P3 completion recorded at the top of this ledger.

The final shared compiler also freshly regenerated SQLite's owning consumer:
raw/optimized JIT/NativeAOT pass at sqlite-regression/attempt-1nqhp4wq, including
WAL, SQL/JSONB/FTS5, callbacks, GC and cleanup. Reproduce with
`scripts/test-sqlite-regression.py` after the pinned SQLite reference is present.
Other dependent-campaign and platform rows remain open.

Fresh picotls regeneration also passes its complete Linux x64 raw/optimized
JIT/NativeAOT campaign, including 224 independent peer executions and zero
dependency-audit violations: picotls-regression/attempt-ymi5de8u. Reproduce with
`scripts/test-picotls-regression.py --cache <verified-picotls-archive-cache>`.
The shared compiler and tracked picotls inputs remained unchanged.

The fresh complete profile with reviewed scalar FP and current HostMemory is
`generated/core-profile/attempt-ngy_l10p`: all 109 sources emitted without object
reuse. Its latest execution, independent storage and publication receipts are
respectively `core-execution/attempt-yufxocze`, `core-abi/attempt-mlv4tdgf` and
`publication-audit/attempt-tvcys8m0`. Direct IL limitations remain unchanged.

The current canonical profile is `generated/core-profile/attempt-7i4_ajz4`,
which retains 108 verified objects from that complete emission and rebuilds
cpuid.c with the narrowed advertisement policy. Core execution and exact AOT
publication pass at `core-execution/attempt-n9ligxbc` and
`publication-audit/attempt-803iuwqv`; CPU qualification passes 495 cases per form
at `cpu-conformance-managed/attempt-jwuzr1go`. This does not qualify unadvertised
handlers or exhaust the remaining baseline instruction families.

A separate empty detached checkout at717ba66 reproduces the complete core from
only the pinned source archive plus installed tools/package restore. All109
objects emit afresh with zero reuse; `core-execution/attempt-xo_zqqig` passes all
four forms and `publication-audit/attempt-ciqxzgs4` passes exact executed binaries.
The identity chain is `clean-reproduction/attempt-6pfwe3d4/receipt.json`. This
qualifies clean generation on this Linux host, not full runtime dependency
resolution, hermeticity, Windows or translated-service behavior.

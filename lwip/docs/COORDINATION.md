# lwIP implementation coordination

Updated: 2026-09-29. Complete scope: [PLAN.md](PLAN.md), baseline v2.
Status: active; no qualification milestone has passed yet.

## Isolation and ownership

All campaign mutations and builds run in `/home/marius/p/dotcc-lwip-full`, branch
`campaign/lwip-full`, based on `cbbef56d`. The separate libuv session owns its work
in `/home/marius/p/dotcc`; do not modify, build, stage, reset, clean or switch the
original checkout. Do not modify sibling SharpAstro sources. Shared changes made
here must be reported before eventual integration; do not push or merge into the
original branch. Use `-p:UseLocalLalrCc=false` for tool/runtime builds.

| Agent | Ownership | Current task |
| --- | --- | --- |
| `/root/lwip_coordinator` | This ledger, scope/contract review, integration and validation scheduling | Coordinate P0–P6 and retain evidence |
| `/root/lwip_coordinator/pipeline` | `config/`, `scripts/`, `src/port/` headers, `docs/source.md`, `docs/blockers.md`, `tests/Upstream/`; approved structural compiler fixes | Source/feature/profile inventory, recipe, full translation |
| `/root/lwip_coordinator/host` | `src/Host/`, `src/Managed.Lwip/`, `samples/`, `tests/` except `Upstream/`, README and `docs/host.md` | sys_arch, owning API, native oracle and consumer |

Current shared .NET build ownership is **pipeline**, returned after host receipt
`20260929-075904-16948989`. Pipeline runs locking/auxiliary translation and
QuickJS downstream regression; host authors direct API/lifecycle tests until the
next finite build window. Host may use gcc or
standalone projects with private output directories; shared bin/obj builds require
coordination. Do not kill unrelated build processes. Workers implement code;
coordinator reviews, schedules and integrates. Commit only named owned files after
relevant validation, with milestone scope stated honestly.

## Agreed constraints and contracts

- Include every implemented bundled feature without external product libraries,
  including PPP's bundled crypto. Exclude TLS and externally provided crypto.
- Use original configuration/hooks first, guarded macro/typed overrides next,
  narrowly justified staged text changes only if those are insufficient.
- Product networking algorithms and socket behavior remain translated lwIP.
- Proposed sys_arch ABI: `intptr_t` semaphore/mailbox/mutex/thread handles,
  `uintptr_t sys_prot_t`, zero invalid handles; u32 clocks. Host and pipeline are
  freezing exact declarations and bindings together.
- Actual translated `tcpip_thread`, per-owner state, explicit owner callbacks,
  bounded queues, correct NULL messages, real waits and joined shutdown required.
- Missing suites and external prerequisites remain incomplete, never passes.

## Milestones and evidence

| Gate | State | Evidence / next action |
| --- | --- | --- |
| P0 inventory/native baseline | In progress | Canonical layout passes; 119 stack source units and 324 contrib files initially inventoried; first full translation attempted, recipe fixes and retry underway; native controls pending |
| P1 ABI/host/ownership | Unrun | Native and emitted boundary probes follow first translation |
| P2 all product/diagnostic/auxiliary builds | Unrun | Full translation starts after recipe and bindings |
| P3a first useful workflow | Unrun | Two owners, 256 KiB dual-stack TCP and UDP, raw/netconn/sockets |
| P3b core/link features | Unrun | Addressing/routing/SLIP/PPP/6LoWPAN/ZEP including bundled PPP modes |
| P3c applications/contrib/tool | Unrun | Every included family has real behavior cases |
| P4 behavioral matrix | Unrun | Upstream/error/recovery and independent peer cases |
| P5 consumer delivery | Unrun | Ordinary references and rooted/trimmed AOT |
| P6 complete reproduction | Unrun | Final regeneration, full evidence and portable orchestration |

Read-only environment check: .NET SDK 10.0.111; GCC 13.3.0. `pkg-config
--modversion check` cannot find Check; native upstream Check qualification needs a
campaign-local test-only acquisition or available system installation. `/dev/net/tun`
exists, but no provisioned isolated TAP link is visible (`ip -brief link` lists
lo, physical interfaces, virbr0 and docker0). Independent kernel peer gate remains
an external prerequisite; do not alter host routes or run sudo. Continue all
independent implementation and in-memory qualification work.

## Restart

```bash
cd /home/marius/p/dotcc-lwip-full
git status --short
cat lwip/docs/COORDINATION.md
cat lwip/docs/blockers.md
bash Scripts/campaign.sh layout lwip --check
bash lwip/scripts/translate.sh --fetch never --profile threaded
```

The last two commands become available when the initial recipe/layout is created;
there are no successful translation receipts yet. Preserve references and authored
files when retrying. Pipeline reports exact first-failure command and receipt in
`docs/blockers.md`. Consult running agents before starting a shared-output build.

## Initial translation attempts

- `20260929-073140-6a993d0a`: tool builds passed, full selected source attempt
  rejected object emission `--runtime=c`; actual CLI accepts runtime selection at
  link time for objects. Recipe now applies it at linking. This corrects the
  planned emission flag without changing instance-method ownership.
- Source parser rejected GNU postfix packed tag syntax in initial configuration.
  Upstream's existing `PACK_STRUCT_USE_INCLUDES` port mechanism selects paired
  packing pragmas; no upstream text changes. Native/emitted ABI proof pending.
- `20260929-073309-d49260fa`: full retry after configuration and typed override
  primitive signature corrections. Read its receipt for the actual latest state.

Host synchronization/registry/lease implementation is in progress. Review requires
worker `__DotCcEnter` binding in addition to retain, real lifecycle quiescence,
and careful shutdown at the translated execution boundary. A cancellation design
is under review; no lifetime or thread gate has passed.

### Full stack emission reached

`20260929-073515-2b65b35b` emitted and linked all 119 selected stack units plus
the authored bridge. Raw C# build failed: 57 conditional conversion overload
ambiguities, 26 fd_set layout/name conflicts, 6 ENOSYS name failures, plus missing
application callbacks. Pipeline is authorized to reduce and structurally repair
shared compiler/runtime causes with focused regressions, then retry complete
inputs. Host owns missing genuine application callbacks and native controls.
No product has been published; P0 native and P1 behavioral gates remain open.

Standalone host witness: 17 checks passed, explicitly a simulated owner harness,
not actual translated lifecycle qualification. Normal TCP/IP shutdown now uses
a queued callback marker and translation-unit-scoped existing ALIVE hook that
releases core locking before returning; actual emitted shutdown not yet run.

### Shared repairs and native control

Shared fixes in the isolated branch: arithmetic conditional emission preserves
its C common type (including nested CBool-rendered comparisons); runtime errno
header/library/literal pool now provide Linux ENOSYS 38 and its message. Existing
`ternary-cmp-arm` and `protocol-errno` fixtures expanded. Focused compiler 14 and
affected functional 6 passed. Full retry `20260929-074005-9a97de31` confirms all
Cond.B/fd_set/ENOSYS build errors gone; remaining application callbacks were added
after that snapshot and a fresh full retry is running. Backend changes overlap a
file in libuv's original checkout, so parent has been notified for future merge.

Native `nosys` oracle passes 256 KiB TCP + 123-byte UDP through both IPv4 and IPv6
loopback, ABI observations and intentional assertion failure (exit 92). Report:
`build/native/nosys/report.json`. These are initial native loopback controls, not
two-owner Ethernet/managed/independent peer proof. Threaded native follows.

### Default product publication

Default `threaded` raw and processed projects now published transactionally at
canonical generated paths. Current pointer: `artifacts/campaign/current-threaded.json`.
Publication builds establish source/build closure for that snapshot only; authored
frame bridge is evolving and requires fresh generation. `nosys` and locking profile
builds follow, with explicit shared-build windows for host integration.

Native threaded oracle also passes dual-stack socket TCP 256 KiB / UDP 123-byte
loopback and ABI/negative control (`build/native/threaded/report.json`). No managed
workflow has yet been qualified. The specialist dispatcher deliberately fails
all unimplemented required suites rather than treating bootstrap consumer as pass.

Native raw, threaded and threaded-locking initial ABI/TCP/UDP loopback controls
have passed, with preserved intentional negative controls. Native reports remain
under `build/native/<profile>/report.json`. Default latest refreshed product:
`20260929-074546-54e9628a`; nosys: `20260929-074353-3cdcb56a`. Host now owns the
shared .NET build window for actual owner/frame/consumer and JIT/AOT integration.
Pipeline works on notices, profile/host-binding audit and auxiliary products.

### First useful managed evidence

Processed threaded JIT and processed nosys JIT now execute two generated owners
connected through bounded, copied Ethernet queues: IPv4/IPv6 TCP 256 KiB echo and
123-byte UDP payloads. Formal raw/processed × JIT/AOT consumer receipts are being
collected; actual emitted ABI probe compares native observations per profile.
Consumer continuation disposes A then exercises B using a fresh port (reusing the
closed server port correctly encountered TIME_WAIT; harness corrected, product
unchanged). Deeper pending protocol shutdown and feature suites remain open.

### Necessary upstream tool repair

Pinned makefsdata `-c` never increments local `src_offset`, causing nonempty input
to emit checksum rows indefinitely natively. Coordinator inspected exact source
and authorized minimal staged loop progress repair. `config/adaptations.json`
records unique local context, why configuration/hooks/typed overrides cannot
express the local change without replacing the algorithm, and required bounded
native/managed empty/odd/multi-chunk checksum tests. Reference remains untouched.
Native tool harness imposes output-size and wall-clock bounds. External compression
libraries remain excluded.

### Consumer matrices and next ownership

Default threaded consumer all four raw/processed × JIT/NativeAOT cells pass:
`artifacts/campaign/20260929-075129-14a2b48d/consumer-cases.json`.
Main-loop nosys consumer all four cells pass:
`artifacts/campaign/20260929-075457-7e0edbef/consumer-cases.json`.
Both include dispose-A/continue-B witness; registry zero after final reclamation
is not a claim of full pending-protocol cleanup. Nosys earlier continuation
failures were harness connect/listen port mismatches, now corrected with fresh
port22345; no product behavior was weakened. Latest harness failure cleanup
hardening is undergoing a current-output rerun.

Host is running actual generated ABI and owner-bound sys_arch four-cell suites,
then rooted AOT, before handing .NET build ownership back to pipeline. Pipeline
also owns `tests/Upstream/` now, preparing Check-compatible/native Check assertions
and fixtures in addition to tool/contrib profiles. Native tool repair checks pass
bounded empty/odd/multi-chunk offsets/data/checksum validation; managed remains
unrun. Full qualification scope, locking/diagnostic/auxiliary builds and all
link/application/contrib/upstream/error/reproduction gates remain open.

### Current boundary evidence (2026-09-29)

- `20260929-075553-7edc1797`: threaded actual emitted ABI initial 10 observations
  and actual-owner sys_arch 24 checks pass raw/processed × JIT/NativeAOT. The
  host contract test uses a real generated owner; actual translated worker is
  separately exercised by consumer.
- `20260929-075656-c2e46bad`: latest hardened nosys consumer and ABI pass all four
  cells, after explicit exactly-once pbuf release and failure-path context drain.
- `20260929-075750-7fe1bb64`: threaded whole-library rooted NativeAOT passes both
  raw and processed forms, distinct from the trimmed consumer acceptance above.

Host is finishing extended emitted/native socket/fd_set/netbuf layout and
odd-address/split-pbuf checksum ABI probes, then gives build ownership back to
pipeline. Pipeline next: locking profile, impact-selected QuickJS downstream
regression, tool/contrib and diagnostic builds. Significant validated initial
milestone commit follows downstream validation, with all remaining gates open.

Extended ABI `20260929-075904-16948989` passes all four threaded cells: 19 actual
emitted core/socket/fd_set/netbuf size/offset observations plus odd-address,
odd-length and split-pbuf checksum comparisons with the native oracle. Build
ownership returned to pipeline, which started locking profile translation
`20260929-080016-cd2be273`.

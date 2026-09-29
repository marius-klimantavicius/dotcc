# lwIP implementation coordination

Updated: 2026-09-29. Complete scope: [PLAN.md](PLAN.md), baseline v2.
Status: active; core and upstream matrices have passed; full P0–P6 acceptance remains incomplete.

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

Current shared .NET build ownership is **pipeline**, handed back after refreshed
threaded independent matrix `085113-333ba145`. It regenerates product profiles
with the native-proven forwarding repair and runs its finite contrib/link cases.
Host finishes native routing controls and authors broader addressing/routing and
owning service lifecycle work. No simultaneous shared bin/obj builds. Workers
implement; coordinator reviews, schedules and integrates. Commit only named
validated files, stating partial milestone scope honestly.

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
| P0 inventory/native baseline | Partial | Canonical layout, full source inventory, three native product controls and native Check pass; active-feature audit remains |
| P1 ABI/host/ownership | Partial | Emitted/native ABI and actual-owner sys_arch matrices pass; pending-resource teardown remains |
| P2 all product/diagnostic/auxiliary builds | Partial | Three product, unit and makefsdata raw/processed builds published; unit-nosys and contrib follow |
| P3a first useful workflow | Partial | Raw/socket consumer and direct netconn/altcp matrices pass; current locking direct-API rerun underway |
| P3b core/link features | Unrun | Addressing/routing/SLIP/PPP/6LoWPAN/ZEP including bundled PPP modes |
| P3c applications/contrib/tool | In progress | Native makefsdata repair verified; managed tool and broader apps/contrib follow |
| P4 behavioral matrix | Partial | Original unit137 passes all four cells; remaining diagnostics/features/error/recovery/independent peers open |
| P5 consumer delivery | Partial | Ordinary references and trimmed/rooted AOT established; full feature delivery pending |
| P6 complete reproduction | Unrun | Final regeneration, full evidence and portable orchestration |

Environment: .NET SDK 10.0.111; GCC 13.3.0. Native Check 0.15.2 is
acquired and built as a campaign-local test-only dependency. Unprivileged
user/network namespace creation and temporary TAP provisioning both succeed;
independent kernel interoperability is implementation work, not a current
external blocker. Never alter host routes/interfaces or run sudo.

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

### Independent peer provisioning feasibility

The initial missing TAP is not currently an external blocker: both
`unshare --user --map-root-user --net true` and
`unshare --user --map-root-user --net ip tuntap add dev lwip-probe mode tap`
succeeded. The probe network namespace and its link disappear when its process
exits; no host routes/interfaces were changed. An independent-peer runner can
provision a temporary user/network namespace and test-only TAP, then run peers
inside it without sudo or interfering with libuv. Actual interop remains unrun.

### Committed checkpoints

- `537374c6`: preserve C arithmetic common type through nested conditionals.
- `a0347b86`: shared ENOSYS definitions and message.
- `b9ae0ce2`: initial isolated lwIP owning stack/consumer/boundary campaign.

Shared fixes passed focused compiler/functional checks and isolated QuickJS
raw/processed translation (`080236-2f0ecda6`) and JIT consumers
(`080433-b0e54d3a`). Initial campaign checkpoint records qualified scope and open
acceptance honestly; new direct API/packet tests remained in-flight outside it.

Native original upstream Check 137/137 passes, with explicit negative/fixture
control. Diagnostic implicit-memset callback-guard failure was a compatible test
header missing Check's transitive string.h include, not a compiler defect; the
harness header is corrected and full diagnostic retries continue.

### Original managed upstream matrix

Original upstream normal diagnostic profile passes 137/137 cases in all four
raw/processed × JIT/NativeAOT cells, with exact native suite/test registration
equality. Deliberate negative Check control matches native exit 1, setup 3,
body 2, teardown 2 and failure 2 counts. Native skips teardown after failed
setup; managed runner preserves that behavior. Receipts:
`20260929-081512-9608394d` (JIT) and `20260929-081538-a7d37662` (AOT).
Unit-nosys native/managed applicability and remaining feature suites still open.

Implementation streams after current windows: host owns core/socket/datagram,
addressing/routing/lifecycle/recovery and independent peer; pipeline owns
upstream/auxiliary products, then links/PPP and applications. Host remains primary
owner of shared authored Host code; pipeline coordinates extensions directly.


### Direct API and datagram matrices

`20260929-082120-9e746efd` (threaded) and `20260929-082457-04b31050`
(nosys) pass all four raw/processed × JIT/NativeAOT direct-apis and
datagram-fixtures cells. Direct cases exercise plain altcp and, when applicable,
netconn. Datagrams cover 4500-byte IPv4/IPv6 fragmentation plus UDP-Lite partial
coverage acceptance/rejection and recovery. ARP_QUEUEING=0 fixtures resolve a
neighbor first; this does not claim pending-neighbor queue behavior coverage.
Locking matrix is running. Full socket-api/packets gates remain open.

Pipeline framework profile-preparation tests pass 37/37 and recipe dispatch 3/3;
review identified a redundant default-profile verify translation being removed
before committing the shared change. Host is preparing stronger identical-port
owner isolation with GC, live B endpoints after A disposal and wraparound timer.

Locking direct-apis/datagram-fixtures all four cells also pass
`20260929-082706-a7b10f5a`, completing these finite suites across all three
product profiles. Pipeline now owns the .NET window for unit-nosys/tools/contrib.
Shared framework commit `d48ffc06` passes 38/38 framework and 3/3 recipe tests;
full verify prepares each selected profile exactly once.

### Active compile-time variant audit

Pipeline has enabled `ARP_QUEUEING=1`, `ARP_QUEUE_LEN=8` and `LQR_SUPPORT=1`
in threaded-locking. These expose bundled working branches omitted by defaults;
LQR means only the upstream-provided LCP quality negotiation. Existing locking
receipts describe the prior snapshot and do not qualify this changed profile.
Regeneration and affected matrices will follow the current unit/tool/contrib
window. Host will distinguish default queue-drop behavior from queued-neighbor
behavior in the packet fixtures.

Latest feature milestone is `2c91bd16` (direct APIs/datagram matrices); shared
framework milestone is `d48ffc06` (verify-only profile preparation).

### NO_SYS original upstream matrix

Unit-nosys translation `20260929-082901-59a31d59` and all four raw/processed ×
JIT/NativeAOT cells `20260929-083055-ad449f5f` pass 132 original applicable cases
with native registration and negative fixture parity. Five threaded socket cases
are inapplicable; the test-only adapter preserves the original zero-case NO_SYS
socket suite and real poll/timeout pumping. This is not five stubbed successes.
Both diagnostic upstream matrices now pass. Managed makefsdata is running next.

Review identified ancillary worker shutdown ordering: stopping only tcpip then
joining all workers before signalling ancillary waits cannot terminate a live
SNMP/SLIP/contrib worker. Host owns coordinated cancellation/cleanup before those
feature families can qualify. No resource-registry clear may substitute for this.

### Managed makefsdata generator matrix

`20260929-083157-23870a49` passes all four generator form/runtime cells with
a 27,529-byte native-exact image and independently verified empty/odd/multiple-MSS
checksum, header-offset and temporary-file controls. The staged upstream progress
repair now has both native and managed evidence. Translated HTTP consumption of
generated content remains required, so this is generator qualification rather
than closure of the complete tools/application roundtrip gate. Contrib 136-unit
full translation is running.

Active-body auditing exposed a material parent-option error: the stateless
DHCPv6 option was enabled but `LWIP_IPV6_DHCP6` remained its default 0, so
`dhcp6.c` had no active implementation. Pipeline now enables the correct parent
in all product/contrib profiles and removes the unused `LWIP_DHCP6` typo. All
affected product/native/ABI/runtime evidence must be refreshed after regeneration.
Contrib TCP ISN had a similar whole-body guard issue: defining the documented
`LWIP_HOOK_TCP_ISN` in the profile makes its implementation active before the
core hook header is included. No upstream patch or compiler change is needed.

Disk headroom fell to 27 GiB during concurrent sessions. With host confirming no
writer, coordinator removed only obsolete `lwip/build/managed-consumer`
intermediate caches (about 3.3 GiB). All campaign app outputs, reports, PCAPs,
receipts, native controls, reference sources and published products remain.
Upcoming .NET tests recreate their caches; no other worktree was touched.

### Native IPv4 forwarding defect and repair contract

The new native routing fixture fails valid forwarded IPv4 header checksum before
translation: pinned `ip4_forward` clears IP/UDP/TCP/ICMP checksums when software
generation is enabled, using ingress-interface state. Hooks cannot express a
local correction; replacing the whole function would replace the algorithm.
Coordinator authorized four narrowly staged predicate corrections, preserving the
original incremental IP checksum logic. Correct offload condition must include
`!CHECKSUM_GEN_* || (LWIP_CHECKSUM_CTRL_PER_NETIF &&
!NETIF_CHECKSUM_ENABLED(netif, NETIF_CHECKSUM_GEN_*))`: the macro expands 0 when
per-netif control is disabled, so naive negation is incorrect.

Apply only in staged sources, with four unique exact local anchors mandatory in
every hash mode; whole-file hashes remain provenance metadata governed by policy.
Reference source is untouched. Central `stage.py` and `adaptations.json` own the
repair contract. Native unmodified failure plus adapted software forwarding,
fragment payload preservation and later managed matrix must pass. Hardware
checksum-offload qualification is not claimed. Current product snapshots precede
this repair.

### Refreshed threaded behavior checkpoint

`20260929-085113-333ba145` passes all four raw/processed × JIT/NativeAOT cells:
owner-lifecycle overlapping addresses/ports/descriptors, GC and B existing endpoint
after A disposal, wraparound timer; socket options/select/poll/partial reads,
half-close and actually blocked UDP receive/concurrent close; DNS A/AAAA/cache/
NXDOMAIN; updated emitted/native netif 584 ABI and 256 KiB consumer. These are
finite suites; complete socket/addressing/lifecycle gates remain open.

Native routing original-fail/adapted-pass pairs now pass threaded and nosys, with
nonzero UDP/TCP/ICMP checksum preservation and first/nonfirst fragment payload
controls. Locking native and all managed routing cells follow. Three staging
guard checks include acceptance of unrelated local source edits; whole-file hash
is not an unconditional repair gate.

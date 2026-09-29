# lwIP implementation coordination

Updated: 2026-09-29. Complete scope: [PLAN.md](PLAN.md), baseline v2.
Status: active; core and upstream matrices have passed; full P0–P6 acceptance remains incomplete.

## Active workspace and ownership

All future campaign mutations and builds run in `/home/marius/p/dotcc`, current
branch `sqlite`, following the user's migration instruction after libuv stopped.
Validated lwIP and completed libuv history merged at `38c1cfa8`. Root copied all 47
unfinished paths with SHA256 verification and preserved them uncommitted/unstaged.
The old `/home/marius/p/dotcc-lwip-full` worktree and `campaign/lwip-full` branch are
historical read-only backups: do not build, edit, reset, remove or prune there.
Do not modify sibling SharpAstro sources. Do not stage unrelated untracked
`docs/plans/compiler-review-2026-09-29.md`. Use `-p:UseLocalLalrCc=false` for tool and
runtime builds. No push is authorized. Earlier isolation directions below describe
historical execution only and are superseded by this active workspace rule.

| Agent | Ownership | Current task |
| --- | --- | --- |
| `/root/lwip_coordinator_resume` | This ledger, validation index, scope/contract review and verification scheduling | Coordinate remaining P0–P6 work and retain evidence |
| `/root/lwip_coordinator_resume/permitted_implementation` | Campaign source, configuration, scripts and fixtures; sole shared build owner | Review preserved LOWPAN/ZEP work and verification policy, then implement and qualify permitted remaining behavior |
| `/root/lwip_coordinator_resume/scope_audit` | Only new unregistered `tests/Applications.Udp/` fixtures; no builds or registration | Draft next positive TFTP/SNTP workflow batch after completing read-only scope audit |

A fresh implementation worker successfully started after normal agent capacity
became available. The former coordinator and workers are no longer in the active
team; their preserved work and receipts remain intact. The previous generic
service errors identified no specific prohibited action. Neither those errors nor
replacement availability authorizes bypassing a safety rejection: any identified
prohibited action must stop and be reported.

Current shared .NET build ownership is **permitted_implementation**, after its
initial preserved-source/policy review is acknowledged. No other agent may mutate
implementation or build outputs concurrently. Latest qualified implementation is
`537bfc3d`: owning LOWPAN/ZEP/BLE finite input/output/lifetime passes all three
product profiles and four cells, with common owning API/lifecycle/consumer
regressions. Complete option/application/contrib and final acceptance gates remain
open. Secondary application fixture drafts are unregistered and unqualified.
Current evidence: [validation.md](validation.md). Old receipts retain historical
provenance; fresh product and PPP/diagnostic records are from this checkout.
No simultaneous shared bin/obj builds. Freeze shared Host/facade source and staging/configuration helpers while the
other worker builds dependent native or managed matrices; author new isolated
fixtures meanwhile. Coordinate complete multi-file adaptation readiness before
allowing builds, since intermediate metadata/stager versions can reject controls.
Receipts from successive source snapshots do not jointly qualify the latest edits.
Workers implement; coordinator reviews,
schedules and integrates. Commit named files with evidence and honest scope.

## Agreed constraints and contracts

- Latest user verification instruction: do not perform corruption tests unless the
  exact scenario exists in a pinned upstream test. Implementation rejection paths
  alone are not provenance. Disable unsupported authored corruption in every
  invoked native/managed/prerequisite path, including full verify; retain ordinary
  non-corrupt API/authentication/lifetime/error cases and original upstream tests.
  Historical receipts remain historical, and excluded-by-user cases are neither
  passes nor blockers. The focused source/call-graph audit is complete; only
  permitted revised verification may resume. Ten revised suites now pass threaded
  raw JIT plus native prerequisites; other cells and new fixtures need fresh results.
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
| P2 all product/diagnostic/auxiliary builds | Partial | Three product, unit/unit-nosys, contrib and makefsdata raw/processed builds published; auxiliary variants/final audits remain |
| P3a first useful workflow | Partial | Raw/socket consumer and direct netconn/altcp matrices pass; full lifetime/error qualification remains |
| P3b core/link features | Partial | Finite DNS/IPv4 routing/lifecycle matrices pass; SLIP and bundled PPP initial controls pass; broader scope remains |
| P3c applications/contrib/tool | In progress | Managed generator→HTTP content and finite contrib/PPP/SLIP/HTTP matrices pass; full app/contrib workflows remain |
| P4 behavioral matrix | Partial | Original unit137/unit-nosys132 and kernel TCP/UDP all required cells pass; full feature/error/recovery/application-link peers remain |
| P5 consumer delivery | Partial | Ordinary references and trimmed/rooted AOT established; full feature delivery pending |
| P6 complete reproduction | Unrun | Final regeneration, full evidence and portable orchestration |

Environment: .NET SDK 10.0.111; GCC 13.3.0. Native Check 0.15.2 is
acquired and built as a campaign-local test-only dependency. Unprivileged
user/network namespace creation and temporary TAP provisioning both succeed;
independent kernel interoperability now passes all product matrices. There is no
current external blocker to the remaining Linux implementation. Never alter host routes/interfaces or run sudo.

## Restart

```bash
cd /home/marius/p/dotcc
git status --short
cat lwip/docs/COORDINATION.md
cat lwip/docs/blockers.md
bash Scripts/campaign.sh layout lwip --check
bash lwip/scripts/translate.sh --fetch never --profile threaded
```

The recipe/layout and translations are established. Preserve references and authored
files when retrying. Consult running agents before starting any shared-output
build. Use wrappers, not direct execution of the imported recipe module. Current
finite repaired behavior can be rerun after the build owner yields:

```bash
bash lwip/scripts/test.sh --fetch never --profile threaded --suite routing-ipv4 --suite interop-kernel --form all --mode all
bash lwip/scripts/test.sh --fetch never --profile nosys --suite routing-ipv4 --suite interop-kernel --form all --mode all
bash lwip/scripts/test.sh --fetch never --profile threaded-locking --suite routing-ipv4 --suite interop-kernel --form all --mode all
```

Following sections are chronological snapshot history; current state is above.

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

### Independent kernel peer and natural cleanup

`20260929-090320-bbf3afe9` passes threaded raw-JIT repaired IPv4 routing and
independent Linux-kernel TCP active/passive 256 KiB echo plus bidirectional UDP 123
over both families through a test-only TAP. User/network namespace provisioning
contains all interface/route changes and exits cleanly. No delivered TAP adapter
is introduced. Remaining interop matrix cells and NO_SYS raw protocol harness
are required; current socket-based finite interop fixture covers threaded modes.

Updated lifecycle observes generated pool/heap counters returning to initialized
baseline before owner disposal or registry clearing: UDP/PBUF/NETCONN/NETBUF 0,
SYS_TIMEOUT 11 and heap 0, plus host resources 2/workers 1/thread-semaphores 0. The
remaining timer/worker infrastructure is intentional initialized stack state.
Host 32 checks now cover bounded serial admission/deferred errors and explicit
service-exit boundary. Real ancillary-service teardown still requires its own
protocol-specific ownership/stop integration.

Pipeline initial managed raw-JIT controls pass contrib addons
`090220-366b245d` (native ABI, routes, extargs ownership, ISN) and bundled PPP
`090230-b19a87b0` (five primitives, MSCHAPv2 positive/tampered response, initial
MPPE key, 600 stateful/stateless packets with rollover and malformed headers).
Full auxiliary cells and actual link/PPP negotiation remain open. Latest host
fixture checkpoint is `3d65b4cc`; subsequent refinements require fresh receipts.

### Complete repaired routing/kernel matrices; next link/application window

Threaded `090503-a94d59d9`, NO_SYS `091423-b5f02ec9`, and locking
`091556-6c1b6415` all pass four cells for repaired IPv4 forwarding, independent
Linux-kernel TCP active/passive and UDP both families, and natural lifecycle.
Locking additionally passes explicit ARP_QUEUEING=1 FIFO cases, host32, socket
semantics, DNS, ABI and consumer. NO_SYS also passes DNS, ABI and consumer.
The earlier NO_SYS `091006-472a1bf0` aggregate is not a pass: its six applicable
suites passed, but a trailing inapplicable sysarch request was rejected. The clean
`091423` receipt is authoritative. Current evidence is indexed in validation.md;
older entries in this chronological ledger refer to their historical snapshots.

Build ownership has passed to pipeline for finite PPP/contrib/SLIP/HTTP matrices
and actual translated makefsdata image→HTTP roundtrip. Host authored additive
StartService ownership and reverse cleanup before core shutdown, still unbuilt.
Its prior validated facade snapshot is `build/validated-LwipStack-before-services.cs`.
Do not treat earlier lifecycle receipts as qualification of the new service API.
After pipeline hands back .NET, host must build and qualify actual ancillary
service stop/join, pool recovery, default ARP queueing and broader addressing.

### Auxiliary protocol matrices and host checkpoint

Validated host checkpoint `cae2e2ef` contains host32/serial, natural lifecycle,
routing and independent kernel fixtures plus the prior validated facade routing
surface. It depends on pipeline-owned staged forwarding repair pending its
checkpoint. New service/DHCP/PoolRecovery work stays outside this commit.

Contrib addon four-cell matrix passes `092742-40fc80aa`. Finite bundled PPP,
SLIP framing/actual UDP ingress and five baseline HTTP transactions pass all four
cells for threaded `092814-c3aef247`, nosys `092953-8a3a53fc`, locking
`093126-d3ef1043`. Initial `092312-37ea469b` aggregate failed HTTP harness CGI
teardown after SLIP passed; fixed clean HTTP `092443-6eedfe86` and the subsequent
full matrices supersede it. These protocol fixtures do not qualify newly authored
owning-service registration/disposal retry. Shared Host/facade sources are frozen
until pipeline hands builds back; host must qualify that exact final snapshot.

Generated-content raw JIT passed `092727-103cb8ef`: translated makefsdata output
fed original fs.c and HTTP served empty/odd/4097-byte files. Pipeline now repeats
preparation with portable bounded monitoring and qualifies all four HTTP-content
cells. Native PPPoS auth negotiation is in progress; postclose allocation accounting
is under investigation, not yet an upstream defect claim. Native DHCP phase
controls are being collected by host ahead of managed tests.

### Actual translated generator→HTTP roundtrip complete

Portable bounded subprocess monitoring replaces POSIX-only preparation limits.
Fresh HTTP-content publication `093414-95dafdc6` passes and all four HTTP-content
behavior cells pass `093554-eba3af0c`, serving actual translated makefsdata output
through original fs.c. Native controls use the same generated image. Full other
HTTP variants and application/contrib families remain open.

Native PPPoS postclose allocation discrepancy was queued IPv6 router-solicitation
loopback data: the fixture's NO_SYS pump omitted netif_poll_all. Actual mainloop
drain fixes accounting; no product patch. Link negotiation fixtures remain in
progress. Host has frozen shared source until build handoff and will qualify the
latest owning-service/disposal phases in its next window.

Pipeline checkpoint `b5ee80e4` commits 44 owned files covering validated diagnostic
matrices, generator/HTTP roundtrip, active-feature/binding checks, both staged
repairs and finite contrib/PPP crypto/SLIP/HTTP fixtures. Unfinished PppLinks and
new host suite names remain outside its index. This supplies the staged-repair
dependency of host checkpoint `cae2e2ef`. Completed private auxiliary build caches
were pruned by their owner (2.3 GiB), retaining apps, receipts, reports and products.

### Confirmed bundled EAP failure-path leak; staging coordination correction

Reduced native wrong-password EAP trace identifies an unfreed 536-byte RAM pbuf
allocated by eap_send_request, leaving 552 heap bytes after both PPP sessions are
DEAD, ppp_free and actual loopback drain. Positive EAP returns to baseline.
Evidence: `artifacts/native-eap-leak/before.json` and `eap-trace-symbols.log`.
Coordinator reviewed the pinned function and conditionally authorized exactly one
pbuf_free(p) before its default switch return, once this trace confirmed the path.
No upstream hook can free the local allocation; a whole-function override would
replace the authentication algorithm. Pipeline now stages this third guarded
repair for native/product EAP paths, leaves acquired references untouched and
keeps EAP-disabled diagnostic source unchanged. Adapted native and fresh managed
full auth controls are required; current published products predate this repair.

Host matrix `094136-ab75833b` failed before native AutoIP compilation because it
observed new three-repair metadata while the old stager still required two. This
is a shared-file staging race, not an AutoIP protocol failure. Pool/DHCP constituent
matrices had passed; no aggregate pass is claimed. Workers must freeze shared
staging/configuration as well as Host/facade during dependent builds. Host retries
only after pipeline confirms the complete coherent repair files are ready.

### Expanded host matrices; final pending-HTTP refinement

Threaded `094413-fe06cf88`, NO_SYS `094658-5dd39ead`, and locking
`095020-fab83601` pass all four cells for pools/recovery/owner independence,
DHCP4 DORA/ACD/options/renewal/NAK/expiry/conflict, AutoIP, ARP queue policy and
datagrams, natural lifecycle and full consumer; threaded modes include host32 and
actual blocked translated UDP receive disposal. Failed service-stop and post-marker
worker-error retry paths pass. These supersede the staging-race aggregate failure.

Final review found active HTTP context refusal must occur before core shutdown;
storage release itself remains after joins. Host adds CleanupForDisposal, available
only after a failed disposal before the marker, to release existing borrowed
resources while ordinary admissions remain closed. Actual translated fs_open →
refused disposal → explicit fs_close cleanup → successful retry will be qualified
before the new lifecycle checkpoint. Earlier matrices do not qualify this latest
refinement. Native IPv6 DAD/RA/SLAAC/DHCPv6 timer/XID/DNS and IGMP/MLD controls pass;
managed full matrices remain next-window work.

Native NO_SYS PPPoS report `artifacts/ppp-links-native/nosys/report.json` passes six
positive serial sessions and five wrong credentials, both IP control negotiations,
actual UDP and six bad-FCS cases, orderly close/free baseline after EAP repair.
Managed link-session products/tests remain pending the build handoff and refresh.

### Migration to original checkout and final host snapshot

User reported libuv stopped and directed integration into original checkout.
Both workers froze safely: host session 75246 and pipeline session 19480 exited 0;
no writes, commits or pruning during root integration. Root merged validated
history into `sqlite` at `38c1cfa8`, resolved one shared framework-test import
conflict preserving both sides, verified 47 WIP files by SHA256, copied ref/generated/
artifacts, and retained old build history only in the read-only worktree.
Migration evidence: `artifacts/migration-20260929`. Root checks passed 43 framework,
105 focused compiler, 10 functional (four optional native-oracle skips), six pipeline
checks and layout; raw/processed JIT consumer passed `100709-5e0adecd` in new root.
Copied historical receipts retain their old paths. check_product selects the new
root's generated project; no historical receipt is rewritten as fresh evidence.

Latest pre-migration host refinements: HTTP cleanup-scope plus consumer all four
cells pass threaded `095609-5894d6ee`, NO_SYS `095703-fadcd930`, locking
`095758-3e4181b3`. Final actual timer/core-context rejection and finite service/pool
recovery pass all four cells threaded `100126-d50e6381`, NO_SYS `100152-2c79deb8`,
locking `100221-287fcea6`; source snapshots are in artifacts. IPv6/multicast have
native controls only; managed smoke was deliberately not started before freeze.

Native PPP serial final controls pass all three profiles with original EAP exit92
versus adapted pass0, actual UDP/TCP data, VJ full/compressed frames, MPPE wire and
payload controls, malformed FCS, wrong credentials and cleanup baselines. The
locking fixture explicitly drives real timeout processing under core lock for the
injected clock. Native independent PPPoE NO_SYS discovery/service name/cookie/
bad HostUnique/bad PADS/LCP reject/PADT controls pass; full data and L2TP remain.
Managed PPP counterpart remains unbuilt and copied publications predate EAP repair.

Both workers explicitly acknowledged new working directory. Pipeline owns merged
refresh; host owns validated checkpoint then isolated native feature authoring.
Old-worktree cache-pruning permission is superseded: preserve it read-only.

### Fresh merged products and initial actual managed PPP

Validated host checkpoint `dbb67fb3` (22 named files) and migration/evidence docs
`36f94d2f` are committed in active original checkout. Pipeline refreshed all eight
profiles with the merged toolchain: threaded `100938-1e61b771`, nosys
`101126-d7f9a0c3`, locking `101258-8d10f70a`, contrib `101436-e4f2dfc8`,
makefsdata `101623-bba174b7`, HTTP-content `101639-f50e3581`, unit
`101818-531970c1`, unit-nosys `101941-06379f74`. All raw/processed builds pass.
Fresh generated HTTP preparation executes the translated tool in the new root.

Actual threaded raw-JIT PPP serial and independent PPPoE tests pass
`102137-1ff0f781`; full three-profile/four-cell matrices and refreshed upstream
137/132 diagnostic matrices are running next. No compiler/runtime workaround was
needed for the first actual managed link sessions. Native PPPoE data/control
fixtures and locking LQR negotiation witness pass. Full PPPoL2TP, 6LoWPAN/ZEP,
remaining apps/contrib and default-off feature variants remain open.

Host native IPv6 forwarding boundary control found hop-limit zero wrapping to255
in pinned ip6_forward. Override-first candidate: guarded per-translation-unit
IP6H_HOPLIM_SET through a typed single-evaluation helper, preserving ordinary
0..255 output setter semantics and routing zero through existing ICMP path.
Native evidence/qualification and product integration remain pending; no additional
text repair is authorized yet. Bridge private timer/state teardown uses an authored
include wrapper under review, keeping original algorithms and sources unchanged.

### Fresh actual PPP and upstream matrices complete

Actual PPP serial plus independent PPPoE all four cells pass threaded
`102218-436a6eb5`, NO_SYS `102323-8af6d3ee`, locking `102423-3e0a37ed`; locking
includes explicit LQR option ACK. Original upstream 137 cases pass all four cells
`102529-f99f8fc4`; applicable NO_SYS132 plus native registration/negative controls
pass all four `102607-122fea01`. Pipeline handed .NET to host for IPv6/multicast.
Its validated EAP/PPP checkpoint excludes unfinished L2TP and unqualified host
fixtures. Native L2TP first tunnel/mutual-challenge/rejection/control/StopCCN
baseline passes; full IP data and managed mirror remain in progress.

Native IPv6 hop-limit override candidate now passes twelve threaded routing
controls, 0/1/2/255 edge values, all 256 ordinary setter values, single evaluation
and assignment-result parity (`artifacts/hop-override-native/threaded`). Original
zero-limit forwarding failure is durable under artifacts. Product integration
remains pending coherent per-unit override scoping; no fourth text patch needed
if final native/managed proof holds. Host transient agent error was resumed within
the authorized bounded local compatibility fixture scope; no work was lost.

Validated EAP/PPP checkpoint `f4448346` commits 21 named files: guarded release
repair and native parity, actual PPP serial/independent PPPoE matrices, new-root
diagnostic evidence and six staging guard checks. Source snapshot:
`artifacts/ppp-source-snapshot-20260929-1030.json`. Unfinished L2TP, host fixtures
and coordinator/unrelated files stayed outside the checkpoint. Pipeline continues
isolated L2TP/6LoWPAN/ZEP/application work while host owns .NET.

### IPv6/multicast checkpoint and next link integration

IPv6 addressing and multicast pass all four cells in all three profiles:
threaded `103455-c49324c6`, NO_SYS `103633-09b9abb5`, locking
`103831-2b08f372`. Checkpoint `e229fc42` contains 13 named fixture/native parity
and dispatcher files. Cases cover DAD, invalid RA rejection, SLAAC lifetime and
duplicate-address behavior, stateless DHCPv6 retry/query/DNS replacement, and
IGMP/MLD exact-group admission, reports and leave. These finite cases do not close
the broader addressing or core aggregate gates.

Native IPv6 routing and the scoped hop-limit override pass all three profiles,
including original zero-hop failure, 0/1/2/255 routing boundaries, all ordinary
setter values and single-evaluation/result semantics. Integration uses a guarded
macro/typed helper, not an additional source patch. Native bridge teardown passes
two bridge instances within one native stack and natural allocation/timer
baselines; managed owner-isolation remains pending. The authored include-wrapper
exposes teardown while retaining original bridge algorithms.

The fourth narrowly guarded source repair corrects LOWPAN source short-address
recognition from the destination constant to the source constant. Original valid
short-source input fails with exit92; the adapted radio/BLE ten-input control passes
all three native profiles (`artifacts/lowpan-native/<profile>`). Macro replacement
would corrupt destination uses, and a whole-parser override would replace more
original code. Full managed link qualification remains pending. Native L2TP now
passes tunnel authentication/control plus actual data in all three profiles.

Pipeline owns the coherent refresh and managed link/routing/bridge build window.
Host develops owning socket/netconn/resolver contracts with registry admission,
blocking-operation leases, close-before-join, accepted-handle adoption and DNS
callback lifetime through core-worker shutdown. Shared facade changes wait for
handoff. Only the confirmed-idle current-root managed-consumer intermediate cache
was removed to recover space; historical worktree and all receipts remain intact.

### Link/routing checkpoint and owning-boundary review

Checkpoint `7835b38b` commits 47 named files after all-three-profile/four-cell
L2TP, finite LOWPAN, IPv6 routing and bridge qualification: threaded
`110105-df4730c6`, NO_SYS `110524-6852b698`, locking `111004-d11b3587`.
New publications are `105719-74e8e2eb`, `110311-4b343b75` and
`110757-36b627d9`. Nine pipeline checks and layout pass. The 54-file input snapshot
is `artifacts/link-routing-source-snapshot-20260929-1114.json`. Auxiliary refresh
waits for the next coherent wrapper/configuration batch. Finite LOWPAN inputs do
not close fragmentation/ZEP; isolated ZEP native work is outside this checkpoint.

Owning API review identified a descriptor lifetime gap between managed operation
admission and entry into the original syscall. Native threaded/locking controls
prove that an authored include-wrapper exposing original get_socket/done_socket
references prevents premature reuse, returns the original closed-socket error,
and restores allocation/resource baselines after release. No socket algorithm or
upstream source text changes. Evidence: `artifacts/native-socket-refs`.

Ordinary interface cleanup also needs to preserve backing through queued loopback
callbacks: netif_remove does not cancel netif_poll callbacks, and the locking
callback_wait path is not FIFO. The native locking control reproduces the old
lifetime error under ASan; a public-API stop/remove, FIFO drain and delayed-free
prototype passes. Evidence: `artifacts/native-interface-retirement`. Shared Host
and facade promotion plus managed qualification remain pending. Durable retired
interface lists must survive failures and disposal retries.

Host agent execution repeatedly ended with a generic possible-cybersecurity-risk
flag despite the task being local compatibility/resource-lifetime implementation.
No action-specific restriction or external target was identified. Its WIP is
preserved, retries paused, and the parent was notified. Pipeline continues
independent work; this is not a whole-campaign completion or external-prerequisite
claim. Confirm ownership before resuming or reassigning the host implementation.

Read-only review of emitted constants in all three latest product profiles confirms
required option coverage still missing: VLAN/PCP/static ARP; DNS local-host lists
and mDNS queries; netbuf receive information; TCP SACK/backlog/timestamps/window
scale; netif removal callback and tcpip timeouts; socket linger/reuse-receive/Linux
FIONREAD mode; ND6 RDNSS; HTTP dynamic/file-state/async/CGI-SSI/raw-SSI variants.
These constants are zero in actual generated code, not merely upstream defaults.
HTTP keepalive and extended netif status callbacks are already enabled. Evidence:
`artifacts/feature-options-review-20260929-1122.json`. Coherent required variants
and behavior cases remain implementation work; tuning/debug constants should be
classified separately rather than generating an arbitrary option Cartesian product.

### User verification restriction applied

The user prohibited corruption tests unless an exact upstream test exists.
Pipeline removed or replaced 13 authored case groups from every identified native,
managed and prerequisite route, including NativeOracle/packets.c which many suites
execute indirectly. Durable review is `config/verification-policy.json`: 23
entrypoint groups, 137 authored source hashes, and 11 original upstream exception
records with pinned source hashes and exact definition/registration lines.
Coordinator independently verified those hashes/locations and reviewed the
implementation diffs before reporting audit completion to root and releasing
permitted verification. No authored corruption exception remains.

Removed groups include UDP-Lite payload mutations; PPP authenticator/MPPE-header/
FCS mutation; PPPoE token and length mutation; L2TP identifier/digest mutation;
SLIP unknown escapes; LOWPAN dispatch and ZEP version mutation; RA hop/checksum
and DHCPv6 transaction mutation. L2TP rejection uses an authenticator correctly
computed with a genuinely different shared secret. Pending-interface/bridge
queues now use coherent IPv6 no-next-header packets, and routing address variants
recompute transport checksums. Product validation is unchanged. Original upstream
malformed TCP/mDNS and recorded short-ICMP/fragment-boundary fixtures stay intact.
Ordinary errors, valid TTL/hop expiry, auth rejection, lifetime and harness-failure
controls remain. All prior receipts predate the restriction; revised fixture
behavior is unrun until fresh receipts are recorded.

The user requested a fresh replacement for the unavailable host worker. Coordinator
attempted a new read-only-first worker with explicit no-bypass and test constraints,
but spawning failed with `agent thread limit reached`; the errored worker still
occupies a thread, and no close/release tool is exposed. Parent was notified.
Pipeline remains the sole active implementation/build owner; source WIP is intact.

### First permitted verification and owning API handoff

Receipt `20260929-113734-bf3d5d86` passes all 30 commands for ten revised suites
on threaded raw JIT and their matching native prerequisites: bundled PPP, serial
PPP, PPPoE, L2TP, SLIP, finite LOWPAN, datagrams, IPv6 addressing/routing and
bridge. No excluded custom corruption cases ran. Alternate profiles and
processed/AOT cells still require revised qualification after the next coherent
product refresh. Policy/removal checkpoint is `82eb1a4a`; documentation checkpoint
is `9a47521d`. Earlier matrices remain historical evidence of their recorded inputs.

Pipeline now owns promotion of the preserved owning socket/netconn/resolver
prototypes, capability metadata selected from the actual product project,
descriptor references spanning admission through original syscall completion,
resolver callback retention through core-worker join, and delayed interface free
after a real FIFO drain. Shared configuration, facade and Host changes are one
coherent batch; no other worker may build or mutate these inputs during validation.
Coordinator review requires numeric IPv6 zone range validation consistent with
lwIP's u8 zone representation, retained after-core-stop hooks even after successful
service stop, and retryable retired-interface ownership. These remain unqualified
implementation changes until fresh product and behavior receipts pass.

### Owning API checkpoint

Fresh product publications pass raw/processed: threaded `114549-0edfd8d9`,
NO_SYS `115149-f29bbb4b`, locking `115452-acdef463`. The owning API, consumer,
owner-lifecycle and pool-recovery suites now pass all four cells in each profile:
`115825-fc8e63a0`, `120036-26c6b530`, `120312-d52e329b`, respectively. This is
post-restriction permitted verification. Socket/netconn cases are correctly
inapplicable to NO_SYS; actual selected-product metadata controls compilation,
and an explicit mismatching profile hint is rejected.

The initial threaded managed run `114817-d24c28ac` failed in an authored IPv6
fixed-buffer address expression. Endpoint write/read and resolver copy now use
the first fixed-buffer element's address; the failed receipt remains preserved.
Focused corrected runs `114957-74819912` and `115339-2ea6fd9d` preceded the
complete matrices above. No compiler repair or upstream text change was needed.

Review also corrected accepted-netconn retention on failed cleanup, monotonic
close timeout accounting, and one-reader/one-writer admission matching upstream's
full-duplex contract. The all-profile matrices include actual accepted-child
shutdown races, late canceled DNS callbacks under GC, pending query disposal
retry and owner isolation, numeric zone bounds and interface argument validation.

An additional fixture now exercises actual NETIF_ADDED duplicate registration,
constructor rollback and restoration of the original interface/default route and
heap/pbuf baseline. Product implementation is unchanged; owning-only four-cell
reruns pass threaded `120630-457be2fa`, NO_SYS `120955-dfcf28c1`, and locking
`121107-56ed1e8c`. Superseded prototype copies were archived
with hashes under `artifacts/owned-api-prototype-history-20260929`; current tests
compile the production facade.
Normalized ProjectReference property propagation was then added for a consumer
that selects its product locally with a relative path. The outside-sample NO_SYS
project checks both generated constants and the facade's socket API absence
without command-line hints. Focused receipt `121229-14960450` failed: the normal
SDK restore graph flattened references and lost the additional properties, so
the consumer selected the wrong transitive generated product. The final import
uses static graph restore and removes only the selected product's duplicate SDK
edge. An unrelated two-hop project dependency remains available. All four cells
for owning API and consumer pass NO_SYS `121848-8f47f868`, threaded
`122007-dac94da8` and locking `122125-d03caecf`. The failed receipt is retained.

Final review found queued async calls missing from disposal accounting. Public
async invocation now retains the owner and increments scheduling admission before
queueing, settles the caller Task before releasing that count, and joins both
scheduled and running operations. Deterministic actual public-receive pauses pass
threaded `122521-090f75f3` and locking `122622-a0980575`, all four cells. NO_SYS
excludes these APIs and keeps its applicable final-graph evidence. Initial
fixture failure `122333-540a599b` is preserved; its transport pump had continued
polling an intentionally disposed owner, so the witness now uses independent
loopback owners.

Checkpoint `aa2e23f1` commits exactly 38 implementation/test/doc paths listed in
`artifacts/owning-checkpoint-files.json`. Nine pipeline checks, layout and syntax
checks pass. Snapshot: `artifacts/owning-api-snapshot-20260929-1227.json`.
Coordinator independently reverified all 145 authored source/build hashes,
11 implementation hashes, 11 exact upstream records and 24 entrypoint groups.
Policy SHA256: `53afae6f1815810e68901c915457b776f2c76f725a3b23039e392413cde0b52e`.
No new compiler repair or upstream text patch was required. Older contrib and
HTTP-content publications need refreshed capability metadata before facade use.

Next implementation remains ZEP/full LOWPAN lifecycle and required default-off
feature variants, followed by remaining application/contrib workflows and owning
services. Typed interface identity and bounded pending TCP TIME_WAIT/fragment/
ARP/ND shutdown observations remain open. Final aggregate verification,
all-profile rooted AOT and reproduction gates remain incomplete.

The next ZEP/radio/BLE batch must retain unchanged original algorithms, explicit
shared radio/ZEP/borrowed timer leases, per-stack PAN/context/reassembly semantics,
and stop/FIFO/free with retry ownership. Closing ZEP must preserve a surviving
direct-radio lease and its fragments; the last lease releases pending resources.
Qualify valid fragmentation/output/expiry and owner independence. The prototype's
ZEP radio FCS bytes are being corrected to a real CRC before reruns; direct radio
input uses its documented already-stripped-FCS contract. No custom malformed
or corruption scenario may be added.

### Owning LOWPAN/ZEP/BLE implementation in progress

Pipeline remains sole implementation/build owner in the current checkout. The
replacement-worker request is still constrained by the agent thread limit; no
replacement is running. Existing source changes are preserved, and the failed
worker is not being retried to bypass its rejection.

The promoted `src/port/lowpan_service.c` includes the staged original LOWPAN body
before the original source include root, plus original ZEP; BLE retains its
original separate context/address state. Product wrapper receipts record selected
include paths and hashes. Radio, ZEP and explicit borrowed-radio leases share
one original timer. BLE holds no radio timer lease. Stop, actual FIFO drain and
backing free are separate retryable phases.

The authored `LwipLowpanLink` facade supplies radio/BLE/ZEP constructors, copied
input/output, owner-wide PAN/context/BLE-peer configuration, same-owner transport
validation and safe interface index/MTU/link-state/default-route access. A shared
initialization helper registers an empty managed wrapper before acquiring native
state, retaining failed cleanup for stack-disposal retry. These changes are not
qualified by the earlier owning API receipts.

Permitted native controls pass all three profiles under `artifacts/zep-native`:
valid ZEP FCS, shared timer, reassembly surviving another service's close, last
lease cleanup, BLE independence, rollback/restart and natural resource baselines.
Raw/processed publications pass threaded `20260929-123615-d0fe9319`, NO_SYS
`20260929-123913-71c8944e` and locking `20260929-124411-8570ce3e`; managed
behavior qualification is in progress. Publication alone is not a behavior pass.
The new suite audit records 146 authored hashes, 25 entrypoint groups and four
LOWPAN implementation hashes before invoking it. Native reports record the same
product wrapper and selected original/staged include hashes. Next work covers
copied output, compression/fragmentation/MTU behavior, valid fragment ordering and
expiry, all applicable four-cell matrices, refreshed policy provenance, and
relevant owning/lifecycle regressions after the shared facade-helper change.

First promoted managed/native receipt `20260929-124849-beeaef37` passes threaded
raw JIT for the finite ingress/lifetime scope above. Initial build receipt
`20260929-124649-91471845` is preserved; an authored fixture used an enum member
as a top-level constant and was corrected to the explicit upstream IPv6 type
value. Output/compression/MTU expansion and the remaining matrix are still open.
Finished private cache pruning is recorded in
`artifacts/owning-completed-cache-prune-20260929-1247.json`; application outputs,
reports, references and published products were retained.

### Worker interruption and preserved handoff

The pipeline worker has now also stopped with the generic service error
"This content was flagged for possible cybersecurity risk." Both implementation
workers are errored; no implementation worker is active. The service did not
identify a particular source action as prohibited. Coordinator attempted a fresh
worker restricted to a separate read-only handoff audit, with explicit no-bypass
instructions; creation again failed with `agent thread limit reached`. No exposed
tool can retire the errored threads. Parent was notified. Do not describe this as
completed implementation or a successful replacement launch.

The latest native output experiment is preserved under
`artifacts/zep-output-native/nosys`. Valid payloads 17, 900 and 1232 bytes pass;
1233 emits 14 frames but hits the original IPv6 reassembly resource limit:
`IP_REASS_MAX_PBUFS=10`, `ip6_frag.memerr=1`, no queued multicast loopback.
The proposed upstream configuration value is 32, bounded against the existing
256-buffer pool; **it has not been applied**. Neither `profiles.json` nor
`adaptations.json` has a working-tree change. A separate suspected lost fragment
link during valid reordering remains unproven because this earlier resource-limit
failure stops the fixture. No new algorithm patch was added.

Current source WIP is the owning LOWPAN facade/wrapper, service-registration
helpers, suite registration/runner, policy inventory and `tests/Zep` fixtures.
It remains uncommitted. No process with a campaign working directory was active
at the coordinator's handoff check. Latest qualified implementation stays at
`aa2e23f1`; `28e65ccd` recorded its scope. Current source publications and finite
`124849-beeaef37` behavior are scoped above; pending output work is not a pass.

Once normal agent capacity is restored, assign a fresh worker only permitted work
with explicit path ownership and the user's no-custom-corruption restriction.
Begin with `git status --short` and this ledger; do not retry any identified
prohibited action or use another agent to bypass a rejection. Safe existing
orchestration entry points remain:

```sh
cd /home/marius/p/dotcc
bash Scripts/campaign.sh layout lwip --check
bash lwip/scripts/translate.sh --fetch never --profile threaded
bash lwip/scripts/test.sh --fetch never --profile threaded --suite lowpan-services --form all --mode all
```

Translation/testing commands are restart references, not evidence that the
unfinished output case or its proposed change passed. Review the permitted
fixture/source snapshot before executing affected suites. Complete native output
qualification and any necessary configuration work before coherent product
refresh; then add the managed output/copy-retention counterpart, all applicable
matrix cells and shared lifecycle regressions. Required default-off feature
variants, applications/contrib, protocol-specific owning services, final rooted
AOT/full verification and reproduction remain unfinished after that batch.

Coordinator handoff snapshot: `artifacts/worker-stop-20260929-100232/manifest.json`
records 14 preserved lwIP WIP paths with verified source copies and SHA256 values.
Unrelated libuv changes and the compiler-review document were not copied or
modified. All campaign WIP, including this ledger, remains unstaged/uncommitted.

### Fresh coordinator and implementation worker resumed

The resumed coordinator recovered the current `sqlite` checkout, read the plan,
verification policy, validation index and worker-interruption handoff, and
confirmed all campaign and unrelated libuv work remained preserved. A fresh
implementation worker `/root/lwip_coordinator_resume/permitted_implementation`
started successfully. It owns source/configuration/scripts/fixtures and shared
builds; the coordinator owns this ledger and validation index. Initial work is a
review of preserved LOWPAN/ZEP inputs before allowed execution, followed by
remaining valid-traffic, ownership and lifecycle implementation. No custom
corruption testing is permitted. No new verification result is claimed by this
restart; full acceptance remains incomplete.

### Valid LOWPAN output expansion

Fresh-worker review confirmed the preserved output fixture sends coherent original
radio frames, validates their real FCS, and only changes complete-frame delivery
order or repeats unchanged frames. The upstream reassembly budget is being raised
from ten to 32 packet buffers against the existing 256-buffer pool; this is a
bounded product configuration choice, with diagnostics unchanged. The first native
run then passed payloads 17, 900, 1232 and 1233 bytes and duplicate delivery, but
failed permitted reordered delivery. These observations do not yet qualify a
complete suite.

Coordinator source review confirmed a local original `lowpan6_input` list bug:
when a received fragment precedes the first queued FRAGN, head insertion changes
`lrh->frags` but omits linking `p->next` to the old head `q`. The fixture delivers
unmodified FRAG1, last FRAGN, then intermediate FRAGNs; it does not fabricate
corrupt or overlapping data. One guarded assignment in that branch is approved
for staged adaptation. There is no local configuration/hook for the insertion; a
whole-function override would duplicate the original parser algorithm. Original
reference remains untouched, and matching native original-failure/staged-pass,
resource baseline and managed matrix evidence are required before qualification.

The read-only remaining-scope audit is recorded in
[remaining-work-audit.md](remaining-work-audit.md). It identifies concrete required
option/application/contrib batches and distinguishes emitted source from behavior
and lifetime qualification. The audit worker now owns only new unregistered
`tests/Applications.Udp/` fixture drafts. It may not edit shared source/config or
build/run/register tests while the sole build owner finishes LOWPAN. These drafts
are implementation work, not qualification evidence.

All three native original/staged comparisons now confirm the scoped reorder repair
and natural resource baseline. Threaded raw/processed publication
`20260929-141507-80ed4f8d` and focused raw JIT behavior
`20260929-141634-8e1839b6` pass, including copied radio/BLE output, compression,
MTU crossing and permitted duplicate/reorder/expiry. The implementation worker is
qualifying all three profiles and four cells plus owning API/consumer/lifecycle/
pool regressions. Remaining cells are not yet counted as passes.

Evidence retention correction: the native runner reuses per-profile output paths,
so the interrupted worker's original ten-buffer resource-limit run log was
overwritten during resumed execution. Its historical handoff summary remains,
but that original log must not be claimed as independently archived evidence.
Reorder-before failure and exact comparisons were archived separately. After the
locking matrix, a fresh isolated NO_SYS valid-output budget-ten control will
substantiate the configuration rationale using a distinct artifact family; it
will be identified as fresh evidence rather than recovery of the old log.

### Qualified LOWPAN checkpoint and next build handoff

Implementation checkpoint `537bfc3d` contains exactly the 20 reviewed paths in
`artifacts/lowpan-qualified-snapshot-20260929-1430.json`. Coordinator independently
verified those path hashes, 148 authored policy hashes, 11 owning and four LOWPAN
implementation hashes, 11 pinned upstream case/registration records, native
comparison source/header bytes and the managed specialist reports. LOWPAN and
common owning/lifecycle suites pass all three profiles and four cells. Final
pool-recovery receipts are locking `142834-20facba6`, threaded `142916-95783640`
and NO_SYS `142927-2dfeb959`; consumers pass all profiles. The locking original
batch `142515-eba9318e` remains failed overall because its later pool AOT build
hit MSB3883. Completed specialist passes are scoped, and the final project-import
repair has fresh successful pool receipts.

A fresh isolated budget-ten valid-output control is now preserved in
`artifacts/zep-resource-limit10/nosys/report.json`, paired with the passing
budget-32 fixture. This is fresh evidence, not recovery of the overwritten
interrupted-worker log. Ten pipeline checks, layout, source syntax and whitespace
checks pass. Public LOWPAN usage and lifetime contracts are documented.

Next, primary is preparing policy inventory for source-reviewed application
fixtures before a brief explicit native-build transfer to the secondary worker.
All shared source/config stays frozen during that native window; primary works
read-only on enhanced option design. Secondary may change only its own authored
fixture directory and preserve failed attempts; it may not patch original source
or change shared configuration. The next product batch will add explicit option
coverage and an enhanced threaded profile, while real application owning services
and SNTP notification integration remain required.

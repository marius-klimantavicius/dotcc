# Validation ledger

All observed executions below are Linux x64. Windows execution has not run.
Every skipped/unrun form remains open; a decoder pass is not CPU execution.

## P6 IMDSv2 extension (2026-09-23)

[I0–I2](P6-IMDSV2.md) is complete. Each configured execution owns a private BCL
HTTP listener, with guest `169.254.169.254:80` routed to its loopback endpoint.
The same serializable metadata options work in both public execution modes.
Tokens require IMDSv2, expire monotonically and are isolated by execution;
identity, user data and role credentials are supplied explicitly. Credentials
retain their configured expiration. Automatic rotation/STS, signed identity
documents, IPv6 and full EC2 metadata coverage are not claimed.

The static musl .NET 10.0.12 guest uses `AWSSDK.Core` 4.0.102.6 at the ordinary
metadata endpoint, with IMDSv1 fallback disabled and no endpoint override inside
Blink. It discovers the role, obtains credentials, reads the SDK cache, clears
that cache and fetches again. Its SHA-256 is
`737f351d13ddea894b01e648c4d42546a471eff979521437b95099c609bbc890`.
The final JIT/NativeAOT × InProcess/SeparateProcess matrix passes twelve guest
executions, followed by 24 additional executions from cold consumers/workers.
Every group includes concurrent independent machines and same-machine restart.
Exact output, normal exit, resource release and actual AOT worker identity are
checked. Twelve native guest controls use only the simulator's private loopback
endpoint. No real host link-local metadata endpoint or AWS account is accessed.

Host JIT and NativeAOT checks pass token issuance, expiration, cross-execution
rejection, restart, reuse, fragmented valid requests, HEAD/UTF-8 length, metadata,
credentials, concurrent requests, peer identity, default network denial and
ordinary disposal. A normal page-registry check concurrently registers/resolves
4,096 backed pages while the registry grows. The focused existing nonblocking
connect/lifecycle checks and network-grant gates also pass. Existing NativeAOT
HttpClient, public machine API and Kestrel/general sample matrices pass all four
consumer/execution forms after the runtime corrections below.

All final receipts pass (paths are under `artifacts/`):

| Gate | Receipt | SHA-256 |
| --- | --- | --- |
| Fresh postprocessed instance-v1 delivery | `translation/attempt-y52sy716/receipt.json` | `a7d96d8a6e0b9fd4d70cfc2d19528938260478bfdebb010532dd342866544276` |
| Serial extension/regression campaign | `imds-campaign/final-awzbgog7/receipt.json` | `7ae653fd0ba07f0b9664ce2bd0fec4aa19526281377d9902c9fc9a675b98678c` |
| IMDS protocol, native control, public AWS SDK matrix | `imds-campaign/final-awzbgog7/imds/receipt.json` | `da0f286c73352641498935a769def32066fc97ce51ed0b47a7d21a1710b212be` |
| Existing NativeAOT HttpClient matrix | `nonblocking-guest/attempt-1nf6ze4r/receipt.json` | `2169882d4cc2b9c9cab9a916c0f228d2407ef92b127dbd3208b3d6d04d9dc321` |
| Public machine API matrix | `machine-api/attempt-i9tei3k9/receipt.json` | `42069aad8efcd46072eceb621da8159d5bcce952d91c20312344209a5d8c2f20` |
| Kestrel/general sample matrix | `machine-api-kestrel/attempt-fld6ovt2/receipt.json` | `773eb4ea1a5e8e4c059a0b1e5df6e03a0fe290d5fc8dda21903db8d12753a08d` |
| Existing host network grants, JIT+AOT | `host-network-grants/attempt-7nkipd5l/receipt.json` | `2c571f79f3f663bd6a69b485ad3c5ae30598d68a7998f378ee768f66d7c91b61` |

Actual SDK execution exposed an unsynchronized upstream non-linear host-page
registry: `TrackHostPage` appends/reallocates while other workers use
`FindHostPage`. The pair now selects typed managed function overrides backed by
a per-program BCL table with serialized publication and stable reader snapshots.
The delivery freshly emits all 108 objects and verifies six endian intrinsics
and fourteen managed boundaries, including the exact canonical source origin of
the source-defined tracking function. Inline deduplication, literal pooling and
direct links to authored sources remain enabled. There is no generated-source
repair or new upstream C patch. The owner also mirrors upstream resumption after
handled architectural signals; no synthetic hardware-fault test was added.

Further normal repeats exposed child startup before `SysSpawn` stores
`CLONE_PARENT_SETTID`. A child could see TID zero and native runtime thread
startup could stall. The managed owner now opens a child startup gate only after
the creating instruction completes publication, including unwind, and disposes
gates after workers join. This matches Linux's publication-before-wake ordering
in [`kernel_clone`](https://github.com/torvalds/linux/blob/v6.17/kernel/fork.c#L2466-L2481).
The [normal clone fixture](../tests/GuestThreadPublication/README.md) passes 64
native, 64 JIT and 64 NativeAOT executions, checking the child's first TID read,
immediate exit, clear-TID and complete cleanup. Budget/deadline diagnostics now
retain per-thread instruction counts/locations and any first architectural trap.

Additional diagnostic observations are kept separate from public qualification:
`imds-campaign/final-awzbgog7/startup-observation.json` (SHA-256
`4674544e320bbcbc369e460d9b35bdd779a7c8a0d18c10d2256b2f275e941558`)
records 67 early zero-TID observations before the gate, then zero across 192
actual HTTP guest executions with the gate. That diagnostic consumer has extra
startup/stop observations; the final public matrices above use production code.

Failed attempts remain failures. Initial `imds/attempt-l7lzqm7q` and the
`imds-stable-helper` diagnostics retain actual SDK execution failures preceding
the page-table fix. `translation/attempt-to31y2ok` retains the unmatched tracking
selector and interrupted emission; the corrected selector is verified in the
final delivery. `imds-campaign/final-9gnrht3h` and `final-2uef195r` retain the later
instruction-budget/HTTP stalls, including
`nonblocking-guest/attempt-q4b9c5aw`. Diagnostic `imds-stack-trace/run-44` and
`frames-63` identify native runtime thread startup and preserve stack/TID
observations. Budgets were not raised to mask these failures. The final campaign
passes after the publication fix; no custom fault injection or malformed ELF
cases were introduced. Windows and P7 remain unrun.

Reproduction and usage: [IMDS example](../tests/ImdsMachine/README.md),
[configuration](../src/Managed.Emulation/IMDSV2.md), and
[clone publication](../tests/GuestThreadPublication/README.md).

## P6 nonblocking-connect extension (2026-09-23)

The [N0–N3 sub-plan](P6-NONBLOCKING-CONNECT.md) is complete for Linux x64.
Nonblocking connect owns its pending operation beyond the initiating syscall,
reports EINPROGRESS/EALREADY/EISCONN, retains get-and-clear SO_ERROR, and publishes
poll/select/epoll completion. Connection epochs prevent an earlier unconnected
HUP from masking a later ordinary shutdown HUP. Final close and machine disposal
drain the operation; duplicated descriptors share its lifetime.

The unchanged static musl NativeAOT HttpClient guest passes 72 HTTP requests
across 12 translated executions: JIT/NativeAOT consumers × InProcess/SeparateProcess,
with two simultaneous machines and same-machine restart in each form. Its native
oracle passes 18 requests. Actual AOT worker executables are independently hashed;
all cases preserve exact responses/stdout, empty stderr, exit zero and resource
release. Ordinary runtime Activity/Guid initialization required a private BCL
`/dev/urandom` device, now mounted in both modes without host-device access.

The public API passes eleven native controls and ten cases per form, including
normal stop, cancellation, disposal, ownership and quotas. The existing sample
passes twelve Kestrel and twelve general guest runs with twenty native HTTP
comparisons. Host gates retain their applicable native/raw/optimized/JIT/AOT
matrices. Focused checks cover 32 connect/epoll/duplicate cycles, 32 caller-scope
cancellation/close/reuse cycles, sixteen concurrent owner disposals, ordinary
shutdown terminal edges, configured timed success and valid entropy reads.

All selected final receipts pass:

| Gate | Receipt under `artifacts/` | SHA-256 |
| --- | --- | --- |
| Postprocessed instance-v1 delivery | `translation/attempt-cekeeech/receipt.json` | `487e2a7b40b6329736ab53bef3086b782bc46336ae53df616617146120bfa7c5` |
| Focused connect/lifecycle/entropy JIT+AOT | `nonblocking-campaign/focused/receipt.json` | `5a1e498683fcbfd223c24a1c9cfedfae4a740f7e0276b059f6f00500be1700e4` |
| Actual NativeAOT HttpClient, all four forms | `nonblocking-guest/attempt-sjizsmch/receipt.json` | `285cc9045e063a5cefeca88dc5b31c71c48ed87e5ec42b7d1e9e0e00b6a7f5e2` |
| Public machine API, all four forms | `machine-api/attempt-_13omhjd/receipt.json` | `b161777725c8a5c831340af8963afc50a9b976961d5915aba9bc9fd7b3d083a3` |
| Kestrel/general samples, all four forms | `machine-api-kestrel/attempt-d1jqgowt/receipt.json` | `18e0dc8b10e9e839c1d54840786d900ec0d98d6a33bcc3fcc55ef5b418e628b9` |
| BCL host socket contract | `host-sockets/receipt.json` | `34e9c2d39c0fc3669b7c896ee81cf4419479b591cdf8b2cda7592b59ca23fadf` |
| Translated TCP callbacks | `host-network/attempt-ctj5__4m/receipt.json` | `1730dd84e910e7a591518df9397b8ac16630690ff228b66cd6880d4afeec8588` |
| Socket queries | `host-socket-queries/attempt-n8czn3mh/receipt.json` | `7162760a2e348bc95503664c20ca87eb09e86fee5e90635556c46cccd418d533` |
| Async socket/edge contract | `host-async-sockets/attempt-lliv1h37/receipt.json` | `d3a65493be67db3a0fb46d2fca1b18507caff2d4bae15529f67ed2c2f50c592c` |
| Socket timeouts | `host-socket-timeouts/attempt-b2pnvkpb/receipt.json` | `6fe02c58b01b7703d29290c01a2c2f2790d5d4fc6f03b7de0b83fa385e331b4f` |
| Poll/select readiness | `host-readiness/attempt-skau6xk6/receipt.json` | `224dcdec726f81e08056aada922907a03d3199af79df4ad66c6bd64dd5ecbf71` |
| Epoll | `host-epoll/attempt-g4s6a397/receipt.json` | `b043f7e1910450bac1313e2de318f862b78fb4cda33b3005380fff0b1b557f89` |
| Network policy grants | `host-network-grants/attempt-t3wp0fw3/receipt.json` | `7e714374edc3b00cf122aa69d5cf622d9aa59f816acf58419a24995c10bcfa36` |

Fresh generation preserves inline deduplication, semantic postprocessing and
original-source links. All 108 objects were freshly emitted in the first attempt;
subsequent delivery refreshes reused those exact objects after host refinements.
No compiler or authored C syscall bridge changes were needed.

Reproduce the new behavior with the commands in
[NonblockingConnect](../tests/NonblockingConnect/README.md) and
[NonblockingGuest](../tests/NonblockingGuest/README.md). Existing gates use
`bash blink/scripts/test-host-sockets.sh` and `python3 blink/tests/NAME/run.py`
for HostNetwork, HostSocketQueries, HostAsyncSockets, HostSocketTimeouts,
HostReadiness, HostEpoll and HostNetworkGrants. Public/sample commands are in
[MachineApi](../tests/MachineApi/README.md). Exact final commands and receipt
indexes are retained under `artifacts/nonblocking-campaign/`.

Another session edited/rebuilt the shared postprocessor during qualification.
Final public/sample and epoll runs use immutable producer closures from
`artifacts/nonblocking-campaign/producer-tools`: all six compiler and five
postprocessor file hashes exactly match the delivery. This qualifies those
recorded producers, not the other session's subsequent uncommitted tool changes.
Actual compiled Blink source files, membership and build configuration stay
frozen. Historical Kestrel ELF inputs are checked against their exact retained
producer archive rather than today's unrelated package versions. No behavior
assertions or binary/oracle hashes were weakened.

Preserved nonpassing attempts:

- `translation/attempt-zym9j4i9`: host sources changed after staging during the
  terminal-epoch refinement; the snapshot check rejected it before publication.
- `nonblocking-guest/attempt-hxbc6kg5`: ordinary HttpClient Activity/Guid entropy
  failed before networking; native strace identified `/dev/urandom`, resolved by
  the BCL device without changing the guest or disabling diagnostics.
- `machine-api/attempt-rqafwfoh`, `machine-api/attempt-rfxhfazq` and
  `host-epoll/attempt-s810h60q`: unrelated live postprocessor binary/source changes
  invalidated frozen identities; final archived-producer runs pass.
- `machine-api-kestrel/attempt-rv9d1i_0`: today's package file differed from the
  historical native ELF build; validating the retained original input resolves it.
- `instance-lifecycle/attempt-0wv811qh`: mistakenly selected legacy synthetic
  protocol-failure fixture has an obsolete frame-budget assertion. Its payload
  fits the 24 MiB limit introduced by `51948ad` before this extension, so it tries
  a nonexistent executable. It remains unqualified and was not rerun or expanded:
  these synthetic failure cases are outside the approved scope. Current valid-guest
  MachineApi lifecycle cases pass in all four forms.

No manufactured transport failure, deadline expiry or invalid ELF is part of the
new tests. Nonzero SO_ERROR and every transient race are not deterministically
qualified by successful loopback traffic. DNS, IPv6, TLS, IMDS routing/simulation,
Windows, snapshots and P7 remain outside this extension. Stop here.

## Original P6 machine API qualification

All selected P6 gates pass, including fresh generation and complete consumer
reexecution after the allocation-free struct-scope compiler refinement.

| Gate | Receipt under `artifacts/` | SHA-256 |
| --- | --- | --- |
| Public instance-v1 delivery | `translation/attempt-tfqtuor3/receipt.json` | `83b804796d8ee211ed9589e2acd996fc3a6f05911204579e4eec0e31b8eb314a` |
| Public machine API all four forms | `machine-api/attempt-b074atpe/receipt.json` | `5841d3b0e8d487e60a1dd685fd5fabf369c4a5bc6d65efef8b979cea343c2be7` |
| Actual Kestrel and general public samples | `machine-api-kestrel/attempt-cqqbx2eu/receipt.json` | `e29648060f862ae629d399510e8ec5ac96e99b07b2751c2e2998af80951c9ffa` |
| BCL mount boundary | `host-mounts/attempt-rmljxlg0/receipt.json` | `6d5cd50f78ccd4e71efc7647ed85cf52f233686ffd8b3cb658d0066a867627e0` |
| Network policy boundary | `host-network-grants/attempt-rhpwmuhv/receipt.json` | `6cefe97940d2a1ab1667be49a0bf579c1f0472c9026801951e32964b3fd8f391` |
| Console/descriptor ownership boundary | `host-console/attempt-071nqrfd/receipt.json` | `0cf272d2bb4736c0eb43dce0aa1c6331e66517f11703c72e0cccf6a135f34a8c` |
| Cold NativeAOT worker deployment | `worker-deployment/attempt-bb3ghtd4/receipt.json` | `ee9a29c9dfee62e2937d9b37d5823b867ccb3156a9d46bb63cefdf9ab9d12242` |

Here the four forms mean **JIT/NativeAOT × InProcess/SeparateProcess**, all using
the actual postprocessed public project. They are distinct from P5's
raw/optimized × JIT/NativeAOT matrix. The product's raw compilation is checked;
P6 does not falsely claim a new raw-runtime CPU/service matrix.

The product has 108 instance-v1 objects, 330 endian selections (55 × six) and
852 managed selections across 12 typed boundaries (100 selected units, eight
absent). All 108 objects were freshly emitted with the final struct-scope compiler;
zero objects were reused in this final delivery.
Original authored Host/adapters are referenced from `src`, with literal pooling
and inline deduplication; no test/C execution frontend is published.

The machine fixture passes 11 normal native controls and ten API cases in each
form. It observes concurrent independent instances, no worker in either in-process
form, private persistence, mounted executable loading without import, RW/RO/COW,
owned and borrowed streams/EOF, memory/descriptor/storage quota enforcement,
wait cancellation, stop, concurrent disposal, explicit process kill, deadlines,
instruction bounds and unchanged host global environment/cwd/Console references.
Native resource controls intentionally use different capacities and prove normal
operations; private quota denials are separate managed contract assertions.
Both process forms use 25 distinct workers each; all 50 are gone. Independent
review verifies 2,327 retained paths, all 22 commands, four mode records and
actual NativeAOT consumer/worker ELFs with no DLL-worker fallback.

The actual sample passes 19 commands and four forms: 12 Kestrel executions
(eight normal exits, four cooperative stops), 20 native-semantic HTTP comparisons
and 12 general console/live-folder executions. Each form runs two overlapping
machines with distinct published endpoints, then restarts the same machine.
All report resource release; instructions range from 88,139,629 to 88,538,215,
within the unchanged 100M/60s/128MiB service profile. The six separate service
workers and all controller groups are gone without external cleanup signals.
Independent review checks 3,016 unique paths, 40 HTTP byte artifacts, exact
requests, native status/headers/body, valid variable Date and exact Content-Length.
All 1,372 shared API/sample input identities and eleven compiler/postprocessor
binary identities match. The general samples prove binary console EOF and host
file persistence across distinct machines through the ordinary CLI.

All 52 current Host project/C# files match the console receipt. The earlier
filesystem/network receipts differ only in three later console integration files;
their filesystem/network implementations remain unchanged. Native boundary controls
and host-only JIT/AOT contracts are kept separate from translated public execution.
Read-only source review confirms filesystem backends use System.IO and contain no
P/Invoke/native helper/proxy implementation. Windows is not thereby qualified.

The generic struct-scope qualification passes native/JIT/actual NativeAOT at
`DotCC.FunctionalTests/bin/instance-methods-qualification/attempt-l10w7brb/receipt.json`
(SHA-256 `a4fe5954bfc455049ea2e5ba0249af5286ffdefb83d70855aad88bb94d063437`),
with eight successful commands and 521 independently checked identities.
The separate root-run tests passed 85 unit/runtime and ten functional cases;
the warmed allocation assertion covers 1,000 Enter/Dispose calls at zero bytes.
That assertion is not a full-emulator performance benchmark. Struct thread/order
checks, callback origin retention and the reference-based cross-thread retain
lease remain distinct contracts.

The earlier passing pre-struct baseline is preserved at `translation/attempt-03tkbonj`,
`machine-api/attempt-l6hma1sn` and `machine-api-kestrel/attempt-wmdytj7d`.
It is not relabeled as the final compiler. The final packaging fix `987094e`
passes from cold worker outputs and with ordinary net10.0-only assets, while
separate explicit publication supplies the actual RID-specific NativeAOT worker.

Earlier failed build, EOF and worker-publish attempts are retained. CS8632 and
CA1416 build warnings remain; no blanket warning-free publish is claimed.
Trusted host roots are required: portable BCL path checks do not give atomic
containment against hostile concurrent mutation, hardlinks or special files.
In-process stop is cooperative, process force termination is explicit, and
neither mode claims hardened OS sandboxing or host-wide resource limits.
P7, Windows, performance, snapshots and PTYs are not covered.

## Final P5 Kestrel qualification

All final product gates below consume `translation/attempt-kaddtzlg`: 108 freshly
emitted objects, six unsigned endian intrinsics and four typed managed
ownership overrides, literal pooling/inline deduplication, original authored
source references, no CoreProbe/test main/C execution frontend. The unchanged
real ASP.NET Core/Kestrel static NativeAOT ELF and six-variable native profile
are pinned in [P5-KESTREL-RUNTIME.md](P5-KESTREL-RUNTIME.md). The focused coarse-clock
boundary fixture is separately compiled native/all-four evidence, not a product
execution receipt.

| Gate | Receipt under `artifacts/` | SHA-256 |
| --- | --- | --- |
| Public delivery | `translation/attempt-kaddtzlg/receipt.json` | `711c66f92de0d2bf3a4a3005c529f316ca29cabfc0a53a3353ea255969c0de11` |
| Coarse-clock native/all-four boundary | `host-coarse-clock/attempt-ny75u0ux/receipt.json` | `814a144c582b76521d9b429def663464a66afc1a11c66ac436eeddd3400e3841` |
| Actual standalone Kestrel | `kestrel-guest-execution/attempt-nb6iv2wz/receipt.json` | `3b88712a5bac07e432f6dd253d0b749f2033a2bc17e231b87099a3d6ec2bb558` |
| Kestrel all-four worker lifecycle | `kestrel-worker-instances/attempt-838yl87x/receipt.json` | `f4d1e9780d32871eb153f46bef9b51f318b559883938a76117a5858ceca97bae` |
| Normal guest signals all-four | `guest-signals-managed/attempt-39sz6b58/receipt.json` | `988820fa3343ffdfdcd3ad6cf75e4c342f682be702c223be87722b031d594aa2` |
| CPU all-four | `cpu-conformance-managed/attempt-mnd6gyjc/receipt.json` | `b833f6c176c5a481c8c1c8db7191fb24638604159b1d54390bb4758139b117f2` |
| Actual public solution/sample JIT+AOT | `managed-consumer-delivery/attempt-ljvj6fxh/receipt.json` | `502992598acf8897e45020c17aeed4c2007e08d980cac0796fe16433f965aaaf` |

Worker qualification runs 16 actual workers and 40 native HTTP comparisons,
covering simultaneous private instances, distinct endpoints, normal/cooperative
stop, restart, natural idle deadline and complete cleanup. Auxiliary marker
files are not guest-read; distinct executable paths/images and endpoints are
observed. The actual public sample adds four workers and six HTTP comparisons
using the real solution and both controller/worker publication forms.

The CPU refresh passes all 2,056 comparisons (514 per mode), native agreement
and 46 unchanged exclusions; independent review verifies 8,610 recorded
identities. Signal refresh preserves sender metadata, pending/unmask, actual
return and per-thread nested accounting. The final sample's six commands and
2,080 independent identity checks pass. Existing CS8632 warnings remain; no
warning-free build, Windows pass, arbitrary signal/epoll compatibility or
performance benchmark is claimed. All execution limits remain 100M/60s/128MiB
for Kestrel. All process groups exit without forced-success cleanup.

The generic compiler changes also passed the observed 55 focused unit and
14 functional tests plus the Release CLI build. Those root-run test results
were not written to a separate durable log receipt; actual compiler and
postprocessor binary identities are pinned by the public delivery and runners.
The separate endian fixture retains its native/all-four detailed contract proof
at `endian-intrinsics/attempt-u1ulwk20`. No P6 work starts at this completion.

## Earlier P4 corrected-profile qualification

| Gate | Latest receipt under `artifacts/` | Result |
| --- | --- | --- |
| Final delivery | `translation/attempt-4yjaed1_` | 108 verified product objects, no C test/execution frontend, literal pool, direct original-src references; raw/postprocessed/final direct-source builds pass. |
| Derived test-only core | `core-execution/attempt-_qybvbye` | Retains 108 canonical product objects and adds the C probe only to its private test link; normal native/all-four execution, ABI and direct boundary inventories pass. |
| Actual service | `guest-service/attempt-18nn8vfz` | Six exact native wire cases in each of four modes, normal exit and memory-owner release; 24 comparisons. |
| Owning stop/deadlines | `guest-execution-stop/attempt-kl7r74np` | Eight native controls and all 44 managed cases pass actual completion/stop/deadline/budget and cleanup contracts. |
| Actual sample | `managed-consumer/attempt-3c0qzs2p` | Real direct-source solution, JIT and rooted Linux NativeAOT health/normal stop pass. |
| Earlier publication inventory | `publication-audit/attempt-og66g378` | Exact pre-token-change AOT binaries; this historical inventory is not relabeled as the latest binaries. |
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
unrun at that earlier checkpoint. The final P5 integration is qualified above;
Windows execution remains unqualified.

## Current P4 qualification

GuestIo `guest-io/attempt-tnq5itfa` passes native/all-four actual SYSCALL file,
duplication, short transfers, valid cross-page vectors, ready-file poll and two
cleanup cycles; receipt SHA256
`15795abe9f6ead804b60eac9c3f996807f3d4f244c10da8515b583146f3c825c`.
It uses canonical 14c483 with 108 retained objects and its unchanged Host
snapshot. Callback cancellation now passes 22 translated-C scenarios plus a
direct BCL pipe-read reference per form at
`host-io-cancellation/attempt-blvv0ho4` (SHA256
`24085e30571d06b3fb41bdc3020cb960bcf708a5926c8323c0000b18a5065f6a`).
This does not qualify guest execution/poll/sleep stop or actual service startup.
GuestEnvironment `guest-environment/attempt-emy2577e` passes native/all-four
actual clock/randomness/thread-ID/signal-state syscalls and two cleanup cycles
against canonical 734288; SHA256
`45d7a0080dfac1664a95c9bb269f80a3318192483396213ecb4c25ee619f7706`.
Raw timestamps/random bytes/IDs are preserved; only their stated invariants and
normal state contracts are compared. No asynchronous signal delivery is claimed.
GuestTcp `guest-tcp/attempt-8r2oj_2k` passes native/all-four actual socket,
option, endpoint, readiness, exact 257/263-byte exchange, EOF and cleanup
contracts; SHA256
`ad31914a360345f527ae55fcff7dcb669b8e8b86b706971953585e447549a03d`.
This uses canonical 734288 and independent native/BCL peers, with no service
ELF, HTTP, execution worker or blocking poll stop claim.
GuestStreams `guest-streams/attempt-21z_o_3s` passes native/all-four actual
standard-fd registration, binary captures and ordinary terminal ENOTTY; SHA256
`0f62de4b0ffed1af3c25a4a9b09863cd8ba098ad790912fd173d455be3c81104`.
All 34 input/46 stdout/14 stderr bytes and two-cycle diagnostic state match.
This finishes the independent selected P4 contract fixtures. Actual service startup
now separately passes all four forms at `guest-service/attempt-18nn8vfz`, receipt
SHA256 `a9283cb6bdc87aa83a64fa1f4c19ea2bb02b6640aef60684b8c88e4502b60c4e`.
Owning execution stop/deadlines now pass at `guest-execution-stop/attempt-kl7r74np`,
SHA256 `aacfeae66a3d1eb23cee6194147d2451667e06f3bfd0dc74cee223bd5579443f`: eight
native completion controls plus 44 managed cases with actual wait barriers and
verified owner/I/O cleanup. P4 is complete for this selected profile; P5/P6 stay held.
See [P4's exact contract ledger](P4-HOST-SERVICES.md).

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

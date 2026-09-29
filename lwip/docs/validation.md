# lwIP validation evidence

Full acceptance is incomplete. This is a current evidence index, not a completion
certificate. Implementation and receipts belong to the isolated
`/home/marius/p/dotcc-lwip-full` worktree. The [plan](PLAN.md) defines the complete
scope; [coordination](COORDINATION.md) records active ownership and next work.

| Evidence | Passed scope | Receipt/report |
| --- | --- | --- |
| Initial complete stack builds | All 119 upstream stack units plus authored host C units, raw/processed; threaded, nosys and threaded-locking | `artifacts/campaign/current-<profile>.json` |
| Threaded consumer | Two generated owners over bounded copied Ethernet queues; IPv4/IPv6 TCP 256 KiB echo and UDP 123 bytes; dispose A then B continues; raw/processed × JIT/NativeAOT | `artifacts/campaign/20260929-075129-14a2b48d/consumer-cases.json` |
| Main-loop consumer | Same two-owner workload through raw APIs, current failure-path callback/pbuf cleanup; all four cells | `artifacts/campaign/20260929-075656-c2e46bad/consumer-cases.json` |
| Actual owner sys_arch | 24 host contract checks with actual generated owner entry, retain and disposal; all four threaded cells | `artifacts/campaign/20260929-075553-7edc1797/sysarch-cases.json` |
| ABI/checksums | 19 actual emitted/native core/socket/fd_set/netbuf size/offset comparisons and odd-address/odd-length/split-pbuf checksum checks; all four threaded cells | `artifacts/campaign/20260929-075904-16948989/abi-cases.json` |
| Rooted NativeAOT | Whole threaded library rooted separately from trimmed consumer; raw and processed execute consumer | `artifacts/campaign/20260929-075750-7fe1bb64/aot-rooted-cases.json` |
| Native product oracle | Raw/threaded/locking matching profile loopback TCP 256 KiB plus UDP, both IP families, ABI observations and deliberate failed assertion | `build/native/<profile>/report.json` |
| Native upstream Check | Original upstream cases, registration, fixtures and simplified test scheduler: 137 cases, zero failures/errors; test-only Check 0.15.2 | `artifacts/upstream-native/lwip_unittests.xml` |
| Native makefsdata | Unmodified checksum loop failure bounded by 8 MiB; staged loop-progress repair passes empty/odd/multiple-MSS checksum/data/HTTP framing controls | `artifacts/native-tools/report.json` |
| Shared repairs | Arithmetic conditional emission regression and ENOSYS runtime/header/message: 14 focused compiler and 6 affected functional checks passed; full lwIP retries pass | `docs/blockers.md` L005/L007; downstream evidence follows |

Actual-owner sys_arch exercises host contracts, while translated TCP/IP worker
execution is exercised by the consumer. Final host registry reclamation and zero
counters do not establish complete pending-protocol teardown. The low-level owner
API currently requires callers to settle their resources before disposal.

Required work remains: locking behavior matrix; direct netconn/altcp and full
socket APIs; complete memory/timer/core/addressing/routing/link/PPP/application
cases; translated makefsdata/contrib and real behavior; managed upstream Check
matrix and harness negative/fixture controls; pending/blocked lifecycle and
recovery; independent peers; import/active-feature/adaptation audits; all-profile
rooted/consumer delivery; clean and local-input reproduction; Windows orchestration.
No provisioned isolated TAP peer was present in the runner. No such missing gate
is counted as a pass. No production TAP/TUN adapter is being delivered.

These receipts describe their source/host snapshot. Later substantive edits must
rerun affected cases; a historical passing receipt does not qualify changed code.

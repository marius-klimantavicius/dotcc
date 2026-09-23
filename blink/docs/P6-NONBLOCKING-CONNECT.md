# P6 extension: nonblocking TCP connect

Authorized on 2026-09-23 after the public machine API delivery. Implement this
sub-plan to completion, commit each milestone or other significant progress,
then stop. IMDSv2 simulation and virtual destination routing are subsequent
work; this extension supplies their asynchronous outbound TCP prerequisite.

## Required behavior

Use the existing managed host network and BCL `Socket.ConnectAsync`. Preserve
in-process execution by default and explicitly selected separate-process mode.
No authored P/Invoke, native network helper, host network reconfiguration, or
implicit subprocess fallback. No dotcc/compiler change is expected; reduce and
qualify any actual compiler defect separately before broadening that scope.

- Track unconnected, connecting, connected, failed and closed states, with an
  owned pending operation and a separately retained guest socket error. Publish
  state transitions atomically and allow only one connection attempt at a time.
- Validate the existing numeric IPv4 outbound policy before initiating host IO.
  Preserve guest-visible endpoints and per-machine ownership. This phase does
  not add automatic external access, DNS, IPv6, TLS or metadata-service routing.
- Nonblocking `connect` returns success or an immediate error when completed
  synchronously; otherwise return `-1/EINPROGRESS` promptly while the socket owns
  the operation. Repeated connect while pending returns `EALREADY`; established
  sockets retain the selected Linux `EISCONN` contract. Audit nonblocking flag
  propagation through guest socket creation and `fcntl`.
- Blocking connect uses the same connection state and awaits completion without
  holding descriptor/network locks. Define interruption and existing send-timeout
  behavior explicitly; never turn a pending operation into fictitious success.
- `poll`/`select` and level-/edge-triggered `epoll` observe completion. A pending
  connection must not be reported as hung up solely because its remote endpoint
  has not been established. Update readiness epochs so completion is visible
  when registration happens before, during or after it, without lost events.
- Support `getsockopt(SOL_SOCKET, SO_ERROR)` through the bridge and descriptor
  layer. Return zero or the retained Linux guest errno, with get-and-clear
  semantics. Keep connection state separate from that error: zero while pending
  or after reading an error does not establish a connection. Readiness probes
  must not consume errors. Map BCL socket errors independently of host OS errno.
- The pending operation outlives the initiating syscall's wake/signal scope.
  Last-descriptor close and machine disposal terminate and drain owned work;
  duplicates retain the shared socket lifetime. Late completions must not mutate
  reused descriptor/handle entries or a subsequent machine execution. Preserve
  lock ordering and observe all task exceptions.
- Preserve working incoming Kestrel connections, normal read/write behavior,
  descriptor limits, policy denial and shutdown semantics.

Linux contracts: [connect(2)](https://man7.org/linux/man-pages/man2/connect.2.html)
and [socket(7)](https://man7.org/linux/man-pages/man7/socket.7.html). Host transport:
[BCL Socket.ConnectAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connectasync?view=net-10.0).

## Milestones and responsibilities

- [x] **N0 — Plan and initial inspection.** Existing connect rejects nonblocking
  sockets, `SO_ERROR` is absent, and readiness treats an unconnected socket as
  hung up. Record the implementation/test ownership with the coordinator.
- [x] **N1 — Managed socket contract.** Implement connection state, pending-task
  ownership, guest error mapping, SO_ERROR, readiness and descriptor lifecycle.
  Add focused meaningful checks and commit the working host change.
- [x] **N2 — Guest integration.** Carry the contract through authored bridges
  and the translated core. The actual static musl client uses `/dev/urandom`
  when `HttpClient` initializes `Activity`/`Guid`; supply a private BCL-backed
  read-only character device at that path in both execution modes. Keep it out
  of private image export/reset and host filesystem grants. Deliver an actual NativeAOT guest using asynchronous
  `HttpClient` against a BCL/managed local HTTP server through an explicit outbound
  grant. Run through the public machine API in both execution modes. Commit the
  fixture and any required integration fixes.
- [x] **N3 — Qualification and documentation.** Run relevant existing socket,
  query, async socket, timeout, poll/epoll, network-policy and lifecycle gates;
  qualify new behavior under JIT and NativeAOT consumers. Rebuild/postprocess the
  translated delivery as required and rerun the existing Kestrel/public-machine
  smoke checks in both modes. Update the API contract, this checklist, progress
  and validation ledgers with exact commands, receipts and limitations. Commit.

Implementation ownership: the network worker owns `VirtualTcpNetwork*.cs`, the
`GuestError` additions and `tests/NonblockingConnect`; the guest worker owns
`tests/HttpClientGuest`, `tests/NonblockingGuest` and
`scripts/test-nonblocking-guest.py`. The coordinator owns bridge integration,
documentation, shared generation/build serialization and final qualification.
The initial bridge audit found existing generic socket-option forwarding,
Linux `EALREADY`/`EINPROGRESS` constants and upstream `SOCK_NONBLOCK` to `fcntl`
propagation; no bridge changes are needed merely to expose those contracts. Other sessions' changes, especially
the active libsmb2/Kerberos/DFS work and shared package file, must be preserved.
Use narrowly staged commits; do not reset or include another session's work.

## Verification scope and acceptance

Use ordinary valid local TCP/HTTP interactions, native-oracle comparisons where
useful, normal cancellation/close, and independent/concurrent machines. Cover
successful completion, immediate/pending result handling, SO_ERROR visibility,
registration/completion races, nonblocking flags and descriptor ownership.
Avoid timing assumptions that require a loopback connection to take a minimum
duration. Lifecycle qualification uses the current public `MachineApi` valid-guest
cases; the legacy `InstanceLifecycle` synthetic protocol-failure suite is outside
this scope. Existing pinned upstream tests may supply error cases with provenance.
Do not add custom fault injection, forced resource/host failures, malformed ELF,
or invalid-ELF cases; do not weaken existing assertions to obtain a pass.

Completion requires the real translated NativeAOT guest to finish asynchronous
HTTP requests and clean shutdown in both execution modes, appropriate fresh
JIT/NativeAOT host-consumer evidence, and the relevant existing gates passing.
Preserve initial failures and classify any scope limits explicitly. Linux x64
execution does not qualify other platforms, full P7, IMDSv2, or snapshots.

## Completed delivery

N0–N3 complete on Linux x64. N1 commits: `e3ec54b`, `ea675df`; guest fixture/runner:
`8adaedd`, `4f37e33`; BCL entropy prerequisite and both-mode mounts: `e32c747`,
`a90539e`. The final delivery is `translation/attempt-cekeeech`; the new full
HttpClient matrix is `nonblocking-guest/attempt-sjizsmch`. All four forms pass
72 translated HTTP requests across twelve executions with simultaneous machines,
same-machine restart and release. Focused checks naturally observed both pending
and synchronous completion across retained runs.

Final public API: `machine-api/attempt-_13omhjd`; final Kestrel/general sample:
`machine-api-kestrel/attempt-d1jqgowt`. Eight selected host regression gates pass.
[VALIDATION.md](VALIDATION.md) records exact hashes, commands, preserved failures,
the isolated producer-tool boundary and coverage limits. The coordinator and
workers stop after this extension; IMDSv2 and P7 have not started.

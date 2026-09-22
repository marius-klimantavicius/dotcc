# Private I/O callback cancellation

The restored transient-wake callback changes passed native ABI/vector/poll smoke
and all four managed modes at
`artifacts/host-io-cancellation/attempt-7bwtbsow/receipt.json`
(SHA256 `d777809043e1b3e7cdf6061092b4b561adf1973754e7827038a9b85dac365564`).
Each mode passed 32 translated scenarios and one direct BCL pipe-read scenario
with exact stdout and empty stderr. All 15 commands succeeded; an independent
post-run check verified 208 source, tool, generated-file, log and execution
artifact identities. Epoll, sleep and guest signal delivery remain pending
separate validation.

The earlier permanent-cancellation fixture passed native ABI/vector/poll smoke
and all four managed modes at
`artifacts/host-io-cancellation/attempt-blvv0ho4/receipt.json`
(SHA256 `24085e30571d06b3fb41bdc3020cb960bcf708a5926c8323c0000b18a5065f6a`).
Raw JIT, raw NativeAOT, postprocessed JIT and postprocessed NativeAOT each passed
the 22 translated scenarios and one direct BCL pipe-read scenario with exact
stdout and empty stderr. Frozen sources, raw generated files, tool identities,
logs and execution binaries were rechecked after completion.

`BindHostIo(InstanceIo)` retains its original public method signature and default
behavior. The existing two-argument overload accepts an explicitly owned
`CancellationToken`, stores its value on the calling translated-C thread and
passes it to existing asynchronous read/write, vector, accept/connect,
send/receive, message and readiness operations. Unbind clears both owner and
token. The binding does not create or dispose the caller's token source.
The three-argument overload additionally binds an optional worker-owned
`HostSignalWake`; unbind clears that reference too.

Each potentially blocking operation borrows a fresh wake lease linked to the
permanent token. A signal request marks active leases and queues their
cancellation without invoking callbacks on the requesting thread. Pending
requests also interrupt leases registered before the next explicit guest signal
checkpoint. A checkpoint acknowledges pending requests without clearing the
interrupted state of an existing operation. Lease release and the dispatcher
claim source disposal under the same lock, so cancellation and disposal cannot
race to dispose the same source. The owner must quiesce operations before
disposing the wake helper; disposal drains its queued callbacks.

The four changed bridges are `src/Host/HostIoBridge.cs`,
`src/Host/HostNetworkBridge.cs`, `src/Host/HostMessagesBridge.cs`
and `src/Host/HostReadinessBridge.cs`. They continue borrowing C pointers
only during the synchronous call and use bounded owned byte arrays for async
work. Existing successful partial transfers remain successes; cancellation does
not replace a positive byte count or discard bytes already committed.

This fixture covers the private callback boundary. Permanent cancellation keeps
ECANCELED125; transient cancellation reports EINTR4 if the permanent token is
not canceled. Successful partial counts take precedence over either error.
This does not qualify guest signal delivery, CPU halt, a service worker, or a
general guest stop/deadline implementation. Actual guest
poll probes readiness with timeout0 and sleeps in an upstream loop; guest
nanosleep may retry EINTR unless CheckInterrupt observes guest state. Those
execution/sleep-loop bindings remain separate unqualified work.
Upstream Poll converts callback failures other than EINTR, including ECANCELED,
into POLLERR readiness; callback cancellation is therefore not proof of a guest
poll syscall cancellation result.

## Finite normal fixture

The C probe uses valid owned descriptors, buffers and addresses throughout.
Each managed mode runs exactly 32 translated scenarios and one direct BCL
pipe-read scenario; the receipt names every scenario. Native smoke and the
translated ABI check are recorded separately from these cancellation scenarios.
There are no injected provider failures, invalid buffers, forced exhaustion,
guest instructions or service/worker loop.

| Case | Evidence and boundary |
| --- | --- |
| Pipe read/readv | Empty pipe; actual pending-operation count; explicit caller cancellation; ECANCELED and unchanged read buffers. |
| Pipe write/writev | Pipe filled by ordinary writes; actual pending-operation count; cancellation commits no extra bytes. |
| Partial pipe write/writev | An 8192-byte write commits 4096 bytes to a 4096-byte pipe, remains pending, then is canceled. Positive 4096 result and all committed bytes must survive. |
| TCP accept/recv/recvmsg | Valid private listener/connection with no available peer input; actual pending socket counter; cancellation and unchanged result buffers/metadata. |
| Deadline | Caller schedules token cancellation after an already pending pipe/socket call; normal timer-driven cancellation is observed. No exact scheduling-latency claim. |
| Poll | Valid nonready pipe; the C worker remains incomplete before caller cancellation or a scheduled deadline. No public poll registration counter exists, so this does not claim its precise internal registration time. Failure preserves revents. |
| Send/sendmsg/connect | Valid resources with a token canceled before entry; ECANCELED is required. Separate default-token translated connect/send/message calls must succeed and transfer the exact reported prefix. No blocked-send/connect timing claim. |
| Binding reset | A canceled input read followed by unbind and the original one-argument rebind succeeds on the same owner. |
| Owner isolation | Two C threads own separate valid pipes and tokens. Canceling one leaves the other operation pending until its own cancellation. |
| Disposal | Disposing owners with pending pipe/vector or message receives must drain operations; translated threads finish and unbind; pipe storage returns to zero. |
| Transient wake | Pending read/readv, recv/recvmsg, accept and poll return EINTR with unchanged failure outputs. After an explicit checkpoint, an ordinary input read succeeds without replacing the owner or canceling its permanent token. |
| Wake before registration | Two requests made before the operation registers remain pending until its checkpoint. The blocked-operation cases use one request so a second request cannot legitimately interrupt the subsequent read after the checkpoint. |
| Permanent cancellation priority | A canceled permanent token and pending transient request retain ECANCELED. |
| Transient partial write/writev | The same ordinary 4096-byte committed prefix survives a transient wake, retaining the positive result and exact bytes. |

The recovered epoll and nanosleep bridges also borrow transient leases and retain
their existing EINTR cancellation contracts. This fixture copies only the four
I/O bridges listed above and does **not** qualify epoll, sleep, upstream signal
delivery, or execution-owner integration.

Compacting GC runs on each translated thread and while selected C calls remain
pending. All threads are joined after their C call before their result is
accepted. Fresh processes independently run raw JIT, raw NativeAOT,
postprocessed JIT and postprocessed NativeAOT.

## References and reproduction

Native Linux C smoke exercises ordinary valid pipe readv/writev/poll and reports
LP64 iovec/msghdr/pollfd layout plus ECANCELED's value. It has no equivalent of a
.NET CancellationToken, and its result is not presented as cancellation proof.
The direct BCL counterpart specifically verifies an actually pending private
pipe read returning GuestError.Canceled. Other rows assert the explicit private
callback contracts listed above through actual translated C calls.

```sh
python3 blink/tests/HostIoCancellation/run.py
```

Run only after source review and release of the shared build slot. The runner
copies all four bridges, authored C/C# fixtures, managed header profile and the
Host source/project into an isolated attempt. It records compiler/postprocessor,
native tool and dotnet executable identities, preserves native smoke separately
from the private callback transcript, and hashes execution binaries before and
after each managed mode. Raw generated files stay separate from the copy passed
to the ordinary semantic postprocessor; their hashes are checked at completion.
No generated source or shared compiler is patched. Build/runtime logs and failed
attempts are retained. Child command groups receive termination and a 10-second
cleanup grace on timeout/interruption; no injected timeout tests are included.
Every child inherits an attempt-local TMPDIR. The runner rejects unexpected
execution stderr and rechecks native compiler and dotnet identities at completion.

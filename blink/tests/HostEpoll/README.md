# Empty private epoll qualification

Native/common, private native layout, and raw/optimized JIT/NativeAOT passed in
`artifacts/host-epoll/attempt-ixes6bgg/receipt.json` (SHA-256
`3688001f5bcff43194a790bb50cc344ef8dc0349978ff388380131e98e2bda5d`).
All 21 commands exited zero. An independent review verified 374 recorded source,
tool, snapshot, log, object, generated-source and executable identities. This is
boundary qualification only; no guest-service success is claimed here.

The earlier `attempt-bkcnbn9o` remains failed: its native and four managed
executions passed, but its final recursive JIT-directory integrity check detected
195 new `linux-x64/` files added by subsequent AOT publishing. All seven original
JIT files were unchanged. `closure-failure-diagnosis.json` preserves this evidence.
The passing rerun executes hash-verified isolated JIT copies and retains the
strict final tree check. It also records the actual root `nuget.config` input.
No Host, bridge, header or semantic fixture change was required.

The selected synchronous .NET service's native trace creates an epoll descriptor
and starts an empty wait. It contains no epoll registrations or delivered events.
This boundary therefore owns a real, quota-counted empty descriptor and actual
zero, finite and indefinite waits. It does not implement registrations, edge
triggering or one-shot events: `epoll_ctl` on an otherwise valid request returns
`EOPNOTSUPP`. No event is fabricated and an empty epoll descriptor is never
reported readable or writable by private poll.

The private callback record is naturally aligned: size 16, alignment 8, data at
8. Linux x64's packed guest record is separately measured as size 12, alignment
1, data at 4. Upstream already translates the guest record to the host record;
the two ABIs are not asserted equal. `host-epoll.h` asserts the private layout.
The overlay selects create1/ctl/pwait; there is no nanosecond host pwait2 callback.
Existing upstream guest pwait2 conversion is outside this focused qualification.

`run.py` compares ordinary native create/CLOEXEC/dup/close, zero and finite empty
waits against raw/optimized JIT/NativeAOT. Events and surrounding bytes remain
untouched; successful calls preserve errno. Supplied wait masks are restored on
the calling worker. Managed-only checks cover explicit registration refusal,
caller cancellation, a surviving descriptor alias, final-alias close, and owner
disposal/drain. Cancellation becomes EINTR and preserves that errno through mask
restoration. Final-close interruption is a private lifetime contract, not a
claim about Linux concurrent close. No signal is injected and no malformed or
fault case is used. A timed wait is checked outside its call using monotonic
elapsed time, with broad bounds; this is not a performance benchmark.

The profile bounds maxevents to 1..1024 and pending waits to the descriptor limit.
Any negative millisecond timeout waits indefinitely. The underlying open
Description owns the empty set; dup aliases share it. Owner shutdown drains
pending operations before releasing state. The fixture discards its process on
a stuck worker and does not dispose owners still borrowed by that worker.

The runner uses private snapshots, records tool/source/log/executable hashes and
checks executed closures before and after. Optimized generation restores exact
original authored Host/bridge/fixture sources before build. It never changes the
compiler or active product profile. Build servers are disabled and temporary
files are isolated; SDK/NuGet caches and the native system toolchain are shared.
Run only after the coordinator releases the serial build slot:

```
python3 blink/tests/HostEpoll/run.py
```

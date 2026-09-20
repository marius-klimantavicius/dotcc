# Ordinary private socket timeouts

Native-first receipt `artifacts/host-socket-timeouts/attempt-7kpmkwuj/receipt.json`
(SHA256 `772a3ee2ba253e6c37d3452d840c23af9ec91ee272d12ee8ceb00013cb70b26e`)
confirmed the normal Linux behavior before managed execution. The subsequent
native + raw/optimized JIT/NativeAOT matrix passed:
`artifacts/host-socket-timeouts/attempt-w1lhum8k/receipt.json`, SHA256
`836b760fe3abfacbc58c6a031e0e65ae2c358e0482fde4d6af28cbdc9325720d`.
All 17 commands exited zero; independent review verified 361 source, tool,
snapshot, log, object, generated-source and executable identities. The bridges
and Host sources remained unchanged throughout both attempts. This qualifies the
ordinary boundary cases below, not a successful .NET guest service run.
This fixture targets the actual .NET service's SOL_SOCKET SO_SNDTIMEO_OLD (21)
and SO_RCVTIMEO_OLD (20) settings: a signed LP64 timeval, 16 bytes, seconds at 0
and microseconds at 8, set to five seconds. Integer socket options retain their
existing four-byte representation.

The normal native fixture runs first. It observes exact five-second set/get
round trips, shared settings through dup, accepted-socket inheritance, and zero
reset. Ordinary empty accept and connected recv/read/readv/recvmsg calls use a
one-second receive timeout; monotonic elapsed checks have broad 0.8..10 second
bounds. Native results determine EAGAIN and output-buffer/message behavior before
managed comparisons. Small successful bidirectional transfers exercise send/recv,
write/read, writev/readv, and sendmsg/recvmsg while tolerating actual short counts.
Each route runs with nonzero accepted-socket deadlines, then again after zero reset.
The actual upstream guest sendto path uses VfsSendmsg, covered by the existing
message bridge; no new sendto placeholder is introduced.

There is no forced saturation, peer delay, forced disconnect, signal injection,
invalid input or custom fault. Send-deadline expiration under backpressure is
**not exercised**. This fixture does not claim arbitrary .NET network support.

Private lifecycle cases use real pending accept and recvmsg calls with normal
caller cancellation and owner disposal. Their Canceled (125) result is kept
separate from native no-progress timeout EAGAIN (11). Every worker is joined and
pending operations drain before owners are released. On a stuck worker the
fixture fails and retains borrowed owners for process discard. The zero timeout
is exercised as an actual pending call subsequently released by owner shutdown.

The Host API rounds positive canonical timevals up to milliseconds, reports the
effective normalized value through getsockopt, and bounds that value to
int.MaxValue milliseconds. Zero means indefinite. Fractional rounding and the
maximum range are documented policy, not measured coverage of this whole-second
fixture. Accepted sockets inherit current listener settings; dup shares the
socket entry. Nonblocking guest flags remain outside the existing profile.
Nonzero send timeout on connect is explicitly unsupported rather than silently
claiming timed connect support. Poll/readiness deadlines remain separate.

After source review the coordinator first releases `run.py --native-only`.
A full `run.py` repeats native checks before raw/optimized JIT/NativeAOT. Receipts
separate native-only success from full qualification. Original authored Host,
bridges and consumer sources are restored after private postprocessing. Executed
closures are hashed before/after and at final review; JIT runs use isolated
copies so later publication cannot add files beneath a measured tree. Native
system libraries, SDK and NuGet caches are shared, not hermetic inputs. No shared
compiler build or active product generation is performed by this runner.

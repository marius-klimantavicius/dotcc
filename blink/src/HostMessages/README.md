# Private TCP message operations

getpeername reports the existing socket's peer in its owner's virtual IPv4
namespace. sendmsg/recvmsg gather or scatter up to1024 vectors through one actual
TCP transfer, bounded by the existing65536-byte chunk limit. They return the
actual short count and share descriptor ownership, cancellation and shutdown
with the existing private network. Peer lookup exposes no OS ephemeral address.

TCP receive returns no source-address bytes and commits namelen, controllen and
flags as zero after success. Native measurements include the fact that a
zero-capacity receive can wait for data or EOF; the managed path preserves that
wait and responds to owner cancellation. Error paths preserve output metadata.
Nonzero flags or control capacity return EOPNOTSUPP without dereferencing control
storage. An explicit send destination returns EISCONN by private policy. Ancillary
transport, descriptor passing, datagram and Unix sockets remain unsupported.

Fresh receipt `artifacts/host-messages/attempt-o2m20b_6/receipt.json` records
native/profile msghdr56/iovec16 layout checks, native behavior comparisons,
raw/optimized JIT/NativeAOT, two owners using the same virtual port through GC,
blocked-receive cancellation and the existing InstanceIo regression. The runner
records the complete compiler identity; older patch receipts are not reused.
The full-core binding manifest now includes this managed bridge. Actual guest
service startup and syscall-path qualification remain open.

# Managed lifecycle campaign

Run through the isolated Samba oracle, which supplies the same credentials and
arguments as the managed sample and preserves build/server/client receipts:

```sh
python3 libsmb2/scripts/oracle.py --suite lifecycle --managed jit
python3 libsmb2/scripts/oracle.py --suite lifecycle --managed aot
# Add --raw to exercise the unprocessed generated product.
```

The campaign checks Unicode directory/file operations, stat/fstat and filesystem
space metadata, ordinary missing-path errors, byte-perfect transfers over 1 MiB,
short reads and EOF, untouched buffer tails, empty I/O, pre-cancelled operations,
repeated disposal, concurrent independent connections, and pending writes racing
with disposal. SMB2.02 must require multiple negotiated transfers. Other dialects
may accept a large write in one call; reads deliberately extend beyond EOF.

A local TCP peer forwards real Samba authentication and open operations, then
forwards the next read request and resets both sockets while dropping responses.
The failed read must retire the connection before returning: accessing its
negotiated dialect and subsequent file operations must throw
`ObjectDisposedException`. The caller buffer must remain stable after completion
and a forced collection. The peer observes and drains its forwarding tasks; a
healthy connection deletes the remote file.

If a failed read leaves the connection live, the standalone test prints a clear
regression diagnostic and exits immediately. It deliberately avoids invoking
cleanup through the old implementation's dangling callback state. This exit is
only a failure path, never a passing substitute for cleanup.

Cancellation coverage is pre-cancelled operations. The pending-write race proves
work was pending and every task drained; it does not claim interception at a
particular socket instruction. The reset peer provides the separate deterministic
transport-failure boundary.

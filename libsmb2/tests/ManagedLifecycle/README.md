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

Cancellation coverage is pre-cancelled operations. The pending-write race proves
work was pending and every task drained; it does not claim interception at a
particular socket instruction.

The former custom TCP-reset case is excluded under the updated
[upstream test scope](../../docs/test-scope.md). Its historical results do not
extend the scope of the current default campaign.

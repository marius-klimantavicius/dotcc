# Signal sets and virtual host masks

`HostSignalOps.c` implements the public signal-set operations and sigprocmask
against the existing worker-local virtual delivery mask. It includes no host
signal API. This mask is separate from Blink's guest Linux blocked-signal state;
the upstream guest marshalling and signal delivery remain unchanged.

The measured Linux/glibc public domain is 1–64, with reserved thread signals
32/33 excluded from add/delete and full sets. Membership can inspect those bit
positions. Invalid numbers return EINVAL, and authored null-set guards return
EFAULT. Successful operations preserve errno. All 128 bytes of newly produced
sets are initialized; only the first 64 bits describe this profile's signals.

Block, unblock and replacement operations preserve the old mask on error. A
null input is a query and ignores the operation selector, matching native
behavior. SIGKILL, SIGSTOP and the two reserved thread signals cannot be blocked.
The same mask is captured/restored by the already qualified signal jump adapter.
No host signal handler, asynchronous delivery or OS process mutation is installed.
Signal handlers, kill/raise, suspension and alternate signal stacks remain
unimplemented isolated operations. The new callbacks do not complete those gates.

Run `python3 blink/tests/HostSignalOps/run.py`. Receipt
`artifacts/host-signal-ops/attempt-3gw_1n_o/receipt.json` records native POSIX,
native adapter and raw/optimized JIT/AOT agreement. Checks cover every public
bit, reserved and invalid numbers, unmaskable signals, block/unblock/replacement,
old-mask/query results, invalid-operation atomicity and restored initial state.
The managed hook independently observes the real host thread mask and forces
compacting GC between operations. Whole-consumer AOT rooting and CS8500 errors
are enabled; raw generated sources remain unchanged. This is focused callback
qualification, not evidence of a completed interpreter execution milestone.

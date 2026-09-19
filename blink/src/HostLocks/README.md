# Private file advisory locks

Shared/exclusive/unlock operations use the actual private node and open-file
description identities. Duplicates share locks; separate opens contend, and
closing the final duplicate releases its lock. Image bytes remain immutable;
locking their descriptions does not modify image metadata. Locks are advisory
and do not change ordinary read/write permissions. No host file or lock API is
called. Independent instances never contend, even for the same path.

Nonblocking contention returns EWOULDBLOCK. Like Linux flock, a failed
nonblocking conversion drops the old lock. Uncontended blocking calls complete
immediately; contended blocking waits are outside the single-worker contract
and return EOPNOTSUPP before starting conversion, preserving the existing lock.
Invalid operations/descriptors fail explicitly. Non-file descriptors are outside
this private file contract and return EOPNOTSUPP.

The last descriptor close immediately removes its lock entry, and acquisition
also defensively prunes closed descriptions. Retained entries are bounded by the
descriptor ceiling. The registry is cleared during owner disposal. Namespace rename/unlink retain the same nodes,
so they do not retarget existing locks to a newly created file of the same name.

The header follows the existing sigaction boundary convention: a function-like
alias separates the measured `struct blink_host_flock` tag from the CLR callback.
Bare C `flock` designators are outside this profile; direct calls and explicit
`blink_io_flock` callback pointers are qualified.

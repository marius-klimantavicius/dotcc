# Additional private process metadata

The read-only identity boundary now supplies saved/real/effective credential
triples, supplementary-group queries, process-group/session IDs, a hostname and
three selected sysconf values. These use the bound immutable HostIdentity; no
host process, credentials, hostname or system limits are queried.

The single private process is its own group and session leader. Queries for zero
or its PID return that PID; other PIDs return ESRCH. Real/effective/saved IDs are
all the documented private UID/GID zero. The supplementary group set is empty;
getgroups returns zero without writing an element, and rejects negative counts.
Credential output pointers are checked together before any output is written.
Mutation of credentials, groups, sessions and process groups remains unsupported
and isolated; these queries do not implement those deferred operations.

HostIdentity accepts an optional immutable hostName of 1–63 ASCII letters,
digits, dots or hyphens, defaulting to `blink-<private-pid>`. gethostname copies
that private name with a terminator. Insufficient capacity returns ENAMETOOLONG
without modifying the output, an explicit atomic private-boundary policy;
native glibc may leave a truncated output. A null destination is EFAULT. Before
binding, all new calls return ENODEV. Successful calls preserve errno.

Native selector values are `_SC_CLK_TCK=2`, `_SC_NGROUPS_MAX=3`, and
`_SC_PAGESIZE=30`, measured by tests/HostIdentityDetails/selectors.c against the
native headers. The corresponding private values are 100 process-accounting
ticks per second, zero supported supplementary groups and 4096-byte pages.
Page size matches the authored mapping adapter. Unknown selectors return EINVAL.
The tick declaration is a guest ABI value; process CPU accounting/times remains
a separate implementation gate. This is not a general POSIX sysconf surface.

Run `python3 blink/tests/HostIdentityDetails/run.py`. Receipt
`artifacts/host-identity-details/attempt-e9y3kpg6/receipt.json` records native C
invariants and raw/optimized JIT/AOT execution through the actual callbacks.
Native comparisons cover credential queries, valid group/session queries,
hostnames, limits and errors. The private checks additionally verify exact
instance values, empty groups, foreign process errors, unchanged failed outputs,
unbound state, and two separately named worker identities across compacting GC.
Whole-consumer AOT rooting and CS8500 errors are enabled. This qualifies host
callbacks; full interpreter startup and guest uname/auxv marshalling remain open.

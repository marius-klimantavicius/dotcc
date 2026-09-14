# Atomic descriptor replacement

The private dup2/dup3 callbacks replace a descriptor while holding the existing
InstanceIo table lock. They validate the source and target before changing the
table, retain the source description while releasing the old target, and share
its file cursor or socket lifetime. A full descriptor table can replace an
occupied slot without allocating another descriptor. No host descriptor number
or OS dup operation is used.

For valid identical descriptors, dup2 preserves per-descriptor flags; dup3
returns EINVAL, including identical invalid descriptor numbers as Linux does.
Successful replacement clears CLOEXEC for dup2 and sets it only when dup3 gets
O_CLOEXEC. Other dup3 flags fail EINVAL, invalid source/range fails EBADF, and an
unbound callback fails ENODEV. Target close errors are discarded as with dup2.

Native and all four managed modes pass the C file/cursor/flag/function-pointer
contract. Additional managed checks cover descriptor exhaustion, simultaneous
workers with GC, replacing the last socket reference and releasing its private
port, preserving a socket through another alias, and redirecting one captured
standard stream to another. HostFiles and InstanceIo regressions also pass.
Reproduce with python3 blink/tests/HostDescriptors/run.py; final receipt is in
PROGRESS.md. Earlier failed harness attempts retained an unnecessary metadata
bridge or used the wrong test-side socket method name; no generated source was
edited to bypass either build failure.

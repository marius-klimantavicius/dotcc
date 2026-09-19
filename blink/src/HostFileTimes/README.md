# Private file timestamps

`HostFileTimesBridge.cs` implements `blink_host_futimens` and
`blink_host_utimensat` over the existing bound `InstanceIo`. The opt-in
`host-file-times.h` declares the callbacks and native-measured sentinel values.
It needs no new owner or teardown step and never accesses host filesystem paths.

An explicit timestamp is stored exactly, including every nanosecond. Supported
seconds are `-62135596800` through `253402300799` inclusive (UTC years 1–9999),
with nanoseconds 0–999999999. Values outside that declared range return EINVAL;
no saturation or sub-tick truncation occurs. `VirtualFileStat` retains its tick
properties and adds 0–99 ns remainder fields. The metadata bridge reconstructs
normalized POSIX seconds/nanoseconds, including pre-epoch values.

Null `times` means both NOW. NOW uses one current UTC sample for both fields and
ctime; its provider has the existing 100 ns quantum. Explicit inputs may have
finer precision. OMIT ignores its seconds field and preserves that timestamp;
both OMIT preserve ctime. The entire pair is checked before any mutation, and
updates hold the same private filesystem lock as stat/read/write. Duplicated
file descriptors refer to the same node.

The native Linux oracle establishes a special all-OMIT behavior: utimensat
succeeds without resolving its path, directory descriptor or flags. futimens
still checks its descriptor. A disposed/unbound private owner remains an error.
Otherwise utimensat accepts flags 0 and AT_SYMLINK_NOFOLLOW only; the namespace
has no symlinks. Other flags return ENOTSUP. Absolute paths ignore dirfd;
relative paths use AT_FDCWD or an existing directory descriptor. No Linux
null-path extension or AT_EMPTY_PATH is selected. Immutable image files and
image directories reject actual changes with EROFS, including NOW; this is an
explicit private image policy, not a claim about host owner permissions.

Run `python3 blink/tests/HostFileTimes/run.py` from the repository root. The runner
compares native layout and shared semantics with raw/optimized JIT and NativeAOT
execution; private checks cover range limits, immutable nodes, unsupported
flags, exact remainders, forced GC, isolated owners and atomic failures. Copied
HostFiles and InstanceIo regressions cover the VFS representation change. Each
attempt records source, compiler and generated-source hashes. The separate
HostFileMetadata matrix should also be run after modifying its output bridge.

Qualification receipts in the isolated campaign worktree:

- `artifacts/host-file-times/attempt-ddqcd8yp/receipt.json`: native + all four
  managed modes and copied file/I/O model regressions passed.
- `artifacts/host-file-metadata/attempt-a5vfkobj/receipt.json`: existing complete
  native metadata layout/invariant comparison + all four modes and regressions
  passed with the new timestamp representation/output conversion.

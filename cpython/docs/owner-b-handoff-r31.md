# B31 file metadata — source-ready and lane released

The generic Linux x64 `stat`/`fstat`/`lstat` repair is ready for the next normal
compiler capture. Only the metadata block of `DotCC.Libc/PosixLib.cs` changes:
SHA256 `9117c38db685534d2145838fc0ffa347da11b66d22e8f3c419d6cc8ef0c7fc1b`.
All services beginning at `gettimeofday` remain byte-identical to the saved
pre-B31 source. Shared headers, file creation, B23 opendir/signal, B24 ownership/
resolver, B29 getcwd/Host, compiler and frozen Python cases remain unchanged.

The saved B30 RED is real: its direct-created files physically have modes
0600/0644/0664 and uid/gid1000, but old `FillStat` reports type-only mode and zero
identity/permissions. Unchanged importlib `_calc_mode` uses that false source
mode, adds0200, then `_write_atomic` creates the bytecode physically0200.
The resulting loader EACCES is legitimate. No permission check was weakened,
and no creation-mode propagation change was needed. Original
[native/actual commands and RED](../artifacts/owner-b-r30-20261018/reduction-commands.json)
and [unchanged fixture](../tests/RawReplayR30/pyc_permission_r30.py) are retained.

Independent C compilation measures the actual target header at **96 bytes**,
alignment8, with offsets0,8,16,24,32,36,40,48,56,64,72,80,88 for its13 fields.
The inherited104-byte description was inaccurate. An independently built binary
consumer of B30's actual library confirms those exact sizes/offsets. Linux host
`struct stat` is separately measured at144 bytes. The runtime uses a guarded
Linux x64 host structure and explicitly copies into the unchanged target layout.
It reports native permissions/type, device/inode/link count, uid/gid, size,
second-resolution access/modification/change times, rdev and block metadata.
`lstat` distinguishes final links, including dangling links. `fstat` retains the
owner's real `FileStream.SafeFileHandle` through its syscall and remains correct
after rename/unlink; synthetic owner descriptor indices never reach libc.
Native errno is preserved on failure, and failed calls leave output untouched.

Validation is focused current-Libc evidence:

- Independent native creation/metadata/descriptor/unlink control passes.
- Original604-check direct-runtime fixture fails266 checks before repair and
  passes all604 after repair. Expanded final fixture passes **685 checks in
  JIT and685 in NativeAOT**, zero failures.
- Checks compare all13 values to a separately compiled system-header oracle,
  including physical permissions, files/directories/symlinks, hardlinks,
  descriptor identity after rename/unlink, fchmod, explicit times, missing/
  inaccessible paths, ENOTDIR/ELOOP, invalid/null arguments, buffer guards,
  colliding owner-local descriptors, compacting GC and a real owner-bound worker.
- Permission-derived atomic output is physically readable and reopens with exact
  bytes. This is a generic libc reduction, not execution of Python's loader.
- Three new persistent metadata unit regressions plus affected file/path/owner
  tests pass **35/35**, zero skips.
- Existing actual emitted layout passes; it does not contain the new runtime.

The first native header probe failed because GCC lacks DotCC's builtin
`offsetof` spelling in the synthetic header. The fixture now supplies GCC's
builtin only for the native probe. Its original command/log remain retained;
no production header was changed. Existing VectorIo unassigned-field warnings
remain unrelated. The initial stackalloc test warning was removed in the final
expanded fixture.

[Commands and results](../artifacts/owner-b-r31-20261018/source-ready.json),
[source pins](../artifacts/owner-b-r31-20261018/source-pins.json),
[actual PE/PDB source bindings](../artifacts/owner-b-r31-20261018/actual-input-proof.json),
[actual ILC inputs](../artifacts/owner-b-r31-20261018/actual-ilc-input-proof.json)
and [preservation](../artifacts/owner-b-r31-20261018/preservation.json) identify
the exact exercised artifacts. The ILC proof binds its actual entry, all176
references, current-Libc PDB, native object and executed binary. Baseline PDB
checksums also verify the small preserved original source copies. No giant
backup, artifact cleanup, staging, commit, reset, worktree or upstream import ran.
The read-only upstream comparison finds the same type-only/alias limitations;
it supplied no repair or validation proof. See
[reference findings](../artifacts/owner-b-r31-20261018/upstream-reference-findings.json).

[RELEASE-ALL](../artifacts/owner-b-r31-20261018/serial-slot-release-all.json)
records16 terminal command receipts, no active/queued owned work and three
independent free-lock checks at transfer. A28 and the supervisor received the
exact source pin and safe handoff directly. B31 is now source/docs-only.

Next internally coordinated batch: A28 focused compiler validation, one normal
isolated capture including this runtime pin, relevant SAME-C emitted controls,
strict raw286/jobs8, then fresh actual-consumer startup and the **same unchanged
pyc fixture** plus the original first importlib failure before another required
suite. Direct current-Libc passes do not qualify a regenerated interpreter.
V17 tools/products stay frozen; processed remains stale V16 and embedded193 old
V11. B30's journal remains3804 started/3803 completed/3674 passed/38 errors/
88 required skips/3 required XFAIL,1064 without completed outcome, with natural
JSON recursion stack overflow. No required case/decorator/exclusion changed.
RecursionR27 diagnostics and production stack-safety scope remain unrun/open;
B31 did not retry A27's flagged work. Full final raw/processed JIT/AOT, owning/
ABI/thread/GC/lifecycle/rooted/trimmed/delivery/audit/new193/P6 gates stay open.

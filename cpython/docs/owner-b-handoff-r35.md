# B35 ftruncate — source-ready and execution lane released

The generic runtime repair is ready at `DotCC.Libc/PosixFsLib.cs`, SHA256
`19e0d945ea29730f87a644290ef5e1e95aea4ae181a6e3d371084ac8906b9397`.
Only the ftruncate method and its XML comment changed. Every other byte,
including B23 opendir and B29 getcwd, matches the retained original source.
B34's final facade/pthread pins and existing stat/file/header services are
unchanged. No compiler, framework, generated or canonical product source changed.

The native system-header C control demonstrates that shrinking, extending and
unchanged truncation preserve the current file offset, including offsets beyond
EOF and sparse positions above 2 GiB. The next write uses that same offset and
creates the expected zero-filled gap. Native errors are EINVAL for negative
lengths, read-only descriptors, pipes and sockets; EFBIG for LONG_MAX length;
EBADF for closed/invalid descriptors. Negative length is checked before even an
invalid descriptor on this Linux host. Error calls preserve offset and length.

Regular files retain the existing descriptor registry and owning SafeFileHandle.
On POSIX the method calls native ftruncate with that handle, preserving exact
native errno and avoiding a seek/restore race. Accessing the FileStream handle
flushes its BCL buffers; callers still explicitly flush C stdio before mixing
stdio and descriptor access. The Windows branch uses RandomAccess.SetLength;
it is unrun here. The non-FileStream fallback restores the original position
after successful SetLength. No FileSlot, buffering service or ownership change
was needed.

Validation is focused current-Libc evidence:

- Independent native control:136 checks, zero failures.
- Faithful unchanged current-Libc RED:147 checks,32 failures, including offset
  and errno mismatches. The repaired same fixture passes147/147 in Release JIT
  and147/147 in NativeAOT, with identical output.
- New position/stdio/owner regressions plus affected existing file durability,
  stdio and descriptor ownership tests:30/30, zero skips. The original unit run
  failed seven tests and passed23.
- Own actual entry/Libc PE identities, matching portable PDB/Compile sources
  and all176 ILC references, actual entry, native object and executed binary bind.
  Baseline PDB inputs were bound before the production edit. The29 KB original
  PosixFs copy remains available for historical V20 source checks.

An initial native harness incorrectly assumed LONG_MAX was a valid seek offset.
The filesystem rejected it; original source/logs remain retained. Its corrected
ftruncate-specific fixture keeps the negative/min/max length errors and valid
previous offset. No existing Python case, decorator, exclusion or skip changed.
Pinned upstream PosixFs has the same faulty SetLength implementation. Its
PosixFs/FileLib/unistd bytes were verified against the exact d3678b5c tree blobs;
no upstream code or architecture was imported.

[Source-ready evidence](../artifacts/owner-b-r35-20261018/source-ready.json),
[source pins](../artifacts/owner-b-r35-20261018/source-pins.json),
[actual inputs](../artifacts/owner-b-r35-20261018/fixed-actual-input-proof.json),
[preservation](../artifacts/owner-b-r35-20261018/preservation.json) and
[direct genuine safe release](../artifacts/owner-b-r35-20261018/serial-slot-release-all.json)
seal the exercised scope. All14 owned command PIDs are absent, active/queued
work is empty, and three independent locks were free at transfer. Commands took
41.898 seconds in total. A30 and the supervisor received the pin and release
directly; B35 has no further heavy work queued or planned.

Next internally coordinated work belongs to the assigned compile/product owners:
one normal capture with the exact runtime pin; SAME-C emitted JIT/AOT controls
using [the frozen native fixture](../tests/Abi/ftruncate_position_r35.c); strict
RAW286/jobs8 publication; then the actual unchanged12 original failing IDs and
full4867 required retry. Direct Libc passes are not product passes. Immutable
V20 proofs remain historical and were not retargeted. B34's complete unique
counts remain4751 PASS/12 FAIL/101 required SKIP/3 required XFAIL, with zero
errors/fatal/unrun;4759 pass events include eight duplicate IDs. Required skips
and XFAIL remain nonpasses pending native adjudication. Final new193/both-form
JIT/AOT/D10/ABI/owning/thread/lifecycle/rooted/trimmed/delivery/audit/downstream/
P6 gates remain open.

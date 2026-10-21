# Independent literal/builtin/filesystem review, round 2

Reviewer: `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
In-flight source review only. No builds, tests, native processes, Git mutations,
or implementation edits were performed by the reviewer. Both writers must be
held and actual matching execution receipts reviewed before acceptance.

The canonical finding IDs remain **M1L-01 through M1L-07** from round 1.
No additional high-severity finding is introduced by this note.

## M1L-01: refined header-scanning requirement

The provisional round-1 phrase "as soon as its opener is exposed" was too broad.
Actual `preproc.c:get_header_name` 1085–1145 distinguishes a literal top-level
header, scanned as one token, from a macro-produced angle operand assembled
from expanded preprocessing tokens. The later native receipts establish:

- `HI(<h.h>)` protects the literal `h`, even with `h` defined as another name.
- `HI(H)` with `H` defined as `<h.h>` expands the generated `h`; it can therefore
  report missing when the literal operand reports found.
- Literal header comment-looking characters belong to the filename. Closed,
  unclosed block-comment-looking text and `//` in a header must not be erased by
  a generic comment pass. A physical newline still terminates header scanning.
- The first closing greater-than ends an angle header; trailing tokens reject.

The prior "macro-opener parity" wording must not be read as identical raw and
expanded behavior. The supervisor preserved the corresponding native sources
and receipts and released the primary writer for the finite correction.

An additional discriminator was forwarded: scanning `__has_include` inside a
macro replacement list must not prematurely freeze a HeaderName that later
requires expanded-token assembly. Use `#define TEST __has_include(<h.h>)`, a
macro for `h`, and `#if TEST`, with actual native results defining the expected
branch. The current lexer arms HeaderName after every spelling of the operator,
so the corrected `HI(H)` control alone is not sufficient evidence.

## M1L-07: actual descriptor-path inspection

The authored Linux x64 adapter now opens with O_RDONLY|O_NONBLOCK|O_CLOEXEC,
wraps the returned descriptor in an owning SafeFileHandle, calls fstat on that
same handle, requires S_IFREG, and constructs FileStream from that handle.
There is no pathname reopen after descriptor validation. Directory is Missing;
special files and actual access/IO failures remain distinct failures. Error
capture follows each failed P/Invoke immediately. Returning through any normal
failure branch disposes the owned handle. Disposing the stream and then the
same SafeHandle object closes the descriptor once.

The installed x86-64 glibc headers independently match the interpreted fields
and flags: struct stat has mode at24, size at48, total storage144; O_NONBLOCK is
octal04000 and O_CLOEXEC octal02000000. Header SHA256 values inspected here:

- `/usr/include/x86_64-linux-gnu/bits/struct_stat.h`:
  `00b3b33c165454470de0f21b324867ec6e9b43e34f5d503121adf021cad8202e`.
- `/usr/include/x86_64-linux-gnu/bits/fcntl-linux.h`:
  `8d50bc7737bcc76f42b1f406244acb287bcf0463d180b6b0566d27d1f23b3e98`.

The official .NET10 runtime source confirms Unix SafeFileHandle accepts fd0,
releases through close, and the SafeFileHandle-based stream strategy retains
the supplied handle rather than opening its path. Sources independently read:
[SafeFileHandle.Unix.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs),
[FileStream.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/IO/FileStream.cs),
[OSFileStreamStrategy.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Strategies/OSFileStreamStrategy.cs).

The authored child launch supplies the tests DLL when ProcessPath is dotnet,
and directly launches ProcessPath under NativeAOT. It creates FIFO/socket/
symlink controls only under an owned temporary root, waits at most10 seconds,
kills a timed-out child and checks termination. Both configurations must
actually execute the wired group. This is not a general wall-clock bound for
ordinary/network filesystem operations, and lexical allowed roots are explicitly
not a symlink security boundary. No source-review observation closes M1L-07
without the finished handoff and matching executed evidence.

## Required finite proofs and policy corrections

The source implements first-entity signed extension for multichar packing, but
the initial managed assertions covered only `ab` and a single high byte. The
supervisor was asked to add actual graph values and PP comparisons for the
already native-witnessed `\377a`=-159, `a\377`=25087 and `\377\377`=-1.

The authorized timestamp policy is actual UTC captured once per session, with
an explicit immutable override for reproducibility. The round-1 positive
observation saying UnixEpoch was the default is historical and superseded.
Current CompilationSession captures the effective Options timestamp once;
preprocessing consumes that snapshot and retained results expose it. Do not
interpret the earlier suggested deterministic default as an approval rule.

The char/PP/builtin tests and filesystem tests remain unexecuted for this
batch at this review point. Full Unicode/UCN/execution encodings, strings,
floating/UDL values, other platforms/operators and all remaining PLAN rows
remain required work after this bounded gate.

# Byte-stream libc provider contract

This is an implementation milestone within the larger libc plan, not complete
stdio/POSIX conformance. Runtime tests currently execute on Linux x64 in JIT
and NativeAOT; Windows/macOS dispatch exists but still requires execution on
those platforms.

## Ownership and representation

`LibcFiles` belongs to one generated program owner. Each FILE is an opaque
native token looked up in that owner's dictionary; it is never a native libc
FILE pointer. Closing a stream releases its owned BCL stream and native buffer.
Its token remains a tombstone until owner disposal, preventing a stale token
from identifying a newly opened stream. Owner disposal closes remaining files,
flushes pending output, then releases the tokens.

The three addressable `stdin`, `stdout` and `stderr` pointer slots are pinned
owner globals. Internal FILE tokens and I/O buffers use NativeMemory. Buffers
supplied to `setvbuf` remain caller-owned. Standard BCL streams may be injected
by a managed host and are host-owned; independent owners have distinct FILE
objects and buffering/error state even when their output ultimately reaches
the same process console.

Generated lifecycle leases must enclose calls through direct exports and
function pointers. Dispose `LibcFiles` before its errno context because failed
shutdown I/O may still report an error. Function import contracts validate the
exact opaque `struct __edg_FILE` type and retain that C++ type identity at the
generated boundary; the runtime uses `void*` internally.

`fpos_t` uses version2: a signed 64-bit byte offset and two 32-bit decoder
state fields. This is not the host libc representation and cannot cross a
native libc boundary. See [wide FILE ABI](libc-wide-file-abi.md).

## Byte and positioning semantics

Text/binary modes both preserve bytes; these portable streams do not translate
LF to CRLF on Windows. Paths are strict UTF-8, resolved against the owner's
captured working directory. Arbitrary invalid-UTF-8 Unix filenames are not yet
part of this portable path interface.

Input and output use native buffers. Large bulk reads can bypass the input
buffer; small reads and `fgets` share readahead. Logical positions account for
unread input, one-byte pushback and buffered output. Successful positioning
clears EOF and discards pushback. `clearerr` clears stream indicators without
changing errno. Read errors and EOF remain distinct. An I/O failure discards
uncommitted output-buffer bookkeeping so a later flush does not duplicate an
unknown already-written prefix.

Update streams require a positioning/flush transition between output and input,
and positioning between input and output unless input encountered EOF. Invalid
sequences are diagnosed rather than implicitly treated as successful I/O.

The selected initial read position for `a+` is the beginning of the file,
matching inspected musl source and the Linux native synthetic oracle. Ordinary
`a` initially reports end-of-file. The C implementation choice does not affect
append writes: every actual append write targets the then-current end despite
intervening seeks.

## Why a few operations need OS adapters

Ordinary opening, reading, writing, seeking and flushing use BCL FileStream or
Stream. Small native operations are selected at runtime where BCL behavior
does not supply the C contract:

- **Append:** .NET 10's Unix handle implementation maps `FileMode.Append` to
  creation flags without `O_APPEND`, and its stream strategy maintains a
  managed offset. The provider uses Unix `O_APPEND` and `write`, or Windows
  `WriteFile` with both offset words set to all ones. It then synchronizes the
  managed position from that descriptor's actual resulting offset. This avoids
  a separate seek-to-end/write race. The OS/filesystem still determines its
  native append guarantees; a multi-call fwrite is not promised to be one
  indivisible transaction. [Runtime handle source](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs),
  [runtime stream strategy](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Strategies/OSFileStreamStrategy.cs),
  [WriteFile contract](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-writefile).
- **Seeking append streams on Windows:** normal BCL-opened write rights are
  retained. An append-only access mask would not meet SetFilePointerEx's
  documented access requirement; the explicit append offset provides append
  behavior without that restriction.
  [SetFilePointerEx contract](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfilepointerex).
- **Remove:** File.Delete suppresses missing-file errors. Native removal
  preserves failure and errno and also supports removing an empty directory.
- **Rename:** native rename avoids a race between probing whether the source
  is a file or directory and choosing separate BCL operations. Unix uses
  `rename`; Windows uses `MoveFileExW` with replacement and without a fallback
  cross-volume copy. Existing-destination directory replacement remains subject
  to the selected platform's rename behavior.

Static source-generated imports are NativeAOT-compatible and invoked only in
their selected OS branches. Apple arm64 `fcntl` needs a dedicated argument
layout because its variadic argument is stack-passed. Synthetic clang output
checks that layout, but this does not replace executing the adapter on macOS.

## Verification and remaining scope

Synthetic tests cover byte values including NUL/high bytes, buffering and line
flushes, small and partial reads, input readahead, seek/tell/pushback, EOF versus
injected errors, update streams, `fgets`, owner/global isolation, double-close
diagnostics, Unicode path rename/removal, shutdown and concurrent append via
independent handles. Native differential tests use original EDG's C output.
They deliberately do not exchange this provider's fpos_t with native libc.

Formatting, scanning and wide orientation have separate accepted components;
see the [completion ledger](libc-completion-ledger.md). Descriptor interoperability,
every platform error condition and cross-platform runtime acceptance remain open.


## Accepted stream management (2026-10-03)

`LibcFilesManagement` supplies `freopen`, `tmpfile`, `tmpnam`, `rewind`, and
`setbuf`. The authored `libc/stdio_management/perror.c` resolves owner/TLS errno
through `strerror` before calling the strict raw-output bridge. The implementation
was audited against pinned musl 1.2.6 `src/stdio/{freopen,tmpfile,tmpnam,rewind,perror}.c`;
these are BCL adaptations, not copied musl algorithms.

Successful `freopen` keeps the original FILE token, dictionary Entry, monitor,
and caller-owned buffering storage. Waiting operations therefore observe the
new association under the same lock. Pending output is flushed; orientation,
captured encoding, decoder/pushback, EOF and error state are reset. A failed
reopen closes the old association and preserves the opening errno. Host-injected
standard streams remain host-owned, while the replacement file becomes owned by
the FILE. Standard pointer cells retain their identity. Service disposal waits
for the same monitor and closes any replacement published before it.

The implementation-defined `freopen(NULL, mode, stream)` profile changes
logical access and append mode without truncating/repositioning the underlying
file. It does not upgrade rights beyond the original handle. Unix append uses
the existing runtime-selected fcntl adapter; Windows selects the accepted EOF
write operation. This choice is based on musl's null-name mode-change behavior,
and does not promise glibc's platform-specific extension semantics.

`rewind` clears the error indicator even when its seek fails; failed positioning
does not clear EOF. `perror` holds the stream lock across the prefix, colon,
error text and newline, using raw byte writes so an already-wide stderr retains
its captured encoding and orientation. `setbuf` uses the existing buffering
contract with BUFSIZ or unbuffered mode.

`tmpfile` creates a fresh read/write BCL file with exclusive creation, Unix user
read/write permissions and delete sharing, then removes its pathname immediately.
It transfers the still-open handle into FILE ownership. This is necessary because
.NET 10.0.11's Unix `DeleteOnClose` unlinks during SafeFileHandle release, which
`Environment.Exit` need not run. No DeleteOnClose pathname callback is retained:
it could unlink a later replacement name after an explicit early removal.
The composition uses BCL File.Delete on both Windows and Unix; runtime tests
currently prove Linux behavior only.
[Exact installed runtime source](https://github.com/dotnet/runtime/blob/v10.0.11/src/libraries/System.Private.CoreLib/src/Microsoft/Win32/SafeHandles/SafeFileHandle.Unix.cs),
[BCL delete sharing](https://learn.microsoft.com/en-us/dotnet/api/system.io.fileshare?view=net-10.0),
[File.Delete contract](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-10.0).

`tmpnam` uses an owner-pinned 20-byte buffer when called with NULL; another call
on the same owner overwrites it. The pointer stays valid until its owner is disposed; disposal drops the pinned
array reference even if the managed owner itself remains alive. Caller-buffer calls copy into that buffer instead. A per-owner randomized
sequence produces distinct 19-character relative names; filesystem availability
is checked against the owner's captured working directory. It does not reserve
or create the named file. Operations serialize under the owner monitor, but a
caller retaining the shared buffer must still coordinate other tmpnam calls.
These are owner global bytes, not short-lived pinned allocation.

Focused standalone evidence is retained in
`artifacts/campaign/libc-stdio-management-runtime-20261003-051800/results.json`:
strict JIT and NativeAOT cover named/null reopen, failure, queued operations,
orientation, errno, temporary-name ownership and actual process exit with an
open anonymous temporary file. The first native public fixture is retained in
`libc-stdio-management-native-20261003-050400`. The final runtime fixture also covers queued reopen against service disposal
and verifies the pinned temporary-name array becomes collectible while the
disposed service remains strongly referenced.
`libc-stdio-management-translated-20261003-051100/results.json` records the
native public oracle and all eight C/C++, static/instance, JIT/NativeAOT modes.
`libc-stdio-management-abi-20261003-051000/results.json` records eight malformed
FILE/character-pointer/result contracts rejected before C# emission. The first
generated attempt at `051000` retains a host-harness missing-parenthesis error;
the generated library itself compiled. No backend source correction was needed.

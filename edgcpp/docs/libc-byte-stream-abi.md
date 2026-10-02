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

`fpos_t` is provisionally a signed 64-bit byte position. This is not the host
libc fpos_t representation and cannot cross a native libc boundary. Wide-stream
orientation and multibyte conversion state will require an ABI update before
the final portable target is frozen.

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

Still outside this slice: formatted/variadic I/O, scanning, wide stream
orientation, locale conversion state, freopen/fdopen/descriptor interoperability,
temporary-file APIs, every platform error condition and cross-platform runtime
acceptance. These remain required work in the full plan.

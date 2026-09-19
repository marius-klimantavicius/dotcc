# Managed consumer API

`src/Managed/ManagedSmb.csproj` provides `Managed.Smb.SmbConnection` over the
translated `LibSmb2` class. Its ordinary project reference defaults to the final
post-processed project. The separate raw comparison path is selected only by the
test harness through `Libsmb2GeneratedProject`.

Dialect defaults, NTLMSSP selection, signing mode, read/write open mode and
directory type use named constants or enum members exported by `LibSmb2`.
`O_CREAT` and `O_EXCL` are absent from this generated closure; the facade keeps
two named fallback constants from dotcc's selected Linux `fcntl.h` ABI. Boolean
enable arguments and sample payload dimensions are not duplicated protocol codes.

This initial facade owns a connection and its open files. It exposes connection,
directory enumeration, exclusive file creation/open, offset reads/writes, flush,
truncate, metadata, free-space queries, rename, directory creation/removal, and
deletion. Directory entries are copied to managed values before the upstream
directory is closed. Reads and writes return actual byte counts: callers must
handle short operations. The sample loops until its complete payload is verified.
The broader low-level C API remains available on `LibSmb2`; APIs without execution
evidence remain unqualified.

Each connection serializes calls. Independent connections can execute concurrently;
creation and destruction also serialize the upstream global context registry.
Task methods schedule the facade's serialized event pump on the thread pool.
The pump submits upstream asynchronous requests and drives translated readiness
and servicing until their real callbacks complete. Callback state, pinned arrays
and stack outputs stay alive until completion or terminal context destruction.
This does not yet expose multiple outstanding requests on one context or a
nonblocking Task scheduler; those plan gates remain open.

Cancellation before a call starts prevents it. Cancellation during a call waits
for that call to finish. A successful operation is then reported as cancelled;
a protocol, transport, or deadline error takes precedence and is reported as
`SmbException`. Buffers remain pinned until the actual operation returns. A write
may have completed before cancellation is reported; cancellation never promises
rollback. Connection cancellation after successful connection closes the result
before reporting cancellation.

Dispose closes owned file handles, disconnects, and destroys the context under
the connection lock. DisposeAsync schedules the same cleanup on the thread pool.
Concurrent disposal waits for an active operation to drain. Transport failure or
a deadline destroys the context and drains queued callbacks before releasing
buffers; subsequent operations reject the disposed connection. Completed ordinary
protocol errors retain the connection. The finalizer destroys an abandoned context
without graceful network operations and releases its idle file allocations. Explicit
disposal is required for timely cleanup and for observing close/disconnect errors.

Credentials are explicit; the sample reads the password from `LIBSMB2_PASSWORD`
and the domain from `LIBSMB2_DOMAIN` (default `WORKGROUP`). No password is placed
on the command line or printed by the sample. Upstream setters copy UTF-8 strings;
temporary password bytes are cleared after configuration. The caller's immutable
.NET password string and upstream credential allocation follow their respective
runtime/upstream lifetimes and are not guaranteed to be erased immediately.

The facade requests signing and an exact dialect, with optional encryption.
Authentication uses built-in NTLMSSP. A negotiation mismatch or upstream failure
throws; `SmbException` retains the upstream status where the C API returns one.
Pointer-returning C APIs expose their error text without a reliable numeric status.
Real Samba qualification is tracked in [validation.md](validation.md).

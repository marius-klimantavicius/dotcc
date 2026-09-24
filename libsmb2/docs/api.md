# Managed consumer API

`src/Managed/ManagedSmb.csproj` provides `Managed.Smb.SmbConnection` over the
translated `LibSmb2` class. Its ordinary project reference defaults to the final
post-processed project. The raw comparison path is selected by the test harness
through `Libsmb2GeneratedProject`.

The facade owns a connection and its open files. Every operation has an awaitable
API; synchronous convenience methods block on the same implementation.

| Connection API | Purpose |
| --- | --- |
| `ConnectAsync` | Resolve the original server asynchronously, connect, authenticate and select a share. |
| `OpenAsync` | Open an existing file, or create a new file exclusively with `create: true`. |
| `ListAsync` | Copy directory entries to managed values and release the upstream directory. |
| `StatAsync`, `GetSpaceInfoAsync` | Read file metadata and share space information. |
| `RenameAsync`, `DeleteAsync` | Rename or delete a file. |
| `CreateDirectoryAsync`, `RemoveDirectoryAsync` | Create or remove a directory. |
| `DisposeAsync` | Close owned files, disconnect, destroy the context and drain host socket work. |

`SmbFile` implements `IAsyncDisposable` and exposes `ReadAsync`, `WriteAsync`,
`FlushAsync`, `StatAsync` and `TruncateAsync`. Reads and writes return actual byte
counts; callers must handle short operations. The sample loops until its complete
payload has been written and verified. Use `await using` for both connections and
files. Each operation accepts a cancellation token except resource disposal, which
must finish cleanup. `Dialect` returns the negotiated dialect.

Dialect defaults, authentication mode, signing, open flags (including `O_CREAT`
and `O_EXCL`), directory types and event constants use generated names. The socket
ABI uses the authored four-byte `t_socket` struct. Its integer value is a registry
token owned by this transport; it is not a native descriptor or a Libc file handle.
The explicit conversion to `int` preserves audited source compatibility boundaries.

## Scheduling and memory ownership

A connection admits one operation at a time through an asynchronous semaphore.
Independent connections can proceed concurrently. A serialized executor runs short
translated submission, callback and service turns. Between completions it owns no
waiting thread. Socket connect, send and receive use the BCL asynchronous APIs;
there is no managed `poll`/`select` loop or `Task.Run` around blocking network work.
Synchronous convenience calls are rejected from their own executor before they
can submit work.

The facade registers `smb2_fd_event_callbacks` before connecting. Host notifications
are queued and coalesced; translated code is serviced from actual completions and
latched buffer state. One-shot timers handle the upstream Happy Eyeballs deadline
and the active managed operation deadline. Upstream PDU timers remain disabled.
DNS is awaited before submission, and a scoped prepared address list supplies the
translated resolver without changing the original server identity.

Each submitted operation owns a rooted callback token, unmanaged UTF-8 text and
output storage. Transfers use an owned copy of the application buffer; successful
reads copy the actual returned bytes back. These allocations survive asynchronous
submission until the callback has settled, or until terminal context destruction
and host I/O drain finish. No borrowed stack pointer or lexical pin is retained
across an await. Socket-side buffering is separately bounded and owned by the host.

Cancellation before submission prevents the operation. Cancellation after submission
waits for the callback or terminal cleanup. A successful operation is then reported
as cancelled; a protocol, transport or deadline error takes precedence. A newly
opened file is closed before cancellation is reported. A write may have completed
before cancellation is observed; cancellation never promises rollback.

Disposal stops new operations, waits for the active operation, closes owned handles,
disconnects and destroys the context on its executor. It then awaits outstanding
host I/O before returning. Repeated disposal shares the same cleanup task. A
transport failure or deadline causes terminal destruction; later operations reject
the disposed connection. Ordinary completed protocol errors leave it available.
The existing upstream compound-metadata cancellation cleanup limitation remains
unqualified; completion-driven transport does not repair that upstream ownership
path. The finalizer schedules local destruction and idle-handle cleanup without a
graceful network exchange. Explicit disposal is required for timely cleanup and
for observing close/disconnect errors.

## Authentication and low-level compatibility

Credentials are explicit. The sample reads the password from `LIBSMB2_PASSWORD`
and the domain from `LIBSMB2_DOMAIN` (default `WORKGROUP`). It does not put the
password on the command line or print it. Upstream setters copy their input text;
the operation's temporary unmanaged password storage is cleared on release. The
caller's immutable .NET string and upstream credential allocation follow their
respective runtime/upstream lifetimes and are not guaranteed to be erased immediately.

The facade requests signing and an exact dialect, with optional encryption.
The default connection uses built-in NTLMSSP. Kerberos and DFS entrypoints are described below; enterprise interoperability qualification remains open. A negotiation
mismatch or upstream failure throws; `SmbException` retains the upstream status
where the C API provides one. Real Samba qualification is recorded in
[validation.md](validation.md).

The translated low-level API remains visible, but the default product profile
requires the authored host and callback-driven service contract. Legacy C synchronous
wait functions and server-hosting loops are unsupported by that profile and their
wait adapters fail explicitly. They must not route `t_socket` registry tokens into
Libc's descriptor table. The isolated legacy translation profile exists for
unchanged upstream regression tests; it is not a fallback for the managed facade.

## Kerberos and DFS APIs

`ConnectKerberosAsync` accepts a principal, password and realm.
`ConnectKerberosWithKeytabAsync` accepts a principal, realm and keytab path.
`ConnectKerberosWithCredentialCacheAsync` accepts an absolute FILE cache path;
`ConnectKerberosWithExistingCredentialsAsync` uses the Windows logon via SSPI or
`KRB5CCNAME=FILE:/absolute/path` on other systems. Each has a synchronous counterpart.
Kerberos selection never falls back to NTLM. DNS resolves the supplied host while
`cifs/<host>` preserves its service identity, excluding an optional port suffix.
Managed credential modes support completed BER SPNEGO responses with or without
AP-REP and require the final SMB session-setup signature. Windows logon uses SSPI
Negotiate and verifies that the selected package is Kerberos with mutual
authentication. Signing/encryption uses the established context key. Provider acquisition
cancellation and Windows SSPI limitations are in [enterprise-client.md](enterprise-client.md).

`SmbDfsClient.Create*` factories support the corresponding credentials and retain
connections until client disposal. Await `ResolvePathAsync`, `ListAsync`,
`OpenAsync`/`OpenReadAsync`, `StatAsync`, `GetSpaceInfoAsync`, `RenameAsync`,
`DeleteAsync`, `CreateDirectoryAsync`, `RemoveDirectoryAsync` and `DisposeAsync`.
Returned files use the existing async file API. Returned targets contain the
server, share and relative path; pass the original namespace UNC to later DFS
operations. `SmbException.NtStatus` preserves the callback's original protocol
status where supplied. Direct shares are used when the server reports that DFS
referrals are unsupported or absent.

Credential factories trust the namespace's target selection. Transient errors
after an operation is submitted are propagated without replaying that operation.
No automatic credential delegation or Kerberos-to-NTLM fallback is requested.

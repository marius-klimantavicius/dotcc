# Kerberos/DFS patch review and merge

The supplied `Add_kerberos_support.patch` targeted the facade before the async
transport merge. Its library features were manually integrated; the unrelated
SQLite source split-size change and checksum-bypassing `--no-fetch` option were
not applied. The verified pinned-source acquisition remains unchanged.

## Corrections made during review

- Kept the completion-driven socket host and int-backed `t_socket`. New Kerberos
  methods acquire tickets before entering translated callbacks; DFS operations,
  DNS discovery and cleanup use awaitable operations rather than `Task.Run` over
  blocking calls. The Libc/poll profile remains NTLM-only and separately generated.
- Preserved the callback's NT status and message before later servicing/destruction
  can overwrite it. IOCTL request storage lives until callback/destruction and its
  output allocation is released through translated `smb2_free_data`. Truncated
  (`STATUS_BUFFER_OVERFLOW`) referrals are rejected rather than cached as complete.
- Required Kerberos mutual authentication and made final provider failure sticky.
  Upstream ignores the final `krb5_session_request` return in one success path;
  key retrieval therefore fails the managed service turn if authentication did
  not complete, including when encryption has disabled the signing flag.
- Replaced the package's current-logon `SspiContext` helper with a narrow Windows
  SSPI adapter. The pinned helper's defaults request delegation and do not request
  mutual authentication. The authored adapter selects the Kerberos package,
  requests/verifies mutual authentication and never requests delegation. This
  path depends on Windows `secur32.dll` and still requires Windows execution.
- Used the translated allocator for session keys freed by the translated engine;
  auth-state/token allocations retain paired managed allocation/free ownership.
- Restricted DFS retries to failed connection establishment and explicit
  `STATUS_PATH_NOT_COVERED`. An ambiguous operation failure is not replayed on a
  different target. Referral-only intermediate responses do not become storage
  cache hits. Encryption requests also cover IPC$ referral connections.
- Checked that referral strings follow the complete entry table and that entries
  agree on version. Canonical paths reject dot components; UTF-16 path consumption
  preserves surrogate pairs and component boundaries.
- Updated dependency/source inventories to include the authored bridge and
  centrally pinned package. Removed only the unused transitive logging generator
  rejected by the semantic postprocessor; no generated C# patching is used.

## Provider provenance and boundaries

`Kerberos.NET` **4.6.168**, MIT, is centrally pinned. Its NuGet metadata identifies
source commit [`b55166eb991c47e32826e9d955fa9d13cc4a3a92`](https://github.com/dotnet/Kerberos.NET/tree/b55166eb991c47e32826e9d955fa9d13cc4a3a92).
Declared dependencies are Microsoft.Extensions.Logging.Abstractions 8.0.2,
System.Memory 4.6.0, System.Security.Cryptography.Pkcs 8.0.1 and
System.Threading.Tasks.Extensions 4.6.0; resolved dependency versions/hashes are
recorded in the generated NuGet assets and restore metadata. Transitive package
internals are not proven native-free by the product source inventory.

Explicit passwords/keytabs and FILE caches use the same managed provider on
Windows and Linux. Protected Windows logon reuse uses the authored SSPI adapter;
KCM, KEYRING and DIR caches are unsupported. Kerberos-only selection does not fall
back to NTLM or guest authentication. The declaration-only GSS/Kerberos headers
describe this translated closure, not the ABI of a native Kerberos library.

Ticket acquisition precedes the SMB operation timeout. The pinned provider's
`Authenticate` has no cancellation overload: cancellation waits for its request
to settle before disposing credentials. Windows SSPI itself is synchronous; no
worker is used to hide that platform call. DFS referral servers are trusted to
select targets. Comprehensive target-set/failback behavior and intermediate
controller failover remain outside the current qualification.

Protocol references: [DFS referral entry rules](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-dfsc/c5c1db02-c74f-4f05-ac6b-72c1f5b9a4a6)
and [SSPI context establishment](https://learn.microsoft.com/en-us/windows/win32/secauthn/initializesecuritycontext--negotiate).

## Validation

The staged raw project and facade build without warnings or errors. Parser tests
pass for request encoding, multiple targets, Unicode paths and invalid boundaries.
An isolated ordinary Samba fixture passes two links to distinct shares, Unicode
suffixes, cached referrals and direct-share reads through the actual translated
IOCTL/async socket path. This uses NTLM and proves neither Kerberos nor AD-domain
DFS interoperability. Reproduce it with `python3 libsmb2/scripts/test-dfs.py`
(and `--raw`/`--aot` for other products).

The original patch reported a Windows logon/DFS success but supplied no execution
receipt. That report is not adopted as independently verified evidence. Explicit
password, keytab, FILE-cache and Windows-logon Kerberos against a real KDC/SMB
server, including signing/encryption and raw/processed JIT/NativeAOT on both OSes,
remain acceptance gates. No enterprise credentials are available in this review.
The staged DFS fixture passes JIT and NativeAOT. Whole-assembly-rooted publication
also completes, but Kerberos.NET emits **IL2104** (trim analysis) and **IL3053**
(AOT analysis). The product audit deliberately continues to reject those warnings;
its gate is not weakened. Consequently the prior NTLM-only whole-assembly audit
is historical and does not qualify the expanded Kerberos product. Normal DFS
NativeAOT execution does not prove Kerberos acquisition is trim/AOT-safe.
Final regeneration and regression receipts are recorded below when complete.

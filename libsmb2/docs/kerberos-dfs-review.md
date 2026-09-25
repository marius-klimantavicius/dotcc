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
- Made final provider failure sticky. Managed password/keytab/FILE-cache modes
  use non-mutual GSS and accept a BER SPNEGO Kerberos `accept-completed` response
  without AP-REP, retaining the service-ticket key. An AP-REP, when present, is
  decrypted and validated and its returned key is used. Missing/rejected/incomplete
  SPNEGO negotiation does not authorize key export. A signed final SMB session-setup
  response is required even when encryption has disabled the signing flag;
  translated libsmb2 verifies its signature after deriving the signing key.
  Upstream ignores the final `krb5_session_request` return in one success path;
  failed/incomplete providers therefore throw during key retrieval.
- Replaced the package's current-logon `SspiContext` helper with a narrow Windows
  SSPI adapter. The pinned helper's defaults request delegation and do not request
  mutual authentication. The authored adapter selects the Negotiate package to
  consume SMB's SPNEGO tokens, requests/verifies mutual authentication, then
  requires `SECPKG_ATTR_NEGOTIATION_INFO` to identify Kerberos before exporting
  `SECPKG_ATTR_SESSION_KEY`. It never requests delegation. This
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
  centrally pinned package. The temporary transitive logging-generator exclusion
  was subsequently removed when the semantic postprocessor gained source-generator
  support; no generated C# patching is used.

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

The token-format corrections have a local regression executable at
`tests/KerberosTokens`. It exercises BER completion-only responses, optional
cryptographically valid AP-REP/subkeys, rejected/incomplete/wrong-mechanism
responses, the signed-session-setup requirement and SSPI completion policy.
These checks need neither domain credentials nor a KDC. They do not exercise
Windows SSPI native calls or establish real-domain interoperability; those
remain remote-environment acceptance gates.

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
(AOT analysis). At the time of this review, the product audit rejected those warnings.
Consequently the prior NTLM-only whole-assembly audit
is historical and does not qualify the expanded Kerberos product. Normal DFS
NativeAOT execution does not prove Kerberos acquisition is trim/AOT-safe.
Final Linux x64 results:

- Full 53-unit raw/processed regeneration and the updated solution build pass;
  solution build has zero warnings/errors. Postprocessing is idempotent on a
  private copy and preserves imported authored sources.
- All **88/88** existing managed Samba sample/lifecycle cases pass across
  raw/processed JIT/NativeAOT. These exercise NTLMSSP signing/encryption, not Kerberos.
- The new standalone DFS fixture passes all **four** raw/processed JIT/NativeAOT
  variants. Codec tests and both finalizer cases in each JIT variant pass.
- All **45 crypto/ABI values** match native controls in raw/processed JIT/NativeAOT.
- Static source/dependency audits pass for both variants: 17 generic OS imports
  plus six explicit Windows SSPI imports. The rooted raw NativeAOT binary builds
  and executes, but the audit correctly fails on Kerberos.NET IL2104/IL3053.
- The existing legacy project still evaluates with no authored host/Kerberos
  compilation sources or package references. No compiler or upstream C source
  changes were needed by this merge.

The aggregate `artifacts/kerberos-dfs-review/result.json` records the patch hash,
source/receipt fingerprints, resolved package versions/hashes, passed regression
runs and unqualified gates separately. `regressions.json` contains the exact 18
commands; DFS receipts are under `artifacts/dfs/`. The audit failure described
above is historical; `artifacts/product-audit/result.json` reflects the latest run.

## Updated audit policy (2026-09-25)

At the user's request, the product audit now ignores the assembly-level IL2104
and IL3053 summaries specifically naming `Kerberos.NET`. Build logs and the
receipt retain these diagnostics in `aot_analysis_warnings` and
`ignored_aot_analysis_warnings`. Other analysis warnings remain blocking and are
listed in `blocking_aot_analysis_warnings`. This changes warning acceptance;
it does not establish Kerberos.NET runtime interoperability under NativeAOT.

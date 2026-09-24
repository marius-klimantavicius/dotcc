# Translated upstream tests, Kerberos, and DFS

Status: the supplied Kerberos/DFS patch has been reviewed and merged into the
completion-driven transport. Explicit passwords, keytabs and FILE caches use
Kerberos.NET 4.6.168; current Windows credentials use a narrow Kerberos-only SSPI
adapter. DFS resolution uses the translated IOCTL path and awaitable operations.
See [patch review and validation](kerberos-dfs-review.md) for fixes and current
execution evidence. Cross-platform enterprise qualification remains open; the
patch's reported Windows result was not independently reproduced here.

## Translate the upstream tests

The pinned `tests/` directory contains 34 files, including 13 C sources and 18
shell test cases. `scripts/inventory-upstream.py` inventories these but explicitly
does not execute them. Existing authored differential suites are separate evidence.

Translate the upstream C test programs against the translated library. Implement
the surrounding test runtime in C#: arguments, environment, temporary files,
standard streams, exit status, deadlines, server fixtures and expected-result
checks. Reproduce shell orchestration in that runner; shell scripts themselves
are not dotcc inputs. Keep upstream assertions and request/callback logic intact,
and record any required source adaptation against the immutable input hash.

Start with `prog_mkdir.c`, `prog_rmdir.c`, `prog_cat.c`, `prog_cat_cancel.c` and
the shell cases that use them, plus required `utils/smb2-cp.c` coverage. The first
shell equivalents are `test_0200_mkdir.sh`, `test_0210_cp_basic.sh`,
`test_0300_cat_basic.sh`, and `test_0310_cancel_pdu.sh`. Retain the authored C#
facade tests: the upstream programs do not exercise managed ownership contracts.
`prog_ls.c` interposes allocation through `dlsym(RTLD_NEXT)` even in ordinary
runs; it needs an explicit runtime mapping before translation can preserve its
semantics. Keep each test entry point isolated so process/global state cannot leak
between cases. Reuse the existing isolated Samba oracle and compare native,
raw/processed JIT and NativeAOT results, including file contents and exit status.

Do not equate Valgrind or LD_PRELOAD with managed runtime checks. Report the
original instrumentation and any managed equivalent separately. The upstream
open-timeout test still requires its special server. Optional full DCE/RPC tests
remain outside the current library profile. Each shell case needs a receipt with
input hashes, original checks, adaptation, execution variants and skip reason.
The [upstream-only fault-injection rule](test-scope.md) continues to apply.

## Kerberos provider and authentication boundary

The pinned `lib/krb5-wrapper.c` already supplies a boundary for credentials,
GSS/SPNEGO token exchange, context release and session-key retrieval. In particular,
`krb5_session_get_session_key` queries `GSS_C_INQ_SSPI_SESSION_KEY`; generating
an initial service ticket alone is insufficient for SMB signing/encryption.
The managed profile now exposes compatible translation-time types but replaces
the native-provider implementation at this narrow wrapper seam with authored C#.

The bridge pins [Kerberos.NET](https://github.com/dotnet/Kerberos.NET) 4.6.168
(MIT). Its client produces service tickets/GSS tokens, and its
application [session context](https://github.com/dotnet/Kerberos.NET/blob/develop/Kerberos.NET/Client/ApplicationSessionContext.cs)
exposes key and response-processing facilities.
The generated raw and postprocessed libraries compile with this bridge. This is
implementation evidence, not proof of SMB interoperability or NativeAOT.
Retain translated SMB negotiation, session setup, signing, sealing and file I/O;
implement the smallest supported GSS/authentication adapter around the provider.
If the existing seam needs adjustment, prefer semantic function overrides, never an edit to generated C#.

Acceptance must establish token continuation and mutual-authentication behavior,
the correct negotiated context key (including subkey handling), `cifs/host` service
names, DNS/realm discovery, expiry/renewal behavior, and no NTLM fallback when
Kerberos is explicitly required. Retain generated named constants in the facade.

Expose the same credential choices on both platforms:

- Explicit principal/realm/password, with keytab support evaluated separately.
- Existing credentials without prompting for the password: current-user Windows
  sign-in and Linux ticket caches. Document exactly which cache types work;
  support for a FILE cache does not establish KCM, KEYRING or Windows logon support.

Keytab and explicit-password authentication use the same managed Kerberos.NET
context on Windows and Linux. Windows current-logon reuse alone uses SSPI; non-Windows
existing credentials currently accept only an explicit `KRB5CCNAME=FILE:...`.
The latter path is not qualified yet; KCM, KEYRING, and DIR caches remain unsupported.
Managed password/keytab/FILE-cache modes request non-mutual Kerberos GSS, so
Kerberos.NET does not generate an AP-REQ subkey that would require an AP-REP.
BER SPNEGO responses must explicitly identify a Kerberos mechanism and
`accept-completed`; rejected/incomplete results fail. An optional AP-REP is
validated and may supply a new key; otherwise the service-ticket session key is
used. The final SMB session-setup response must be signed, and translated libsmb2
verifies that signature before accepting the session, including encrypted sessions.

The authored SSPI adapter uses **Negotiate**, which accepts SMB's SPNEGO tokens,
and requests mutual authentication without delegation. On completion it requires
`SECPKG_ATTR_NEGOTIATION_INFO` to identify Kerberos before querying
`SECPKG_ATTR_SESSION_KEY`. NTLM is not accepted. See Microsoft's
[Negotiate context queries](https://learn.microsoft.com/en-us/windows/win32/secauthn/querycontextattributes--negotiate).
Evaluate that boundary and its dependency implications explicitly. The current
plan's native-free product preference must not hide a platform dependency or
silently drop the requested current-user mode. A translated C Kerberos library
is a fallback if the managed provider fails required gates; it would require its
own source/dependency/ABI campaign and would not by itself solve OS cache access.

Use a disposable KDC/domain and SMB server for development. Require real
Windows and Linux executions with explicit and existing credentials, signed SMB2
and signed/encrypted SMB3 file operations under JIT and NativeAOT. Record actual
Kerberos use and compare with a native Kerberos-enabled client. AD/Windows-server
interoperability remains a required separate receipt if unavailable locally.

## Domain DFS path resolution

The described `\\work_domain\a\path` layout is consistent with a domain-based
[DFS namespace](https://learn.microsoft.com/en-us/windows-server/storage/dfs-namespaces/dfs-overview):
different namespace folders can refer clients to different backing shares.
This is an inference from the path description; the work environment has not
been inspected. For example, `\\work.example\a\finance\x` could resolve to
`\\files01.work.example\finance$\x`, while `a\engineering\y` resolves elsewhere.

The pinned libsmb2 source defines `SMB2_FSCTL_DFS_GET_REFERRALS`, its EX form,
DFS flags and `SMB2_STATUS_PATH_NOT_COVERED`, but no automatic referral-following
implementation. `SmbDfsClient` supplies that managed layer without changing or
retranslating the C sources. It sends the standard referral request through the
already translated generic IOCTL path, parses versions 1 through 4, performs
component-safe prefix replacement, caches by TTL, follows referral servers with a
bounded cycle check, and authenticates separately to each concrete target.

Domain controllers are discovered using the active Windows logon server and
Kerberos.NET DNS SRV support. Referral target order is preserved for retries; a
`STATUS_PATH_NOT_COVERED` operation requests a deeper referral using the original
namespace path. Each target receives its own CIFS authentication. Callers must trust the namespace
and its administrators to choose targets; no target allowlist is implemented.

Parser tests cover exact request encoding, version 4 multi-target responses,
Unicode path consumption, and malformed responses. Acceptance still requires at
least two namespace paths resolving to different servers, direct-share parity,
nested referrals, TTL refresh, target failover, referral loops, and the complete
explicit/current-user, signing/encryption, raw/processed JIT/NativeAOT matrix.

## Async merge behavior

Ticket acquisition is awaited before registering the translated context; the C
callback seam only consumes prepared state and handles server tokens. Password
and keytab `Authenticate` calls have no cancellation overload in the pinned
provider, so cancellation waits for that provider call to settle. Windows SSPI
calls are synchronous platform authentication calls; they are not a socket poll
loop and are not wrapped in `Task.Run`.

DFS uses asynchronous connection, DNS SRV discovery, IOCTL and disposal. Cached
entries represent storage referrals only. Connection establishment may try another
target; an ambiguous operation failure is never replayed on another server.
`STATUS_PATH_NOT_COVERED` can request a deeper referral, bounded by a cycle check
and hop limit. Referral-only intermediate/name-list targets currently follow the
first advertised controller; comprehensive target-set/failback selection remains
unqualified. Rename resolves both endpoints and rejects cross-share moves, but
does not retry a deeper referral returned by the rename itself.

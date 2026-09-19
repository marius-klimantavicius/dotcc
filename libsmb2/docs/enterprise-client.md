# Translated upstream tests, Kerberos, and DFS

Status: requirements and implementation approach recorded; no new runtime
capability is claimed. The current product is an NTLMSSP client validated on
Linux x64 against Samba. The user requires explicit Kerberos credentials and
existing tickets/current-user sign-in on both Windows and Linux.

Current authorization: implement translated upstream tests. Kerberos and DFS are
on hold while the user decides; the sections below preserve proposals and
requirements, not authorization to continue those workstreams.

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
The current managed profile disables this code with its feature configuration.

Evaluate a pinned [Kerberos.NET](https://github.com/dotnet/Kerberos.NET) NuGet
release first. Its documented client produces service tickets/SPNEGO, and its
application [session context](https://github.com/dotnet/Kerberos.NET/blob/develop/Kerberos.NET/Client/ApplicationSessionContext.cs)
exposes key and response-processing facilities.
These are feasibility evidence, not proof of SMB interoperability or NativeAOT.
Pin version, license, transitive dependencies and source provenance before use.
Retain translated SMB negotiation, session setup, signing, sealing and file I/O;
implement the smallest supported GSS/authentication adapter around the provider.
If the existing seam needs adjustment, apply a documented hash-checked input
adaptation, never an edit to generated C#.

Acceptance must establish token continuation and mutual-authentication behavior,
the correct negotiated context key (including subkey handling), `cifs/host` service
names, DNS/realm discovery, expiry/renewal behavior, and no NTLM fallback when
Kerberos is explicitly required. Retain generated named constants in the facade.

Expose the same credential choices on both platforms:

- Explicit principal/realm/password, with keytab support evaluated separately.
- Existing credentials without prompting for the password: current-user Windows
  sign-in and Linux ticket caches. Document exactly which cache types work;
  support for a FILE cache does not establish KCM, KEYRING or Windows logon support.

Windows logon reuse may need a narrow platform authentication/context adapter;
do not assume protected logon credentials can be exported into a managed cache.
Windows SSPI documents [session-key context queries](https://learn.microsoft.com/en-us/windows/win32/secauthn/querycontextattributes--general).
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
DFS flags and `SMB2_STATUS_PATH_NOT_COVERED`. Searching its `lib/` and public
headers finds no automatic referral-following implementation. The current facade
accepts a concrete server/share and does not resolve domain DFS paths. DNS lookup
of the domain alone cannot select a target based on the remaining path.

Add domain/DC discovery, root and link referrals, target selection, path-prefix
replacement with suffix preservation, referral TTL caching and bounded referral
chaining. Preserve the original namespace path separately from the resolved
server/share/path. Authenticate each selected host with its appropriate CIFS SPN;
a ticket for the namespace host is not a ticket for every referred file server.

The missing referral protocol implementation needs an explicit source decision:
prefer suitable upstream C code or a small documented C extension translated by
dotcc. A managed orchestration/cache layer may use translated protocol operations;
do not quietly replace the SMB client with OS-mounted shares or a C# SMB stack.
Qualify a real namespace with two links pointing to different servers, nested
paths, Unicode suffixes, cache expiry and ordinary target selection. Require the
same public behavior on Windows and Linux; any extra injected-failure tests still
need an explicit pinned upstream counterpart.

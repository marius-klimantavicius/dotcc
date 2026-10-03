# Owner environment locale selection

The selected source facade resolves empty `setlocale` and `newlocale` names
against the translated owner's live `environ`. It reads nonempty `LC_ALL`, then
the requested category variable, then `LANG`; if all are absent or empty it
selects `C.UTF-8`. It never reads the host environment after the ordinary owner
environment snapshot is created. Source `setenv`, `unsetenv`, `putenv` and
`clearenv` therefore affect subsequent locale selection without mutating other
owners or the host.

The implementation is an authored one-TU component at
`backend/runtime/libc/locale_environment`. Its manifest hashes the adapter and
retains unchanged musl 1.2.6 `locale_map.c`, `setlocale.c` and `newlocale.c` as
reference files, not selected translation units. The original locale-map code
provides an independent native oracle for the supported precedence cases.
The [POSIX environment rules](https://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap08.html)
describe the LC_ALL/category/LANG hierarchy; the
[setlocale contract](https://pubs.opengroup.org/onlinepubs/009695399/functions/setlocale.html)
requires checking relevant environment values before changing LC_ALL state.

All requested names are resolved before entering the managed category bridge.
The bridge validates category bits and owned handles under the existing locale
lock, then commits a single mask update. Failed all-category selection leaves
the previous mask and existing pinned name strings unchanged. `newlocale`
preserves unrequested base categories, rejects foreign/global/busy handles, and
does not mutate a base object when a requested environment name is unsupported.
Existing thread overrides remain separate from global `setlocale` changes.
Concurrent source environment mutation retains the environment API's existing
caller-synchronization requirement; this is not an atomic environment snapshot.

## Deliberate bounded profile

Recognized names remain `C`, `POSIX`, `C.UTF-8` and `C.utf8`. An unsupported
selected name makes `setlocale` return null with EINVAL, or `newlocale` return
null with ENOENT. Invalid categories and masks use EINVAL. There is no fallback
to a host `CultureInfo` or silently approximated locale.

This is not a claim that every musl locale behavior is reproduced. Musl may
create fallback maps for unknown names, whereas this provider rejects them.
Musl's built-in UTF-8 locale principally changes LC_CTYPE; this provider retains
its established per-category UTF-8 name mask, including composite queries.
A null newlocale base retains C for unrequested categories, as in the existing
provider, rather than copying musl's environment-dependent base construction.
The source wrapper changes empty-name selection only; explicit-name, query,
localeconv, owner/TLS handle lifetime and supported `_l` behavior stay with the
existing service.

`LibcLocaleEnvironment.cs` is a partial of the existing owner service and adds
no global state. Repeated selection uses source string scanning and existing
pinned name storage without allocating temporary managed strings or arrays.
The four private imports validate exact lowered types, including the opaque
locale pointer identity; no name-only function binding is introduced.

## Verification

The retained standalone matrix at
`artifacts/campaign/libc-locale-environment-runtime-20261003-063000/results.json`
passes JIT and NativeAOT transaction, owner/TLS, handle-lifetime and steady-state
allocation checks. The original-musl oracle at
`libc-locale-environment-native-20261003-063100` passes 125 precedence/default
LC_CTYPE combinations under UBSan. The first allocation probe is preserved:
a first measured loop allocated 24 bytes; repeated warmed loops allocated zero.
The final test warms the same measured helper path before asserting zero across
100,000 updates. No runtime semantic change was made to hide that observation.

The generated/native matrix passes ten checks at
`libc-locale-environment-translated-20261003-063500/results.json`: original musl,
native host, then C/C++ static/instance JIT/NativeAOT. Each generated mode checks
750 category/precedence combinations, transactional failure, newlocale base and
TLS isolation, owner versus host environment, and zero-allocation repeated
selection. Eight malformed imports are rejected before C# emission at
`libc-locale-environment-abi-20261003-063400/results.json`. Including the standalone
runtime, the component has 20 focused checks. Reusable verifiers are
`libc_locale_environment_verify.py`, `libc_locale_environment_abi_verify.py` and
`libc_locale_environment_runtime_verify.py`.
The first generated run at `libc-locale-environment-translated-20261003-063400`
retains a host-fixture nullable annotation error; the translated library itself
compiled. The corrected run is recorded separately. Execution evidence is
Linux x64; the implementation uses translated owner state and BCL synchronization,
but Windows/macOS execution remains a separate coverage requirement.

The accepted component is registered as one selected source unit, bringing the
provider to185. Eight composed C/C++ static/instance JIT/NativeAOT packaging
checks pass in `artifacts/campaign/musl-provider-locale-conversions-001`; these
are shared with the binary80 conversion milestone and counted once.

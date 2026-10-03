# C17 host-command profile

The verified `system` facade runs commands through a BCL-backed command
processor. This is an explicit C17 implementation profile, with implementation-
defined processor/status behavior. It is not a complete POSIX process layer.
Selected-provider registration is coordinated separately from these focused proofs.

The authored `libc/system/system.c` passes the translated owner's current
`environ` vector to the strict `__edg_system(const char *, char **)` import.
`LibcCommand` snapshots that vector into the child ProcessStartInfo environment,
clearing the managed host's inherited entries first. Source environment changes
therefore affect that owner’s future commands without mutating the host or other
owners. The caller must synchronize concurrent source environment mutation, as
with the existing environment implementation. Duplicate names use the last
entry under the runtime platform’s environment-name comparison rules; malformed entries without a nonempty name and equals sign are rejected.
Command/environment bytes must be valid UTF-8; malformed bytes report EILSEQ.

The command service uses the **same captured working directory as LibcFiles**,
even if the host changes its current directory between constructing the FILE
service and calling system. Generated entry leases retain the owner throughout
a synchronous wait. No global ambient command state is introduced. Disposal
marks the service unavailable; individual Process objects are disposed after
waiting. The child environment, managed strings and BCL Process objects are
ordinary managed allocations, without transient pinned arrays.

Linux/macOS use `/bin/sh -c` with the command as one ArgumentList entry. Windows
uses the system-directory cmd.exe with `/d /s /c`, wrapping the command in an
outer pair of quotes so cmd's documented /s rule preserves the inner command.
The /d option disables registry AutoRun commands. Windows/macOS branches are
runtime-selected; current execution evidence is Linux x64 only.
[cmd syntax and quoting](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/cmd).

A null command actually starts the selected processor with a no-op exit command;
it returns nonzero only if that probe succeeds. Non-null calls return the raw
BCL Process.ExitCode. A normal child exit137 remains137; that number alone does
not prove termination by SIGKILL. Launch/wait errors return -1 with a normalized
portable errno; a failed null probe returns0. Unsupported runtime platforms
report ENOSYS. Host programming errors are not collapsed by a blanket catch.

There is no fake POSIX wait-status encoding or signal policy. .NET10 ExitCode
does not supply the distinction needed for raw wait status. Although .NET11
introduces a richer ProcessExitStatus API, this backend currently targets10.
[Official .NET process API evolution](https://devblogs.microsoft.com/dotnet/process-api-improvements-in-dotnet-11/).

## Standard streams and remaining process work

The child inherits the **host OS standard handles**. Source `freopen` currently
replaces an owner’s abstract FILE stream and does not change host descriptors.
Thus source FILE redirection, its buffers, or a host-injected MemoryStream do
not redirect a command’s OS stdout/stdin/stderr. This is tested explicitly:
child output reaches captured host stdout while the owner’s reopened stdout
file and injected stream stay empty. It must not be described as independent
owner I/O or as complete process integration.

Required later work includes source descriptor inheritance/redirection,
portable child status/signal representation for POSIX APIs, POSIX system signal
masking/disposition rules, and broader process execution/owner-termination
contracts. Environment names/values that cannot be represented by the BCL
child environment remain diagnosed rather than silently omitted.

## Metadata and focused evidence

Generated manifests carrying LibcCommand must record:

```json
{"hostCommand":{"version":1,"status":"bcl-exit-code","environment":"owner-source-environ","workingDirectory":"owner-service-snapshot","standardIO":"host-os-handles","posixSignalPolicy":false}}
```

The authored facade has sources:{} and an adapter hash, with sourceBase `libc`;
it has no invented musl provenance. The initial bridge imports only
`__edg_system`, keeping its character-pointer and pointer-vector ABI checked.
Ordinary system routine-address calls go through the translated C wrapper.

`artifacts/campaign/libc-system-runtime-20261003-053900/results.json` records
strict standalone JIT/NativeAOT execution, real NULL query, status7/19/137,
owner environment, FILE-anchored working directory, UTF-8 and quoting, empty or
malformed environment, missing processor/directory, disposal and the explicit
host-standard-handle behavior. The earlier independent-cwd prototype remains
at `libc-system-runtime-20261003-053600`. Neither proves Windows/macOS execution.
`artifacts/campaign/libc-system-translated-20261003-054400/results.json` adds
nine checks: a native command-processor reference with explicit normal-status
normalization, followed by C/C++ static/instance JIT/NativeAOT execution. These
exercise the public function address, owner environment, shared FILE directory,
UTF-8 errors, host-handle stdout behavior and the exact manifest profile.
`artifacts/campaign/libc-system-abi-20261003-054400/results.json` records six
rejected private ABI mismatches (return width, command pointee, environment
pointer depth/element, arity and variadic signature). Together with standalone
runtime evidence this slice has 17 focused checks. Windows/macOS execution,
owner FILE-to-child descriptor propagation and POSIX signal/wait semantics
remain unverified or unimplemented as described above.

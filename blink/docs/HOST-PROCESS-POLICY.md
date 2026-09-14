# Private process policy

The managed profile has one immutable process identity per owner. It exposes no
host process creation or replacement primitive. With HostIdentity bound, fork,
execv, execve and execvp return ENOSYS; setuid/seteuid/setgid/setegid/setpgid/setsid
return EPERM, including requests for the existing value. With no owner these
callbacks return ENODEV. Arguments receive normal C evaluation, but exec pointers
are not dereferenced and no operating-system process API is called.

waitpid accepts only WNOHANG, WUNTRACED and WCONTINUED option bits and returns
ECHILD without changing status. Unknown option bits return EINVAL. This is the
truthful result for the private namespace, which has no children; it does not
implement general host waiting. Pipe creation, guest exec loading and asynchronous
signals remain separate work.

Pinned demangle.c retains SpawnCxxFilt pipe/fork/execv/waitpid paths even with
DISABLE_THREADS. Empty private CXXFILT disables that route, and a missing private
executable stops lookup; retained calls still need explicit link bindings.
SysSetresuid/SysSetresgid fallback paths call the individual identity setters
when HAVE_SETRESUID/HAVE_SETRESGID are absent. Their denial preserves the bound
immutable identity. No native Blink process is used as a managed fallback.

`python3 blink/tests/HostProcessPolicy/run.py` snapshots sources and compilers,
checks a native fork/child-exit37/wait baseline, then independently checks the
specified refusal behavior in raw/optimized JIT and NativeAOT. The native and
managed expectations deliberately differ. The intended fixture includes direct
and function-pointer calls, C argument evaluation, unchanged failed outputs,
binding checks, two workers and compacting GC. Final passing receipt:
`artifacts/host-process-policy/attempt-t2f68dzt/receipt.json`.

An initial fixture helper named Path shadowed System.IO.Path inside generated
Libc and caused CS0119. Renaming this campaign-owned helper to SelectExecutable
allowed qualification; this did not repair the underlying generic name collision.
The failed attempt-8bgratgh is retained and tracked in BLOCKERS.md.

# Private process identity

The opt-in `HostIdentity/host-identity.h` boundary redirects getpid/getppid and
real/effective uid/gid calls into an explicitly bound immutable identity. The
default private process is PID 1 with parent PID 0. UID/GID are zero, matching
the private filesystem metadata owner. These values never query or change host
processes or credentials. They do not grant host operations. A controller may
choose another positive PID and a nonnegative different parent PID for tests.

The binding is managed thread-local state; no object reference enters C storage.
An unbound signed-ID call returns -1 with ENODEV; an unbound unsigned-ID call
returns UINT32_MAX with ENODEV. Successful queries preserve errno. Credential
mutation, process groups, child processes and guest threads are not implemented
by these read-only identity callbacks.

Actual upstream NewSystem stores getpid into System.pid, and its first machine
uses that value as tid. Guest uid/gid syscalls also call the corresponding host
functions. Full-core startup must bind this identity before creating the system;
that integration remains pending. The selected no-thread/no-fork profile keeps
one guest process identity per worker.

`tests/HostIdentity/run.py` compares native C identity/type invariants with
raw/optimized JIT/NativeAOT. The consumer additionally verifies the exact private
defaults, unbound errors, errno preservation and concurrent workers with distinct
explicit PIDs through compacting GC. All modes pass. Native host numeric IDs are
not compared to virtual values or copied into them. The complete Host source
project and all compiler/profile/generated inputs are snapshotted for the run.

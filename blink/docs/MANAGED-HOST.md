# Managed host declaration profile

`config/managed-host/` is an explicit campaign header overlay for compiling
unchanged Blink code toward an owning managed interpreter. It currently provides
**declarations and native-checked storage**, not a working host implementation.
`blink/linux.h` continues to own guest Linux wire records and constants. The
overlay does not define `__linux__`, `__GNUC__`, or another fabricated platform
identity to enable unavailable native code.

The first reference model is 64-bit pointers with LP64 C integers and the
observed Linux x64/glibc host record shapes. Those fixed storage shapes can be
emitted on another managed host, but this work has not measured emitted C#
layouts or executed on Windows. No record may be passed directly to a Windows
native ABI. A later deliberate storage change needs a new matching staged-native
oracle; the untouched native oracle remains a separate comparison.

## Header selection and failure evidence

The initial core attempt was affected by dotcc's include map registering headers
beside input C files by basename. Passing `upstream/blink/*.c` directly could
make Blink's own `signal.h` or `string.h` override the standard header of the
same name. Core probe inputs are therefore copied, with hashes, into isolated
staging directories before translation. The probe snapshots the host overlay
per attempt. Include-directory priority currently differs from GCC: later dotcc
include-map registrations replace earlier ones, so the campaign overlay must be
registered last. Do not infer a POSIX type defect from an incorrectly selected
header.

With correct staged inputs, a real subsequent failure was `pthread_t thread`
inside `struct Machine`. That identity field remains even with
`DISABLE_THREADS`; glibc supplies `pthread_t` transitively through its signal
header. The campaign now declares a native-checked 64-bit identity slot without
providing pthread operations. Including the campaign `pthread.h` explicitly
fails, preventing accidental promotion to a threaded profile.

The overlay's `config.h` records its baseline exclusions. The core probe owns a
separate `core-config.h` and deliberately excludes the overlay's `config.h` from
its snapshot, so feature selection has one owner. Both keep explicit guest
address translation, disable JIT/threads/x87 and unqualified optional CPU paths,
and do not declare unproven `HAVE_*` host capabilities. Sockets remain selected.

## Reviewed storage surface

| Header | Storage supplied | Native LP64 size/alignment examples |
| --- | --- | --- |
| `signal.h` | Signal set, used signal-information fields, action union/mask/flags/restorer, signal stack, retained thread identity | `sigset_t` 128/8, `siginfo_t` 128/8, `sigaction` 152/8, `stack_t` 24/8, thread identity 8/8 |
| `setjmp.h` | Jump storage and saved signal mask, with separate ordinary/signal jump declarations | Both jump buffer forms 200/8 |
| `sys/uio.h` | Scatter/gather pointer and length | `iovec` 16/8 |
| `poll.h` | Descriptor, requested events, returned events, descriptor count | `pollfd` 8/4, count 8/8 |
| `termios.h` | Flags, control characters, line discipline, input/output speeds | `termios` 60/4; 32 control characters |
| `sys/socket.h` | Generic/address storage, message and ancillary header, linger and credentials | Address storage 128/8, `msghdr` 56/8, `cmsghdr` 16/8, linger 8/4, credentials 12/4 |

Fixed-width signed/unsigned integer types describe storage. Pointer-bearing
records contain C pointers only, never movable CLR references. `siginfo_t`
contains the fields actually referenced by Blink plus reserved storage; its
complete POSIX union is not exposed or claimed. Socket ancillary traversal
remains an unresolved helper; simple header alignment/length macros preserve
the native layout arithmetic and do not implement a socket operation.

`constants.json` pins 84 signal, 126 terminal, and 42 socket constants measured
from the native Linux x64 headers. The signal numbers and flags are values for
this campaign's virtual host contract, not permission to use the managed host
OS's signal APIs. Unsupported operations still require explicit errors from an
eventual implementation.

## Operations intentionally remain unresolved

Signal, signal-mask, jump, poll, vector I/O, terminal, socket, and ancillary-walk
declarations map to `blink_host_*` symbols. No implementation or success stub is
provided. The operation inventory lists each redirected name and source header.
In particular:

- `sigsetjmp` is not an alias for ordinary `setjmp`. The 200-byte storage match
  proves neither synchronous unwind nor mask restoration. Any eventual managed
  token must use an integer handle; native saved-register bytes have no CLR
  control-flow meaning.
- `sigaction`, `sigprocmask`, and `kill` cannot accidentally bind to shared libc
  signal stubs or process-wide termination. Virtual delivery, mask changes,
  termination results, and cleanup remain separate implementation work.
- `poll` cannot bind to an always-ready placeholder. Readiness must reflect
  actual instance descriptors and support deadline/stop behavior.
- Socket names cannot bind to a global descriptor table without an instance
  host policy. No socket or host endpoint is opened by these declaration tests.

This overlay is incomplete. Other retained shared headers still require a
semantic audit, including filesystem, allocation, clocks, resource identity,
and process lifecycle functions. An emitted or linked program cannot pass a
runtime gate while required operations are unresolved or reach existing
placeholders. The current core probe has not passed that gate.

## Reproducing the evidence

```sh
python3 blink/tests/HostAbi/run.py
python3 blink/tests/HostAbi/inventory.py
```

The first command passed **87 native size/alignment/offset checks**, **252
constant checks**, and a compiled declaration-object audit requiring exactly
seven unresolved campaign host symbols. It compares native system headers
against the independent prefixed records in `abi.h`. A separate declaration
probe uses dotcc's foundational headers plus the overlay and verifies key record
sizes and unresolved call names. It does not execute the unimplemented calls.
Receipts and native probe output are under `artifacts/host-abi/`.

The inventory generator starts from the actual native core's recorded source
closure and recursively scans quoted upstream headers. It records 65 public
header names and precise users of relevant types. This part is explicitly
lexical: missing headers include conditionally excluded Darwin, Windows, and
other platform branches, so the list is not a claim that all names are active
managed blockers. Header existence also does not establish runtime semantics.
The remaining public POSIX candidates include memory mapping, ioctl, directory
statistics, scheduling, group identities, and platform resource interfaces;
they must be added based on the staged core's next concrete diagnostics.

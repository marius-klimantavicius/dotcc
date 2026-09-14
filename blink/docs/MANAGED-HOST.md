# Managed host declaration profile

`config/managed-host/` is an explicit campaign header overlay for compiling
unchanged Blink code toward an owning managed interpreter. It currently provides
**declarations and native-checked storage**, plus an explicitly qualified virtual
host-delivery-mask nonlocal-jump adapter. The overall host implementation remains incomplete.
`blink/linux.h` continues to own guest Linux wire records and constants. The
overlay does not define `__linux__`, `__GNUC__`, or another fabricated platform
identity to enable unavailable native code.

The first reference model is 64-bit pointers with LP64 C integers and the
observed Linux x64/glibc host record shapes. Emitted C# storage is now measured
under raw/optimized JIT and NativeAOT on Linux x64. Windows execution remains
unrun. No record may be passed directly to a Windows
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
| `setjmp.h` | Opaque ordinary jump prefix and separately owned saved virtual mask | Ordinary 200/8; signal-aware record 336/8 |
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

Signal, signal-mask, poll, vector I/O, terminal, socket, and ancillary-walk
declarations map to unresolved `blink_host_*` symbols. The signal-aware jump
adapter in `src/HostSignals/` separately implements virtual mask capture and
restore around generic numeric-slot nonlocal unwind. In particular:

- `sigsetjmp` calls `PrepareVirtualSignalJump` once before ordinary setjmp.
  Its 336-byte record owns a 200-byte jump prefix, a saved-mask flag and a
  separate 128-byte virtual mask. Only the prefix's first identity word has
  managed control-flow meaning. Native staged tests use real native jump
  storage plus the same separately owned mask; they do not reuse glibc padding.
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
python3 blink/tests/HostAbi/run-managed.py
```

The first command passed **99 native size/alignment/offset checks**, **308
constant checks**, and a compiled declaration-object audit requiring exactly
nine unresolved external symbols (including generic setjmp). It compares native system headers
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

## Executed emitted-storage qualification

`run-managed.py` snapshots the authored `abi.h` and the same `probe.c` used by
the native layout comparison. `BLINK_HOST_STORAGE_ONLY` selects 198 observations
against the authored records, then the script compiles and executes that exact
snapshot natively and through dotcc. Each type reports its emitted size, declared
alignment, actual position following a byte inside a containing struct, and
array-element stride. Each field reports both `offsetof` and actual pointer
subtraction from an instance address. The generated C# retains real field-address
subtractions; these checks do not rely solely on compiler-reported offsets.

| Linux x64 execution | Result |
| --- | --- |
| Matching native authored profile | 198 observations; also matches 99 native system/staged-record comparisons |
| Raw emitted C#, JIT | All 198 outputs match native |
| Raw emitted C#, NativeAOT | All 198 outputs match native |
| Semantically postprocessed C#, JIT | All 198 outputs match native |
| Semantically postprocessed C#, NativeAOT | All 198 outputs match native |

The observed run is recorded at
`artifacts/host-abi/managed/attempt-54lu91nd/receipt.json`; subsequent runs use
unique attempt directories and update `artifacts/host-abi/managed-latest.json`
only on success. Receipts include exact compiler/input/generated hashes, build
commands, case counts, and output hashes. Raw source is copied before semantic
postprocessing, and neither generated variant is hand-edited.
For this probe the semantic postprocessor produced identical C#; both separately
built execution variants were still run and compared.

This closes the emitted layout comparison for these authored host records on
Linux x64. It does not qualify the complete `Machine`/`System` layout, the remaining host
callbacks, guest execution, or Windows behavior. Signal-aware unwind has its own
semantic oracle described in `NONLOCAL-JUMPS.md`.

## Subsequent source-required headers

Actual unchanged `syscall.c` next required `struct flock`, `struct itimerval`,
and `struct rlimit`. The campaign now supplies measured `fcntl.h`, `sys/time.h`,
and `sys/resource.h` records and constants. Their file-control, timer, clock,
resource-limit, usage, and priority calls redirect to `blink_host_*` declarations;
these additions provide no operation implementation. This also prevents the
campaign from accidentally binding file-control or resource-usage calls to the
shared headers' existing success placeholders.

The flock record is 32 bytes/alignment 8, with five native-checked member
offsets. `timeval` is 16/8, `timezone` 8/4, and `itimerval` 32/8. The foundational
`timeval` tag remains shared with dotcc's `unistd.h`; its include guards also
allow native `time.h` coexistence. The resource header explicitly includes
`time.h` for the `clock_t` declaration needed by upstream `xlat.h`.
`rlim_t` is unsigned LP64, `rlimit` is 16/8, and `rusage` is 144/8.
`RLIM_INFINITY` retains its unsigned `rlim_t` type and is measured separately
from the numeric manifest. Native checks add 32 file-control, three timer, and
21 resource/priority constants, bringing the numeric manifest to 308 entries.

The incremental emitted evidence has its own immutable inputs and receipts:

| Added surface | Linux raw/optimized JIT/NativeAOT results | Receipt under `artifacts/host-abi/managed/` |
| --- | --- | --- |
| Flock added to initial host records | 188 outputs match native in all four variants | `attempt-w5oi7xot/receipt.json` |
| Timer header | 14 outputs match native in all four variants | `attempt-c6tm4zbw/receipt.json` |
| Resource header, unsigned infinity, clock type | 28 outputs match native in all four variants | `attempt-hk8_uxy8/receipt.json` |

The timer receipt precedes the additional native `timeval` coexistence guard;
that final header is included in the resource receipt, and the native timer
comparison was repeated successfully. Reproduce the additional emitted probes
with `python3 blink/tests/HostAbi/run-managed.py --timers` and `--resources`.
`run.py` also compares their actual campaign headers against native system
headers. These are layout and constant checks; resource-limit enforcement,
clocks, timers, and file operations still need separately qualified host behavior.

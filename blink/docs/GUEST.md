# Static HTTP guest fixture

The initial guest is the campaign-authored, single-threaded C program
[`service.c`](../tests/ServiceFixture/service.c), linked with musl 1.2.5 into a
little-endian Linux x86-64 ELF64 `ET_EXEC`. It has no `PT_INTERP`, dynamic loader,
fork, thread creation, or guest executable download. Native execution and native
Blink are test oracles; this fixture does not implement any emulator behavior.

## Pinned inputs and build

[`inputs.json`](../tests/ServiceFixture/inputs.json) pins the source, fixture,
musl release archive, and qualified executable. The musl archive SHA-256 is
`a9a118bbe84d8764da0ea0d28b3ab3fae8477fc7e4085d90102b8596fc7c75e4`.
Its source archive is retained in `ref/`; its `COPYRIGHT` contains the MIT license,
the x86-64 port notice, and the component license inventory. This project does
not change the license of musl or the surrounding repository.

The qualified executable SHA-256 is
`916eeadfde4092c55bd2d05d5cacbe20482f4cabda8c0436fcfb68d690f2a9b8`.
[`qualified-build.json`](../tests/ServiceFixture/qualified-build.json) records
the observed compiler, assembler, linker, archiver, make, compiler backend and
libc archive hashes. The compiler is Ubuntu GCC 13.3.0
`13.3.0-6ubuntu2~24.04.1`, with GNU binutils 2.42. These are identified installed
host tools; their distribution packages have not been archived by this fixture.
Reproduction on a different compiler is unqualified until its generated binary
is reviewed and the native cases rerun. The build refuses an executable that
differs from the pin and preserves the differing build receipt for inspection.

From any working directory, run the scripts using their repository paths:

```sh
blink/scripts/build-guest.sh
blink/scripts/test-native-service.py
blink/scripts/test-native-service.py --blink blink/build/native/blink
```

The build verifies the archive before extracting staged source, starts from
fresh musl build directories, and supports offline reruns once the archive is
cached. It bounds fetch, configure, compile, and install duration. Guest flags:

```text
-std=c11 -D_POSIX_C_SOURCE=200809L -O2 -static -fno-pie -no-pie
-Wl,--build-id=none -Wall -Wextra -Werror
```

The resulting executable is `build/guest/service`. Build identity, ELF headers,
musl configure/build/install output, and test receipts go in `artifacts/guest/`.
The service takes `PORT FILE`; port zero selects an available loopback port for
the native oracle. It reads the complete instance file at startup (256 KiB
maximum) and prints `READY <actual-port>\n` only after successful bind/listen.

## Request and response contract

Each request uses HTTP/1.1 with CRLF line endings and a `Host: fixture` header.
The stop request also includes `Content-Length: 0`. Each response has exactly
these headers in this order, followed by the documented body:

```text
HTTP/1.1 <status>\r\n
Content-Type: text/plain\r\n
Content-Length: <decimal body byte count>\r\n
Connection: close\r\n
\r\n
```

| Case | Request line | Status | Body |
| --- | --- | --- | --- |
| Health | `GET /health HTTP/1.1` | `200 OK` | `ok\n` |
| Instance file | `GET /file HTTP/1.1` | `200 OK` | Exact bytes of `instance.txt` |
| Fragmented request | Same as instance file; client writes three bytes per send | `200 OK` | Exact bytes of `instance.txt` |
| Large response | `GET /large HTTP/1.1` | `200 OK` | 131072 bytes, byte `i` is `97 + i % 26` |
| Missing route | `GET /missing HTTP/1.1` | `404 Not Found` | `not found\n` |
| Stop | `POST /stop HTTP/1.1` | `200 OK` | `stopped\n` |

The client compares every response byte, including headers; no normalization is
applied. It saves requests/responses and their hashes. TCP may coalesce client
writes; the fragmentation case does not assert a particular receive-call count.
After stop, the service closes descriptors, prints `STOPPED\n`, and exits zero.
The fixture has a 4096-byte request-header bound and intentionally implements
only this small request contract, rather than general HTTP parsing.

The Python oracle driver enforces a whole-test deadline (60 seconds by default),
readiness and response-size limits, and a process-group kill on timeout or failure.
This is test-harness containment; guest-side cancellation, hostile clients,
two-instance virtual files/endpoints, and managed worker lifecycle remain later
qualification gates. Native files and loopback sockets here are real host assets.

## Observed execution and syscall inventory

On Linux x64, native Linux under `strace` and the pinned native Blink interpreter
both passed all six exact-response cases and exited zero on 2026-09-14. Blink ran
with `-jms` (JIT disabled, memory safety, syscall logging) against the configured
native oracle whose linear-memory fast path is also disabled at build time.
Native results are in `artifacts/guest/native-linux/result.json`; interpreted
results are in `artifacts/guest/native-blink/result.json`. Raw traces are retained
as `syscalls.log` beside each result. This reports native P0 evidence, with no
claim that translated C# has executed the service.

The complete observed guest syscall-name set for these requests is:

```text
accept arch_prctl bind close exit_group getsockname ioctl listen open poll read
recvfrom sendto set_tid_address setsockopt shutdown socket writev
```

The native Linux trace additionally includes `execve` from the test harness's
initial executable launch; it is not a guest requirement for spawning a child.
`arch_prctl` and `set_tid_address` occur during musl startup even though the guest
creates no threads. `ioctl(TIOCGWINSZ)` probes stdout and fails `ENOTTY` as expected
when output is captured; that is an observed error path, not a required terminal.
No `mmap`, `brk`, or `mprotect` was observed for this fixture run; their absence
does not remove the plan's required memory tests. Inventory completeness applies
to the executed cases, not all possible input/error paths in the C source.

# Genuine .NET NativeAOT HTTP guest

This fixture is C# code compiled into a Linux x64 NativeAOT executable. It is
distinct from both the earlier C service guest and the NativeAOT emulator host.
It uses synchronous `TcpListener`/`Socket` calls; no authored C or P/Invoke
implementation supplies its HTTP behavior.

Run from the repository root:

```sh
python3 blink/scripts/build-dotnet-guest.py
```

The executable accepts a loopback port (`0` chooses an available native port),
prints `READY <port>`, and serves one connection at a time. `GET /health` returns
`ok\n`; `POST /stop` returns `stopped\n`, closes the listener, prints `STOPPED`,
and exits zero. Both replies have an exact HTTP/1.1 status line, text/plain
content type, content length and connection-close header. The native control
compares every response byte and requires empty stderr and normal shutdown.
Requests are bounded to 4096 header bytes. This is a small controlled fixture,
not a general-purpose HTTP server.

The project pins SDK **10.0.111** without SDK roll-forward and runtime
**10.0.11**, targeting `linux-x64`, self-contained NativeAOT. Invariant
globalization is explicit because its contract uses only ASCII HTTP and numeric
ports. Runtime threads and GC retain ordinary behavior. The runner snapshots
the project, SDK pin, inherited build/package props and NuGet configuration;
it records the actual publish command, SDK information, tool versions/hashes,
resolved packages, ELF headers, binary and native wire/trace identities. The
native linker driver is explicitly the available `gcc`; the qualified host has
GCC 13.3.0 and GNU binutils 2.42. Shared SDK/NuGet caches and system link inputs
are used: this is not a hermetic build or a cross-host bit-reproducibility claim.

The first ordinary publish/native control passed at
`artifacts/dotnet-guest/attempt-ly2kzxeo/receipt.json` (SHA-256
`ba15e6f325cbbf0ea0c0df910da844421b5d65d93c509f4c470668816ed84450`).
Its binary was 1,553,192 bytes, SHA-256
`71361cb5a8995671c21cd99a24d27b95fdffd5969e5630856f37aafaef7e4946`.
This first receipt preceded explicit copying of the inherited central-package
props; its resolved assets retain the effective central versions, and its exact
runner is preserved beside the receipt.

The final repeat, including the explicit central-package props snapshot,
passed both exact native requests and shutdown at
`artifacts/dotnet-guest/attempt-5130ydrk/receipt.json` (SHA-256
`0c5841150df27f83e0aa49d4225b84dde1cc041caff1f0de16ea14d1ce1bd2ac`).
Its binary is also 1,553,192 bytes, SHA-256
`87985a99c01e395468de19e0fae5f8c907dbcf50ad08cf4fb07613aeda124be4`;
the syscall trace SHA-256 is
`9a34ed7d637d37d5d2482a12909708cbdce410a5d38ff808225834055b369094`.
Separate publish directories produced different binary hashes; no bit-for-bit
reproduction claim is made. The ELF dependency/TLS shape remained identical.

The observed ELF uses `/lib64/ld-linux-x86-64.so.2`, with `DT_NEEDED` entries
`libm.so.6`, `libc.so.6` and `ld-linux-x86-64.so.2`. Its own `PT_TLS` has 24
initialized bytes, 288 memory bytes and 8-byte alignment. Symbol requirements
include GLIBC versions through 2.34. Native `strace -f` observed genuine
`.NET Finalizer`, `.NET Sockets` and `.NET SigHandler` threads created with
`clone3`/`CLONE_THREAD`/`CLONE_SETTLS`; synchronization and startup include futex,
epoll, signal masks/actions, TLS setup, pipe2, rseq and membarrier. The runtime
also inspects procfs, CPU/cache sysfs and cgroup limits. The full trace preserves
success/error outcomes; observation alone does not establish that every probe
must succeed in a guest environment.

Only native Linux execution is qualified here. No translated-emulator startup,
musl publication, guest thread implementation, or P5 completion is claimed.

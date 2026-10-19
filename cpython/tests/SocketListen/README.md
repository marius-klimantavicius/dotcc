# Focused socket listen contract

`header_contract.c` uses the actual selected `sys/socket.h`, twice, and proves
macro presence, integer type, positive value and retained socket ABI. The native
oracle supplies the value and CPython default-minimum expectation. The original
A23 reduction remains unchanged at its original artifact path.

`listen_contract.c` is the SAME-C fixture reserved for fresh-tool controls after
normal capture. Its native-only `SOCKET_LISTEN_NATIVE=1` helper binds the real
glibc TLS errno accessor when GCC uses the target headers. `native_syntax.h`
only forward-declares time-header tags. Native commands and failures are retained.

`Program.cs` calls the actual current shared Libc project. Its header-value
argument comes from the native oracle; it does not establish that an immutable
compiler embeds the repaired header. The driver inspects the kernel listener
queue using `ss`, sends one TCP message, checks exact echo bytes and observes
listener closure. Wrong bytes must reject. No stress or performance claim.

The driver reuses `campaigns.process` for bounded build execution and process
cleanup. Each shared/native command acquires its own nonblocking `flock --close`.
B27 ran `before`, `before-tool`, `after-r2`, `unit`, `bindings` and `release`.
The original RED is retained; `before` refuses to overwrite its initial snapshot.
Do not rerun it against the repaired header. The initial `after` phase retains
the native fixture's subsequently corrected errno macro warning.

The sibling `SocketListenUnit` project compiles unchanged socket/readiness/reuse
test files against Libc only. Fresh immutable-tool direct/object raw/processed
JIT/AOT execution of these C fixtures remains a coordinated next-owner step.

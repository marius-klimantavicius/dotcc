# Managed embedding profile

`scripts/translate.sh` uses the managed embedding profile by default. Whole-function
host behavior is selected through typed `managedMethod` bindings in
`config/dotcc-overrides.json`. Implementations live in `src/Host/*.cs`, with
namespace `Managed.Database`; C declarations live in `src/Host/valkey_host.h`.
The generated project links these authored files instead of generating host C#
from C implementations. `--unadapted --probe` diagnoses original sources.

Prefer function overrides and existing portable behavior over source edits.
The static Lua resolver and process setup functions use overrides. Existing
portable platform selection chooses `ae_select`; libc already supplies the fixed
C locale. Unsupported signal registration returns an error. The managed host
owns descriptor limits and worker lifetime, so process-only setup overrides are
empty. None of these cases requires replacing C source text.

`config/managed-adaptations.json` retains narrowly scoped staging edits for
hooks inside command dispatch, completed shutdown, configuration dispatch and
BIO queue/worker lifetime, plus opaque module type identities. These hooks need
to preserve the surrounding upstream algorithms; whole-function overrides do
not currently retain a callable original body. Every changed file must match its
pinned SHA-256, and every replacement must be unique. Receipts record input and
output hashes. Reference sources remain untouched.

The C# lifecycle initializes upstream configuration, modules, listeners, Lua,
workers and persistence before reporting readiness. Calls run serially on the
owner's executor with explicit runtime binding. Event dispatch is nonblocking;
the executor supplies pacing and stop requests. Startup options are checked
before the upstream parser can perform side effects. Command checks run before
transaction queuing and at common execution dispatch, including module calls.
The configuration-dispatch hook also covers direct command execution during AOF
replay. Runtime configuration policy is enforced before setters execute.

Command and configuration admission does not depend on the qualification inventory.
`OBJECT ENCODING`, diagnostics, ordinary configuration and cluster commands reach
upstream validation. Only fork-dependent operations are rejected: background
RDB/AOF work, replication/failover (including cluster replication and fork-based
slot migration), asynchronous Lua debugging and daemonization. Runtime AOF
activation requires a background rewrite; startup-enabled AOF and foreground
RDB saves remain available. Multi-option CONFIG SET is checked before mutation.

One I/O thread and 512 clients are defaults, not enforced caps. Runtime I/O
workers use upstream code with libc deferred pthread cancellation and cleanup
handlers. Shutdown joins I/O workers before disposing their owner. Debug and
native-module commands remain disabled by upstream startup defaults; upstream
ACLs, immutable options, feature availability and actual libc failures still
apply. Native module binaries cannot use the translated owner-carrying callback
ABI: the typed `moduleLoad` boundary logs this incompatibility and returns
`C_ERR`/`ENOTSUP`, without calling native exports. Startup module directives are
processed through the upstream queue and fail explicitly when loading fails. Allowing dispatch does not establish that every feature is qualified.

Successful stop preserves upstream save/flush decisions. Cleanup drains and
joins workers, releases the event loop and closes wakeup descriptors. The managed
owner must reclaim remaining allocations and descriptors only after cleanup
returns success. Failed startup and worker-fault paths require managed runtime
qualification; the native harness cannot establish isolation or CLR unwinding.

The separate C reference host retains its historical, narrower allowlist. It is
not the current managed admission policy. The separate C reference host in `tests/native_reference/` is only a native
control, not the product implementation. Its recorded
`scripts/host-oracle.sh --no-fetch` run passed 32 checks against real adapted native
Valkey objects, including static Lua, configuration/transaction guards, RDB
reload, startup AOF replay, pending lazy-free work and worker shutdown. Its
receipt is `artifacts/native-host/receipt.json`. Default invocation fetches and
verifies inputs; `--no-fetch` is explicit. This native control does not establish
that the translated server works.

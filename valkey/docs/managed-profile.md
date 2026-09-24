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

The initial profile disables process signals/watchdogs, daemonization, process
title changes, background persistence, replication, cluster and dynamic modules.
It uses one I/O thread and caps clients at 512 for the 1024-descriptor select
backend. Foreground RDB saving and startup-enabled AOF remain upstream operations.
The authored option/command guards still need auditing against the complete
required/deferred inventory and managed execution tests.

Successful stop preserves upstream save/flush decisions. Cleanup drains and
joins workers, releases the event loop and closes wakeup descriptors. The managed
owner must reclaim remaining allocations and descriptors only after cleanup
returns success. Failed startup and worker-fault paths require managed runtime
qualification; the native harness cannot establish isolation or CLR unwinding.

The separate C reference host in `tests/native_reference/` is only a native
control, not the product implementation. Its recorded
`scripts/host-oracle.sh --no-fetch` run passed 32 checks against real adapted native
Valkey objects, including static Lua, configuration/transaction guards, RDB
reload, startup AOF replay, pending lazy-free work and worker shutdown. Its
receipt is `artifacts/native-host/receipt.json`. Default invocation fetches and
verifies inputs; `--no-fetch` is explicit. This native control does not establish
that the translated server works.

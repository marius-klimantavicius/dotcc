# Managed embedding profile

`scripts/translate.sh --managed-profile` applies the exact-input adaptations in
`config/managed-adaptations.json` to an isolated staging tree. Every changed
upstream file must match its pinned SHA-256 and every replacement must be unique.
The receipt records both input and output hashes. Reference sources are untouched.

The profile selects upstream `ae_select`, resolves static Lua entrypoints through
real translated C function addresses, and aligns opaque module string/reply tags
with their actual core identities (`serverObject` and `CallReply`). It preserves
the opaque module API; it does not expose or replace the objects' implementation.

The authored C lifecycle initializes upstream configuration, modules, listeners,
Lua, workers and persistence before reporting readiness. Calls run serially on
the managed owner's executor. Each call requires an explicit runtime binding.
Event dispatch is nonblocking; the managed executor supplies pacing and stop
requests. Startup options are checked before the upstream parser can perform
side effects. Deferred commands are rejected before transaction queuing, and
runtime configuration changes are checked again before their setters execute.

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

`scripts/host-oracle.sh --no-fetch` passes 32 checks against real adapted native
Valkey objects, including static Lua, configuration/transaction guards, RDB
reload, startup AOF replay, pending lazy-free work and worker shutdown. Its
receipt is `artifacts/native-host/receipt.json`. Default invocation fetches and
verifies inputs; `--no-fetch` is explicit. This native control does not establish
that the translated server works.

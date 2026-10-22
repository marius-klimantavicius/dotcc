# PGlite translation campaign

The P0 campaign baseline is established for `TranslatedPGlite`. Native embedded
SQL controls pass 21 assertions; the managed SQL product does not yet work. [PLAN.md](docs/PLAN.md) defines the first useful workflow and open
milestones; [VALIDATION.md](docs/VALIDATION.md) records current results and
[BLOCKERS.md](docs/BLOCKERS.md) retains reproducible failures.

The fork is pinned in `config/sources.json`. Use the shared campaign runner:

```bash
bash Scripts/campaign.sh layout pglite --check
bash pglite/scripts/fetch.sh
bash pglite/scripts/test.sh --suite native-control --fetch never
bash pglite/scripts/probe.sh --profile native-diagnostic --fetch never --tools reuse --jobs 1
```

`native-diagnostic` is a host configuration for feasibility evidence. It does
not prove PGlite's Emscripten embedding profile or produce the delivered engine.
Native database executables are controls only. Generated libraries, sources,
build work and receipts/logs remain ignored beneath this campaign directory.
The default `embedding-native` profile and consumer remain required gates.

The selected default profile is the real fork's `__PGLITE__` native LP64 variant,
pinned to `29b6424aa312ec3556247db069939006dd27f878`. Normal translation selects
865 units: the complete observed 863-unit native backend closure, original
PGlite libc unit, and forwarding adapter. A separate initdb/initialization closure
and required host services remain incomplete; publication is blocked. The C11
atomic port passes native/emitted JIT layout and concurrency controls; spinlock
selection is the next complete-input blocker. Native reopens use fresh processes.

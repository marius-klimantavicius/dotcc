# EDG C# implementation campaign

The authoritative scope is `docs/dot-gen-be-plan.md`. The user requests full
C++ and libc support in runnable translated C#, not merely a passing subset.
Keep completed milestones and remaining gaps explicit. The coordinator owns
task allocation, integration, build scheduling, acceptance evidence and commits.

## User constraints

- Assume EDG is correct. Consult `upstream/src/c_gen_be.c` and the EDG runtime
  for suspicious lowered IL and C++ behavior; adapt our backend rather than
  changing EDG semantics. Consult dotcc for .NET implementation techniques.
- Generated code and library glue must support NativeAOT and cross-platform
  execution. Prefer BCL services; choose unavoidable platform APIs at runtime.
- Preserve correctness when optimizing. Prefer pinned arrays for globals and
  TLS; use native allocation for short-lived storage and C/C++ heap allocation
  (`malloc`, `new`, etc.), with explicit ownership and cleanup.
- Instance globals and independent library state remain opt-in CLI features.
- Prefer musl over newlib if a manageable BCL-backed port is feasible. Audit
  dependencies; dotcc's libc remains an initial bridge/fallback. C++ standard
  library coverage is also part of the plan.
- The user subsequently authorized temporary cleanup because disk space was
  nearly exhausted. Root coordinates removal of disposable build outputs from
  completed, inactive tests. Preserve generated C/C#, IL, projects, provenance,
  verification logs and failure evidence, current compilers and active work.
  Do not perform concurrent worker cleanup. New large test outputs may use
  `artifacts/campaign/persistent-continuation`, a link to persistent storage.
- **Only synthetic inputs and EDG tests may be used for verification until the
  entire plan is complete.** Do not run SQLite, Valkey, compiler self-translation
  or other real-world project acceptance before that point. Existing historical
  evidence may be read but is not new acceptance evidence.
- Commit every significant verified milestone. Stage only owned EDG changes;
  preserve unrelated user work. EDG sources are embedded in the parent dotcc
  repository, with no nested `.git` repository.

## Coordination

The user explicitly authorizes a coordinator and implementation sub-agents.
Use disjoint file ownership. Serialize compiler preparation/builds and Git
mutations through the coordinator. Workers report changes and validation;
they must not overwrite another worker's edits or claim broader coverage than
their actual tests establish. Keep the campaign ledger current across handoffs.

Canonical backend sources live in `backend/`; `backend/prepare.py` copies them
into the embedded upstream build tree. Edit the canonical files. `build/` and
`artifacts/` remain ignored but must be retained during this campaign.

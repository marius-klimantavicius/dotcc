# Translate {{upstream_name}} with DotCC

<!-- Copy to <project>/docs/PLAN.md. Replace {{...}}, resolve decision rows,
and remove instructions/examples that do not apply. Links to the shared docs
work at both this location and the standard <project>/docs/PLAN.md location.
Do not pre-mark milestones complete based on another project's results. -->

Status: planning; no implementation or qualification claimed.
Campaign: `{{project}}/`. Updated: {{date}}.

This plan adopts [translation baseline v1](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md). It records this project's
decisions and gates; common execution and repair methodology stays in the
baseline. Current user instructions govern implementation, commits, branch work,
and delegation; this template grants none of those permissions.

## Outcome and scope

Translate {{actual upstream implementation/source component}} into
`{{TranslatedName}}`, preserving {{algorithms/state machines}}. The separate
managed consumer must {{concrete useful workflow and observable result}}.
Host services provide {{operations}}, through {{selected boundary}}.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required for completion | {{explicit first useful profile}} |
| Excluded from this product | {{components and reasons}} |
| Later extensions | {{separately qualified work}} |
| Departures from the baseline | {{decision IDs and reasons, or none}} |

## Decisions and feasibility work

Use `decided`, `provisional`, or `open`. Replace suggestions with concrete choices.
For an open decision, name the experiment and the dependent milestone; keep
independent work moving. Record evidence when accepting or changing a choice.

| ID | Decision | Choice and reason | Status; resolving evidence/gate |
| --- | --- | --- | --- |
| D1 | Source version and dependencies | {{release/commit; product vs test/peer inputs; metadata paths}} | {{status; P0}} |
| D2 | Source/configuration closure | {{manifest, generated headers, defines, dialect; default/alternate profiles}} | {{status; P0}} |
| D3 | ABI and text/binary formats | {{data model, widths/alignment, endianness, encodings; native oracle}} | {{status; P1}} |
| D4 | Host/provider and dependency policy | {{reused runtime/libraries, authored services, permitted imports and prohibited backends}} | {{status; P1}} |
| D5 | State, concurrency and lifecycle | {{static/per-instance; reentrancy; startup/readiness/stop/drain}} | {{status; P1/P3}} |
| D6 | Translation and delivery | {{direct/object emission; class/namespace; source splitting; host/facade projects}} | {{status; P2}} |
| D7 | Native and independent controls | {{native build/profile, assertion checks, fixtures and peers if relevant}} | {{status; P0/P4}} |
| D8 | Required execution matrix | {{platform/architecture × profile × form × runtime; additional axes}} | {{status; P4/P6}} |
| D9 | Test scope and optional campaigns | {{upstream coverage; ordinary errors; custom faults/fuzz/stress selected or excluded}} | {{status; P4}} |

Source and adaptation metadata: `config/{{source_metadata}}.json` and
{{other metadata paths}}. Keep revision/checksum literals out of scripts.
Default hash policy is `warn`; `off` is supported; strict reproduction is optional
unless explicitly selected here: {{none, or narrow reason/scope}}.
Exact text adaptations still require unambiguous local guards in every mode.

## Source, host and ownership contracts

{{Identify the selected upstream build files and source manifest. Explain omitted
units, generated prerequisites, per-unit options and direct/object linking needs.
Separate observed compiler failures from suspected language/ABI difficulties.}}

| Boundary | Translated responsibility | Host/reused dependency | Ownership, errors and validation |
| --- | --- | --- | --- |
| {{allocation/arena}} | {{upstream behavior}} | {{service}} | {{allocator pairing, stability, lifetime; probe}} |
| {{callbacks/I/O/provider}} | {{upstream behavior}} | {{service}} | {{calling convention, buffers, handles, failures; probe}} |
| {{lifecycle}} | {{init/work/cleanup}} | {{owner/executor}} | {{concurrency, cancellation, callback drain; probe}} |

Allowed adaptations: {{existing extension points, typed overrides, any necessary
staged patches and their exact-match guards; or none}}. State what stays unchanged
and how the adapted boundary is checked against native behavior. Identify any
upstream bug repair separately from a compiler repair.

Highest risks to resolve before broad implementation:

| Risk | Smallest useful experiment | Observable success/failure | Dependent gate |
| --- | --- | --- | --- |
| {{actual project risk}} | {{real source/host/ABI probe}} | {{assertions; fallback decision if unavailable}} | {{P1/P3/...}} |

## Framework integration and deliverables

- Recipe: `scripts/campaign.py`, exporting `Recipe("{{project}}", ...)`.
- Final library: `generated/{{TranslatedName}}/{{TranslatedName}}.csproj`.
- Raw library: `generated/{{TranslatedName}}.Raw/{{TranslatedName}}.csproj`.
- Nondefault profiles: `generated/profiles/{{profile}}/{{TranslatedName}}[.Raw]/`.
- Solution: `ManagedConsumer.slnx` at the project root.
- Sample: `samples/ManagedConsumer/ManagedConsumer.csproj`; its useful workflow
  is {{inputs/actions/asserted result}} and its translated-project property is
  `{{TranslatedProject}}`.
- Authored host/API: {{original src/ paths and project references}}.
- Recipe dependencies/hooks: {{translated dependencies and project-only hooks}}.
- Prerequisites: {{SDK/native tools/fixtures/peers; how acquired and when needed}}.

Use the shared helpers and Python resolution. Declare additional source groups
and specialist suites in the recipe; do not reproduce the framework's fetching,
process, postprocessing or publication code. Default translation must complete
without manual source copying/repair. Keep native/peer test prerequisites out of
ordinary translation unless a documented required gate needs them.

## Milestones and evidence

Specialize each gate below into observable project results. Split host/behavior
work into named milestones if necessary. Evidence belongs to the artifacts
actually exercised; old successful receipts do not close new work.

| Phase | Concrete exit gate for this project | Status | Commands/evidence and remaining work |
| --- | --- | --- | --- |
| P0 — Scope/native baseline | {{profile frozen; working native workload; first real translation findings; recipe/layout}} | Planned | {{paths}} |
| P1 — Boundary feasibility | {{actual emitted/native ABI and highest-risk host/ownership probes}} | Planned | {{paths}} |
| P2 — Complete translation | {{full manifest builds raw/processed; canonical output and no unresolved required imports}} | Planned | {{paths}} |
| P3 — Useful execution | {{real workload, required host behavior and clean lifecycle}} | Planned | {{paths}} |
| P4 — Feature qualification | {{required features, upstream cases and selected error/interop checks}} | Planned | {{paths}} |
| P5 — Consumer delivery | {{root solution; separate owning consumer; required JIT/AOT execution}} | Planned | {{paths}} |
| P6 — Reproduction/acceptance | {{final-tool regeneration, required matrix, dependency audit and affected regressions}} | Planned | {{paths}} |

Dependencies: {{normally P0 → P1 → P2 → P3 → P4 → P5 → P6; specify any parallel
design work and additional feature dependencies}}.

Blocker ledger: {{location}}. Track classification, reproducer, native expectation,
regression, fix and full-source retry. Preserve baseline failures separately.

## Validation selection

Replace every row with concrete recipe suite names and assertions; add/remove
optional rows according to D9. List `default_suites` and `verify_suites` explicitly.
A help command is a packaging smoke check, not the useful-workflow acceptance test.

| Behavior/gate | Suite and oracle/assertion | Required profile/form/runtime/target | Required or optional; prerequisite |
| --- | --- | --- | --- |
| ABI/callback/host contract | {{suite; actual native/emitted observations}} | {{matrix}} | {{selection}} |
| Useful consumer workflow | {{suite; outputs and cleanup}} | {{matrix}} | Required; {{inputs}} |
| Required upstream features | {{suite; case inventory and comparison rules}} | {{matrix}} | Required; {{fixtures}} |
| Ordinary errors/lifetimes | {{suite; actual statuses and ownership checks}} | {{matrix}} | {{selection}} |
| Independent interop/file exchange | {{suite; peer or format invariants}} | {{matrix}} | {{selection or not applicable}} |
| Custom stress/faults/fuzzing | {{explicit selection or excluded}} | {{matrix}} | {{reason and scope}} |
| Shared regressions | {{affected tests/campaigns and reason}} | {{matrix}} | {{triggered by which shared changes}} |

Default suites: {{names}}. Verification suites: {{names}}.
Comparison/normalization rules: {{exact values, logical state or defined masks;
original logs retained}}. Upstream test inventory and exclusions: {{path}}.
Performance scope: {{none, or workloads/metrics and any justified thresholds}}.

## Reproduction commands and acceptance

Fill these placeholders after recipe creation and retain the commands actually
used. Framework options and prerequisites are described in the shared guide.

```bash
bash Scripts/campaign.sh layout {{project}} --write
bash Scripts/campaign.sh layout {{project}} --check
bash {{project}}/scripts/fetch.sh --fetch missing
bash {{project}}/scripts/translate.sh --fetch never
dotnet build {{project}}/ManagedConsumer.slnx -c Release
bash {{project}}/scripts/test.sh --suite {{consumer_suite}} --form all --mode all --rid {{rid}}
bash {{project}}/scripts/verify.sh --fetch never --rid {{rid}}
```

Confirm fetching can be disabled with usable local inputs and provenance drift
does not fail `warn`/`off` runs. Document any specialist download/restore behavior.
Repeat with clean campaign outputs without deleting unrelated files, invoke from
an unrelated directory, and preserve authored files through regeneration.

Final evidence: {{validation report and framework/specialist receipts}}.
Unrun targets, failed/blocked required gates and excluded cases: {{explicit list}}.
Remaining optional work: {{list}}.

Complete only when the selected workflow, required features/matrix, consumer
delivery and reproduction gates pass with current evidence. Report any smaller
validated subset precisely while keeping incomplete required work open.

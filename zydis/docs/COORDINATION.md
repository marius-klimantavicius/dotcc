# Integration coordination (supervisor)

2026-10-27: Sources available, exact upstream pin read by supervisor. DotCC and
DotCC.PostProcess Release tools built successfully. All managed/tool builds remain
supervisor-owned. Initial independent Decoder.c object probe needs explicit
`__x86_64__` (without it Zycore reports Unsupported architecture); include the
architecture selection in policy. Probe is diagnostic, not product configuration.

Tests must remain out of the product. Prefer a separately emitted companion for
qualification C plus any upstream tool test entrypoints, with the same full
library selection. Parent can integrate companions once tests interface exists.
Generated low-level surface should remain fully rooted in qualification AOT.

API contract src/INTEGRATION.md and tests/CONTRACT.md reviewed. Final recipe suite
names should be native, abi, qualification, upstream, consumer, audit (adjust plan
if more accurate names chosen). No fixed workload entrypoints in library.

Full 18-unit diagnostic now found compiler ZY-001: inferred multidimensional
struct-array bounds, from Generated/DecoderTables.inc. Supervisor reduced and
removed inappropriate guard in IrBuilder.Aggregates.cs; 17 focused tests and
2 runtime aggregate-dimension fixtures pass. Starting canonical framework full
translation with freshly built tools. Recipe presently missing architecture
macro; expected first canonical failure until explicit target macro is added.

Supervisor integrated explicit `__x86_64__` into config/translation.json after
canonical run 20261027-065712-85121b5e reproduced unsupported architecture on
MetaInfo. Full retry using fresh fixed compiler now running. Do not remove this
required target selection; native control command uses identical profile macros.

Actual generated output available for API agent inspection:
`build/campaign/20261027-065728-f4b894b4/raw/TranslatedZydis`.
Full 18-unit emit/link succeeded; raw build log at matching artifacts/campaign
run directory. Supervisor reviewing next compiler/host/API errors. ZY-001 repair
committed as b2c10fae1 after 17 unit + 2 functional cases and full-source emit.

Companion canonical attempt `20261027-070205-c301d404` fails Terminal.c preprocessing:
unsupported platform. Supervisor added explicit `__linux` target macro (Zycore
checks this spelling, not __linux__) in shared profile. This selects POSIX for
test-only original terminal support and records actual target. Product core had
already emitted/built without OS services. Tests agent must preserve __linux.

# B36 — V21 actual RAW JIT full run and required-marker inventory

B36 received A30's direct V21 receipt and genuine safe release before any heavy
command. The unchanged RawDiscoveryR30 project uses the frozen production facade.
Its own entry/facade/library PE/PDB and physical Compile inputs bind separately
from the canonical publication. The actual library is SHA256 `20b60ea75819c0f135799ced26e21e935860fc3f842858673d81c58e21e2416c`,
MVID `2184752b-db8c-47b3-8eec-65e13b7f547d`; A30's canonical `d50031a5…` remains
a different PE. All four relevant runtime sections and 93 physical upstream test
sources match their frozen inputs. Synthetic doctest metadata is identified
separately. Resolved execution uses .NET 10.0.12 on Linux x64.

All 12 original I/O failures pass in separate focused execution and inside the
immediately following full unchanged suite. Eight actual Python resize/write/read
scenarios match the successful cases in B35's native C fixture, including sparse
offsets above 2 GiB. A30's correct same-C native/emitted JIT/AOT proof is reused.

The full RAW JIT run completes in **322.367 seconds**:

- 4,867 unique IDs started and completed; **4,763 unique PASS**.
- 101 required SKIP and 3 required XFAIL; zero failures, errors, fatal or unrun IDs.
- 4,875 outcome events include 4,771 PASS events and eight repeated pass IDs.

Required markers and their reasons are unchanged. The normal suite exits 1
because these markers remain required nonpasses. No unexpected assertion failure
or error remains for this RAW JIT input; complete qualification stays open.

[Actual inputs](../artifacts/owner-b-r36-20261018/actual-input-proof.json),
[runtime observations](../artifacts/owner-b-r36-20261018/runtime-execution-binding.json),
[unique/event counts](../artifacts/owner-b-r36-20261018/full-required-journal-counts.json)
and [original-case comparison](../artifacts/owner-b-r36-20261018/result-comparison.json)
retain the exact scope. Four pure count controls reject event inflation and
preserve interrupted, mixed-subtest and unexpected fixture-error observations.

The [case-level inventory](../artifacts/owner-b-r36-20261018/required-marker-source-inventory.json)
and [next-control report](../artifacts/owner-b-r36-20261018/marker-categories-and-next-controls.json)
classify every remaining row:

- **85 bytecode-writing skips:** explicit Host `write_bytecode=0`; qualification
  needs an explicitly coordinated writing-enabled startup before discovery and
  writable consumer-owned cache paths. Ambient environment variables cannot
  override the isolated explicit configuration.
- **3 UTF-8-mode skips:** explicit Host `utf8_mode=1`; exact native mode-0/mode-1
  controls and a coordinated qualification startup are needed. Preserve the
  production default and B34 API until its owners coordinate any change.
- **10 pipe skips:** `HAVE_PIPE` is not selected. Existing shared pipe services
  need a prerequisite control: source inspection finds pipe slots have null
  streams, while `fstat` rejects null streams with EBADF and FileIO rejects that
  result. The new [same-C reduction](../tests/Abi/pipe_metadata_r36.c) is **UNRUN**.
  No service defect has been newly executed or repaired by B36; do not flip the
  feature flag without the native/emitted/owner/nonblocking/metadata controls.
- **1 symlink skip:** `HAVE_SYMLINK` is not selected despite an existing shared
  declaration/service. Native/emitted symlink, broken-target, lstat/readlink and
  errno controls precede policy review and any normal regeneration.
- **2 unconditional upstream skips:** the zip resource decorator and condition
  recursion method have no environment activation switch. Their frozen required
  status creates an explicit completion-adjudication conflict; no waiver occurs.
- **3 upstream XFAILs:** two unchanged descriptor cases freshly reproduce the
  same XFAILs on the exact-pin native oracle. The namespace-zip case remains
  native-unavailable; matching expected failures remain required nonpasses.

The bounded fresh native availability control takes 3.169 seconds: two exact
cases execute as XFAIL; 102 cannot load. First missing modules are `_socket` for
15 IDs, `binascii` for 2, `_testmultiphase` for 72 and `_posixsubprocess` for 13.
The existing oracle's configure uses `--disable-test-modules`; its actual
`Setup.local` explicitly disables these modules and other transitive helpers.
A separately coordinated same-pin qualification-native profile must close the
real import/helper graph. Four names alone are not claimed sufficient. No new
native build or alternative Python was used.

[Direct genuine return to A30](../artifacts/owner-b-r36-20261018/serial-slot-release-to-a30.json)
records all five owned command PIDs absent, three independent free locks and no
active/queued work. B36 is source/reporting/docs-only. The prepared pipe control
and native-profile closure were sent to A30 for sequencing after its current
data-proof safe phase; neither is queued. [Preservation](../artifacts/owner-b-r36-20261018/preservation.json)
verifies B34/B35 sources, frozen cases, decorators and runner. No runtime, header,
Host/API, compiler, policy, production framework or canonical output was edited
by B36; no staging, commit, push, reset, cleanup or worktree was performed.

New193/current processed products, final D10/type/IL/RVA/allocation acceptance,
both-form JIT/AOT interpreter/ABI/owning/thread/lifecycle/rooted/trimmed/delivery/
audit/upstream, production regressions and reproduction/P6 remain OPEN. None of
the 104 required markers was reclassified; P0–P6 acceptance remains incomplete.

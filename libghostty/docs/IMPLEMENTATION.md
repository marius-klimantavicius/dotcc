# libghostty-vt implementation ledger

Updated: 2026-10-29. Acceptance remains incomplete; see [PLAN.md](PLAN.md).

## Repository baseline

`sqlite` at `d3b1ccd975fd883f4f6b75d4b70fa4c9f112fc81`, preserving all preexisting
tracked/untracked changes. Initial coordinated command:
`dotnet build DotCC/DotCC.csproj -c Release --no-restore -m:1` passes with zero
warnings/errors (`/tmp/libghostty-initial-compiler-build.log`).

Framework baseline: `python3 -B -m unittest discover -s Scripts/campaigns/tests -v`
runs 90 tests, with 89 passing and one preexisting failure:
`test_campaign_helpers_keep_hash_literals_in_metadata` finds a digest literal
in preexisting untracked `blink/scripts/full_jit_copy_bindings.py`. Original log:
`/tmp/libghostty-framework-baseline.log`. No Blink file was changed to hide it.

## Blocker ledger

| ID | Classification | Reproducer/expected vs observed | Regression/fix | Full-input retry |
| --- | --- | --- | --- | --- |
| Z1 | Compiler grammar | Pinned `src/lib_vt.zig`, line 187: valid trailing argument comma in multiline `@export`; parser rejects it. | Frontend agent implementing shared grammar support and native-valid reduction. | Pending; original log `artifacts/zig-frontend-probes/initial-lib-vt.log`. |
| C1 | Support-header compatibility | Original `zig.h:6177` expands symbol assembler labels; DotCC parser rejects `__asm`. | Exact typed forwarding macro policy, plus removal of labels only on unused Windows intrinsic declarations; see ADAPTATIONS.md. | Advances to C2. |
| C2 | Shared compiler grammar/layout | Genuine freestanding generated `engine.c:7853`, `typedef __attribute__((aligned(1))) uint64_t aligned__1_u64_9;` rejected. | Scalar typedef alignment metadata and prefix/suffix grammar; actual explicit storage/layout, compatibility and aligned locals/globals. Native/emitted direct and object-link fixtures match, including actual record size 10. No annotation erased. | Actual full C retry advances to C3. |
| C3 | Shared C lexer | Generated `engine.c:15909` contains valid `"\?"`; string lexer rejected it although decoding already supports it. | Accept escaped question marks; native/emitted literal bytes agree. | Actual full C retry advances to C4. |
| C4 | Shared declarators | Generated `engine.c:19664` needs pointer-to-const-function-pointer local storage; retry at 19716 also needs the corresponding abstract cast. | Compose pointer levels/qualifiers structurally for named callbacks and abstract callback types. Native and direct/object-link execution all produce `callbacks 41 42 7`; read-only diagnostics preserved. | Actual full C retry reaches C5. |
| C5 | Shared expressions | Generated `engine.c:25395` uses `(struct T){{...}}.array[index]`; direct compound-literal postfix access rejected. | Place C99 compound literals in the postfix expression tier. Native and direct/object-link reductions produce `postfix 42 7`; 3510 unit cases pass. | Current accessor engine advances to line 447923, C6. |
| C6 | Shared callback parameters | Accessor engine line 447923 declares a named const callback parameter `void (*const a1)(...)`. | Share structural named declarator construction across callbacks and array-pointer parameters. Native/direct/object-link reductions produce `parameters 42 41`; all 3516 unit tests and six affected functional cases pass. | The complete 530948-line accessor engine now parses. Semantic C7 is first failure. |
| C7 | Source ABI | Full source semantic check fails its line-28 assertion requiring `sizeof(long double)==16` and alignment 16; the existing DotCC default models long double as 8 bytes. | Investigating explicit source-ABI storage support with loud rejection of unsupported numerical operations. Assertions remain enabled; no engine values use this type at this pin. | Not passed. |

Shared C2/C3 validation: **3504/3504 DotCC unit tests**, 89 focused alignment/GNU
unit tests, and **6/6 focused functional tests** (three Zig execution cases,
direct/object-link actual scalar alignment/storage, escaped-question execution)
pass. Logs: `artifacts/zig-frontend-probes/all-unit-tests-after-alignment.log`,
`scalar-alignment-functional.log`; full-input retry logs:
`engine-after-alignment.log`, `engine-after-question-escape.log`.

C4 validation: **3510/3510 DotCC unit tests**, 19 focused callback cases and
two direct/object-link callback execution cases pass. Logs:
`all-unit-tests-after-callback.log`, `qualified-callback-functional.log`,
`qualified-callback-unit-tests.log`. Actual full source retries:
`engine-after-callback-storage.log`, `engine-after-callback-cast.log`.

Milestone commits so far: `89e9bb534` (P0 campaign/native controls),
`a19c47f2d` (generic Zig expression/import repairs and regressions).

## P0 validation

Shared recipe discovery/layout passes. Existing product, toolchain and supplemental
license archives/trees are reusable with `--fetch never`, both `--hashes warn`
and `off`; fetch invocation also passed from `/tmp`. Layout includes the canonical
root solution, authored owning API/sample and shared Bash dispatch wrappers.

`bash libghostty/scripts/test.sh --suite native --fetch never` passes with 16
assertion cases, original native engine and genuine whole-engine C exact parity:
`artifacts/campaign/20261029-154051-f5367cca/receipt.json`, specialist report
`native-oracle.json`. Native checks cover creation, ASCII/render, cursor/erase,
SGR, split Unicode, visibility, screens, resize/reflow, reset, callbacks/title,
formatter buffers, ordinary errors, allocation failure, instance independence
and 32 balanced allocator lifecycle cycles. Additional authored allocator/ABI
adapter controls pass natively against both engines; their integrated rerun
is required after adapter edits. ABI observations: pointer 8, formatter options
56, allocator 16 and vtable 32 bytes.

`translate.sh --fetch never --tools reuse --form raw --jobs 1` with the preserved
initial compiler closure reaches C2, with no product published:
`artifacts/campaign/20261029-154246-a24e2a2c/receipt.json`, `000-engine.log`.
This attempt uses the full actual upstream build/module command, generated
Unicode inputs, freestanding C target, and authored ABI adapter. A preceding
attempt failed because a manually copied diagnostic tool closure omitted
satellite resources; it was corrected by preserving the complete closure and
was not mistaken for an engine failure.

Direct Zig findings/repairs and native/emitted regression receipts are detailed
in [ZIG-FRONTEND.md](ZIG-FRONTEND.md). At this point 556 Zig unit tests and three
native/emitted execution reductions pass; direct root semantic/export support
remains incomplete. The complete product proceeds through the genuine C route.

## Acceptance evidence

Current typed-address ABI integration preserves all **93 upstream exports**:
`artifacts/abi-export-inventory.json` records identical native-C export inventories
before/after the staged accessor extension. The current full native suite passes
16 oracle plus two boundary cases:
`artifacts/campaign/20261029-160333-22351e10/receipt.json`. New independent physical
ABI probes measure six public aggregates using actual storage sizes and field
address differences; native/emitted JIT transcripts match exactly at
`artifacts/abi-layout-managed.stdout` versus `tests/native/abi-expected.txt`.
These public header probes do not substitute for actual emitted-engine execution.

The applicable upstream render control passes **58/58**, zero failures/skips and
zero live allocator blocks, with exact original-Zig/native-C transcripts:
`build/native-tools/upstream-context-r3/artifacts/receipt.json` and
`upstream-render.json`. The loaded render inventory contains 61 named tests;
three Kitty graphics tests are explicitly excluded because graphics was excluded
from the agreed product before testing. Exact accepted output is retained in
`tests/upstream/expected.txt`. The new diagnostic managed profile is unrun.

An auxiliary broad native Debug test attempt reports C-API superset
3403 pass / 155 skip / 2 fail of 3560, and Zig module 2928 pass / 105 skip /
2 fail of 3035. The failures are an out-of-product TinyIo OS path case and a
full-reset test expecting glyph registration despite the disabled glyph feature.
This auxiliary run is not a passing campaign gate. Remaining applicable core
test groups are being classified; no full upstream qualification is claimed.

No translated terminal engine product or raw/processed JIT/NativeAOT runtime
gate is accepted yet. Pins and downloaded references are available locally;
native control and direct Zig translation work are running independently.

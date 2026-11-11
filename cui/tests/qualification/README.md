# CUI qualification runner

`../../scripts/qualify.py` runs actual upstream C programs emitted by DotCC,
native controls, and independent C workloads linked to the translated core.
It does not load native CUI or native GLFW in managed executions. All temporary
inputs, native/managed builds, transcripts and `result.json` use `--work`.
Choose a fresh external work directory for each invocation. The script accepts
absolute paths and runs from an unrelated current working directory.

Install `requirements.txt` in an isolated Python environment on external storage.
Supply the trusted Noto fixture described in `font.json`, a built matching native
CUI GLFW control, .NET 10, and the actual campaign compiler DLL. `cc`, CMake/CTest
and native GLFW are test-only prerequisites. The current native path layout and
native probe compile command target Linux; actual Windows qualification is open.

```bash
python "$REPO/cui/scripts/qualify.py" \
  --suite upstream --source "$CUI_SOURCE_ROOT" \
  --native-build "$CUI_NATIVE_BUILD" --work "$CUI_QUALIFICATION_WORK/upstream" \
  --compiler "$DOTCC_COMPILER" --font "$CUI_ARABIC_FONT" --jobs 2 \
  --form raw --mode jit --rid linux-x64
```

The coordinator can invoke this command with `ctx.run`. `--suite` selects:

| Suite | Executed checks |
| --- | --- |
| `native` | All nine selected deterministic native CTest commands, with output paths relocated to private scratch; unchanged upstream shaper vs independent HarfBuzz, 661 cases |
| `upstream` | Original `test_main`, `test_devtools`, `test_instrument`, `test_inflate`, `test_todo`, `test_bidi`, `rtl_layout`; `rtl_paragraph` with trusted font and five exact normalized RGBA comparisons; unchanged `shape_dump` vs HarfBuzz |
| `render` | PNG/JPEG/BMP/GIF/TGA decode and ownership/error checks; software light/dark RGBA buffers compared byte-for-byte to native, plus RGBA/BGRA channel checks |
| `lifecycle` | First context allocation refusal, null cleanup, eight offscreen create/frame/destroy cycles, allocator balance after every cycle |

`--action inventory|native|managed|all` supports specialist runs without a suite;
default `all` executes native controls and every managed program. `--program NAME`
may repeat to select a narrower run; the receipt records the actual programs.
`--form raw|processed|all` and `--mode jit|aot|all` select execution variants;
defaults are raw/JIT. Supply `--postprocessor DLL` for processed forms. AOT
requires the selected RID's toolchain and execution host; cross-publication alone
does not pass a test. Full form/runtime/platform acceptance remains coordinator
work until those variants are actually run.

By default core tests call the existing recipe's `prepare` and emit the full
58-unit selection in private storage using its typed host overrides. To reuse
the coordinator's exact objects instead, add both:

```text
--objects-dir RETAINED_OBJECT_DIRECTORY --staged-source ITS_MATCHING_SOURCE_TREE
```

The runner checks object fragment headers and records object hashes. The
coordinator must supply corresponding source, profile and compiler provenance;
the runner does not infer that arbitrary objects belong to the pinned source.
Minimal bidi/inflate/shaping programs independently emit their unchanged source
closure. Every qualification link uses the same CUI profile's inline and static
deduplication flags as the delivered libraries, including these minimal closures.
No generated C# is patched. An authored managed entry point calls the
emitted typed `Managed.Forms.Cui.main` API with stable UTF-8 arguments. Only
project scaffolding is adjusted; processed host inputs are private copies so
postprocessing cannot rewrite authored campaign sources.

Core programs share one linked executable to avoid repeating the large core link.
Each test's sole top-level `main` definition is renamed in a staged test copy,
with a unique local anchor; all test bodies and assertions stay intact. The
dispatcher selects one test per fresh process, so runtime/global state never
leaks between suites. `--separate-programs` retains the slower diagnostic path.

`test_main` includes upstream randomized docking checks. C's seeded `rand`
sequence depends on its implementation, and C permits different operand
evaluation orders for random calls in a mutation assignment. The ordinary native
GLFW baseline retains its original 6,147 assertions. For strict differential
comparison, a test-only typed input hook records values from the actual emitted
`Managed.Forms.Cui.Libc.srand/rand` provider, tagged with seed, original source
line and call site. Native C replays those exact call-site queues and must consume
every value. All 69 named groups must have identical cumulative assertion counts; the four
managed variants must produce identical recorded input hashes. The control
changes input delivery and adds group-count diagnostics; it preserves assertions
and mutation/drag algorithms. No shared random generator is changed.
The binary vectors (four little-endian 32-bit fields per record), complete logs,
seeds, provider/source hashes and check counts are retained under `--work`.

Exit zero means every selected native/build/execution/comparison passed; nonzero
means a failure. Read `WORK/result.json` for selection, command transcripts,
case/check counts, fixture identity, byte comparisons and failures. Full logs
are `WORK/logs/`. The runner continues to independent programs after a program
failure. A failed combined test build falls back to separate program builds so
one test-only lowering failure cannot hide the other programs' results.
A core-emission failure stops all programs depending on that closure.

Upstream inventories 18 registered CTest entries: nine deterministic selections,
eight fuzz programs and one timed soak. Fuzz/soak are explicitly excluded by
PLAN D9. Upstream's built-in inflate mutations and core incremental-render tests
remain unchanged and execute as part of their selected suites. Upstream core,
devtools, instrument and todo print assertion counts. Bidi/RTL aborting CHECKs,
inflate vectors, C++ ABI smoke and screenshots have their own output/exit
contracts; no fabricated runtime assertion count is assigned to them.

The native C++ public-header test and gallery screenshot generator are inventoried
as native-only here. The translated runner uses C upstream tests and its exact
software-pixel workload; it does not claim the full gallery was compared. Desktop
GL, GLFW callback lifetimes and failure/reinitialization belong to the separate
consumer/host suites. Unicode's external full bidi-conformance dataset, benchmarks,
manual RTL screenshots and documentation programs are inventoried separately,
not represented as passing executions.

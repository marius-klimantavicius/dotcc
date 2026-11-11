# CUI 1.4.0 upstream refresh

The selected upstream revision is
`5a9c12b433d4501bc2f5632ead3a1cc05e5b1ee5`, release **1.4.0**, eight commits
after the previous `629b6fcb991d71527f2d67b25c51429c3dcdb498` pin. See the
[upstream comparison](https://github.com/yappologistic/CUI/compare/629b6fcb991d71527f2d67b25c51429c3dcdb498...5a9c12b433d4501bc2f5632ead3a1cc05e5b1ee5)
and [source metadata](../config/source.json). The archive SHA-256 is
`cf30dad8bac9c34c5a199614e1bd9aa083f00fc70e273230f32bacccb23cb310`.

## Changes and integration

The refresh includes keyboard selection fixes for split-button menus and path
bar overflow menus, stable keyboard focus through Copy/Copied transitions,
Windows SDK macro-compatible local variable names, a Linux desktop application
ID, release/version metadata and documentation. Native SDL2/X11 backend changes
remain outside the selected managed GLFW profile.

All 58 core units and the two original gallery units are regenerated with current
tools, private staging, the existing inline/static deduplication policy and
transactional raw/processed publication. Namespace `Managed.Forms`, safe
namespace `Cui.Managed`, project names, JSON managed-symbol metadata, original
gallery separation and the copied managed GLFW dependency boundary are retained.
Generated implementations are not edited by hand.

This refresh also adopts opt-in `--source-root` compiler identities. The default
compiler behavior remains compatible with legacy absolute-path identities.
CUI source/object emission uses one root with the upstream directory layout and
campaign headers under `dotcc/include`; qualification stages private copies with
the same logical paths. Paths preserve case and subdirectories and use `/`
separators. Hashes retain full SHA256 and ordered NUL framing. Outside-root files
and duplicate logical units fail; roots are never inferred from Git or basenames.
Object-only links do not accept a rewrite option. Existing objects remain
readable, and all affected CUI objects and JSON manifests are freshly regenerated.
See [the CLI/API contract](../../docs/cli.md#stable-source-identities).

The [relocation proof](../tests/source-identity/README.md) re-emits the full core
and gallery at another absolute root. All 60 object fragments, 21 generated
C# files and the exported managed-symbol JSON are byte-identical: 82 complete
files, with no redaction, whitespace normalization or identifier masking.
Lexical Windows path tests do not establish Windows runtime execution.

The app descriptor gains `app_id`; its x64 size changes from 48 to 56 bytes.
`ApplicationOptions.AppId` and the optional `DesktopApplication` constructor
argument expose it safely. Null/empty preserves defaults; embedded NUL is
rejected. The host pins encoded UTF-8 during creation only. The new typed
`glfwWindowHintString` override decodes it to an owned managed string before
GLFW retains its hint. All 41 upstream GLFW calls have managed overrides, and
the existing header/port already supplies the necessary hint constants.

The [retention audit](MANAGED-STORAGE-AUDIT.md) reviews every changed native
definition and refreshes evidence for 216 definitions plus the public header.
It preserves all 225 stack-local substitutions, stable retained pointees and
owned state leases. No existing widget lifetime change or translated-core
repair is needed for this update.

Upstream added a Copy-button test group. The existing random-input differential
harness incorrectly required exactly 69 groups, even when all assertions and
group counts matched. It now inventories the selected upstream entrypoint and
requires every expected group in order, with exact cumulative counts, identical
four-variant input traces and complete native consumption. A regression checks
that an added group becomes mandatory. The initial rejected receipt is retained;
the corrected gate is rerun separately without changing upstream assertions.
It is then included again in the complete final stable-identity matrix.

Qualification stages tests beneath the same logical root while keeping retained
sources and objects read-only. Each bundled test's renamed entry is restored
immediately after emission: `rtl_paragraph.c` includes `rtl_layout.c` under a
`main` macro and must see its original entrypoint. This prevents the first
emission's rename leaking into a later include. The staging regression suite
checks that case, failure cleanup, root consistency and retained-input integrity.

The original 27-page gallery C sources are unchanged. Existing emoji picker
scope and editable glyph adaptations remain guarded and are applied to native
oracles as well. Data-grid row-position selection through sorting is preserved.
The previously investigated trailing-newline caret issue and static Markdown
hint are also unchanged upstream; this release does not repair them.

## Qualification and reproduction

The [refresh receipt](UPSTREAM-1.4-VALIDATION.json) binds this source pin, current
generated/authored file hashes, current tools and execution results. Earlier
core, emoji, managed-library, gallery and namespace receipts remain historical
under their original source/product bindings. Actual Windows execution remains
unavailable and **unrun**; Linux results and compilation do not qualify it.

| Current Linux gate | Result |
| --- | --- |
| Shared compiler | Implementation's 147 focused tests and coordinator's additional 77-test selection pass; CLI/API compatibility and Windows/POSIX lexical contracts covered |
| Two-root CUI reproduction | 82 complete object/generated C#/metadata files byte-identical, with identical full unit/anonymous identifiers |
| Native upstream control | Nine deterministic CTest entries, 661 shaping cases and new application-ID smoke pass |
| Core/render/lifecycle | All 12 programs × four variants pass; 70 core groups exact, 6,162 ordinary native checks and 6,175 input-aligned checks, zero failures |
| Safe library | All four variants pass 293-type/12,170-assertion public audits, 75 safety assertions and three process benchmark trials |
| Full authored gallery | 1,728 wrapper captures, 1,728 translated comparisons and 432 independent native palette captures; all full-window pixels, ordered text and node layouts exact |
| OS interaction | Original and safe gallery each pass 4,042 assertions and 95 actions per variant; emoji/edit/grid/drawer state and pixels match |
| Application ID | Actual X11 class/instance values, UTF-8 copy, GC pressure, invalid input, reset and lifecycle pass; 25 safe-owner and 142 host/GL assertions per variant |
| Deployment | Four copied-output standalone runs pass 27 pages/108 frames, capture, dispose/reopen, dependency and file-access audits |
| Ordinary project build | Root solution passes with five existing translated-code CS1718 warnings and zero errors; no authored-code warnings |

The native pixel gate still permits the established one-byte GL conversion
rounding bound; the observed maximum in this matrix is **zero**. No masks,
sidebar exclusions or omitted pages are used. The comparison initially caught
the safe gallery's stale `v1.3` badge: it now caches a label derived from
`ManagedCuiInfo.UpstreamVersion`, matching native `v1.4` without per-frame
formatting. The complete gallery and copied-output matrix were rerun after that
correction; the earlier mismatch receipt is retained on external storage.

All allocation-conscious library workloads add zero steady-state managed bytes.
The largest observed ratio of summed construction/layout-paint medians to direct
CUI is below 1.267 for the library and 1.082 for the full gallery pages, against
the existing 2.0 gate. Maximum p95 construction overhead is below 0.108 and
0.089 µs/node respectively, against 0.5 µs/node. These are phase statistics,
not full-frame quantiles or FPS. Exact software pixels are checked before
timing. Wrapper/core retained storage stabilizes, and observed core allocations
return to zero at disposal. Whole gallery pages still allocate: Buttons 952,
Grid 792 and Developer tools about 50,524/50,920 B/frame (JIT/AOT). Receipts retain
every trial's p50/p95/p99 and allocation/resource observations, rather than
claiming whole-app zero allocation or hardware-independent speed.

Choose external storage for work, temporary directories, sources and tools:

```sh
export CUI_WORK_ROOT="$CUI_REFRESH_WORK/campaign"
export CUI_SOURCE_ROOT="$CUI_REFRESH_SOURCE"
export TMPDIR="$CUI_REFRESH_WORK/tmp"
mkdir -p "$TMPDIR"
bash cui/scripts/translate.sh --fetch never --jobs 4 --form all
python3 -B cui/scripts/managed-api/generate.py --check --native-root "$CUI_SOURCE_ROOT"
LIBGL_ALWAYS_SOFTWARE=1 xvfb-run -a -s '-screen 0 1440x1000x24 +extension GLX' \
  bash cui/scripts/test.sh --suite consumer --suite abi --suite audit --form all --mode all
```

`abi` includes the new unsafe-disabled AppId consumer, actual X11 `WM_CLASS`,
UTF-8 hint copying, GC pressure, thread/handle guards, close/reopen, GL execution,
desktop input and desktop services. See [the AppId test](../tests/app-id/README.md).
The new upstream native `cui_app_smoke` also verifies both `WM_CLASS` values.

Run native/core/render/lifecycle qualification using the pinned source, matching
native build, current compiler/postprocessor, trusted Arabic font and external
Python dependencies, as described in [qualification](../tests/qualification/README.md).
The corresponding objects and staged source from the translation can be reused
only together, preserving their receipt/tool identity. Run
[managed library](../tests/managed/README.md),
[full gallery](../tests/managed-gallery/README.md) and standalone product matrices
against the published projects. Use a fresh native gallery oracle from this pin,
not prior-revision captures. The full gallery uses the qualification-only frozen
wall-clock tool and all two-theme/eight-accent palettes.

Benchmark phases exclude GL, vsync and software rasterization, compare equivalent
work and preserve the existing allocation/retention/timing gates. Historical
timings are not silently rebound to the new implementation. Full raw logs,
captures, binaries and data remain on external storage; durable receipts use
portable location tokens and hashes.

Create the portable receipt after all gates pass with
`scripts/record-upstream.py --work-root "$CUI_REFRESH_WORK" --upstream-work
"$CUI_UPSTREAM_INPUT_WORK" --translation-receipt TRANSLATION.json
--integration-receipt INTEGRATION.json --baseline-commit BASE --source-commit REV
--compiler-commit COMPILER_REV --compiler-implementation-log FOCUSED.log
--output RECEIPT.json`. The input work directory
contains the fetched pin and matching independent native controls; the refresh
work directory contains the stable-root reproduction and final managed matrices.

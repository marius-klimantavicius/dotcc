# Owner B29 source-ready handoff — 2026-10-18

Both native-confirmed B28 blockers have narrow source fixes. This is a terminal
source-ready handoff; no canonical product or complete campaign is qualified.

- `DotCC.Libc/PosixFsLib.cs` SHA256
  `49409ec5a096fe98acb75614b5b6758aa7ebbc9951fc4074f2dec8e8673ecbed`:
  contain named cwd failures, honor GNU allocation and EINVAL/ERANGE rules, and
  avoid narrowing the caller's size_t when writing the actual UTF-8 path.
- `src/Host/dotcc_cpython_host.c` SHA256
  `84eb24c397b94f620e596b147c89720be824ca2e6348f41840d2e8ee17cf6328`:
  copy the selected stdlib root through real PyConfig_SetString before startup,
  retaining the independent module-search-path copy and existing cleanup/status.
- PathOwnership, RuntimeContext, B23 opendir/signal, B24 resolver/ownership,
  compiler/header/policy, frozen IDs/decorators/exclusions and main A documents
  are preserved. No native getcwd bridge or new cwd ownership framework is added.

[Source-ready evidence](../artifacts/owner-b-r29-20261018/source-ready.json) and
[focused results](../artifacts/owner-b-r29-20261018/focused-results.json) bind the
before/current source bytes and exact test scope. The initial group finishes in
156.902 seconds, including its retained harness failure, within the 900-second
cap. All builds use per-command nonblocking locks and bounded .NET settings.

The same original C fixture passes 47 native process checks and, per current-Libc
JIT/NativeAOT mode, 47 process plus 47 virtual-owner checks and 18 additional
owner/GC/thread/disposal assertions. Old source fails with seven escaped managed
exceptions and 13 process/14 virtual assertions per mode. Current source escapes
zero exceptions. Existing virtual cwd is path-based: external rename/deletion
returns ENOENT until absolute chdir recovery; process cwd follows rename. Returned
allocated buffers use the actual shared allocator and owner disposal.

The actual authored Host linked against the same-pin **base native oracle**
passes two selected stdlib roots, two complete lifecycles and two callbacks per
root. Missing encodings returns PyStatus kind1, exit23; partial-init clean0 is
retained. This does not claim the full 75-helper native profile.

Separate consumers of unchanged raw/processed V16 test and historical V11
embedded PEs pass all14 target-header/actual typed PyConfig layout values,
independent wide-copy mutation/GC/clear, non-ASCII relocation, ordinary invalid
options/error statuses and two initialization/finalization cycles. Four unchanged
frozen-finder IDs pass twice per V16 form with the full delivered stdlib selected
(16 executions, zero failures/errors/skips). These direct typed API probes do
**not** call or repair the old dcp_initialize in those frozen PEs.

The initial relocated partial stdlib sourced `test.support` from a second root,
so its expected STDLIB_DIR disagreed with the correctly configured selected root.
The failed command/log remain intact. Corrected relocation controls and the four
original cases use separate, consistent inputs. No upstream test was edited.
Fresh preexecution PE/PDB/reference/source bindings are retained; 25 additional
strict negative checks are explicitly post-execution evidence. Current-Libc
NativeAOT proofs bind the actual ILC entry, 175 references, object and executable.

[Commands](../artifacts/owner-b-r29-20261018/commands.json),
[strict input proof](../artifacts/owner-b-r29-20261018/strict-input-validation.json),
[preservation](../artifacts/owner-b-r29-20261018/final-preservation.json), and
[RELEASE-ALL](../artifacts/owner-b-r29-20261018/serial-slot-release-all.json)
provide the concrete review inputs. No compiler/tool/postprocessor capture,
emission, profile generation, publication, full required-suite run, staging,
commit, push, cleanup, worktree or nested worker occurs.

Next: normal immutable capture including these source fixes, unchanged-C
native/direct/object/raw/processed JIT/AOT controls, new actual profile and owning
startup proofs, then the unchanged required-suite retry. V16's captured runtime
still has the getcwd failure, and its Host still omits stdlib_dir. Keep B28's
2616 completed/2609 passed journal, four failed-subtest IDs/20 events, one fatal
case without outcome, one required skip/two required XFAILs and 2251 required IDs
without completion separate from these focused controls. Expanded D10, new193,
the complete owning/ABI/thread/GC/lifecycle/rooted/delivery/audit matrix and P6
remain open. Coordinator A records main plan/implementation/validation status.

# Fresh full-source compiler/build gate, 2026-10-24

The full 175-unit build-only retry after compiler repair `260387e88` passes:
174 unchanged selected upstream units plus the existing compatibility adapter,
linkage, RAW build, semantic processing, processed build, and both final private
publication builds. No emitted RAW C# was edited. The successful builds each
report zero errors and the two existing `CS0649` warnings for
`Libc.VectorIoEntry.Base` and `Length`.

`runtime_executions=0`, `tests_executed=0`. No test, application entrypoint,
provider probe, protocol/cipher suite, original held suite or controller ran.
Compiling the unchanged provider project as an ordinary dependency establishes
build evidence only. P4/P6 runtime qualification and canonical product adoption
remain open. No private provider candidate was applied.

Evidence root:
`/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only`.
The [machine summary](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/evidence-summary.json)
binds exact receipts, commands/logs, per-unit input/object identities, output
assemblies, tool identities, failed attempts and preservation checks.

## Execution and placement

The external `driver.py` adapts the existing campaign `Context`, original
recipe preparation/configuration, shared `translate`/`deliver`, transactional
`promote`, and original translation-closure `freeze` APIs. It does not duplicate
the translation engine. All 175 fresh objects were emitted once and linked once.
After a delivery-placement failure, external `resume.py` reused those actual
fresh objects/link and invoked shared delivery directly; it did not repeat
emission or linkage. There is no shared framework or harness implementation
change in this package.

The final chain contains 186 successful commands: 178 actual successful commands
from the retained emission/link attempt and eight actual commands from resumed
delivery. The failed RAW build remains in its original unsuccessful receipt;
it was not retagged. Exact argv, cwd, exit status, timestamps and logs are in the
[completed campaign receipt](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/publication/artifacts/campaign/20261024-124727-aa58fb51/receipt.json)
and [translation closure](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/publication/artifacts/campaign/20261024-124727-aa58fb51/translation-receipt.json).

Executed driver commands from `/home/marius/p/dotcc`:

```bash
python3 -B /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/driver.py
python3 -B /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/resume.py
python3 -B /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/review.py
```

The preserved scripts enforce attempt retention; these are execution history,
not instructions to overwrite this evidence root. `attempts.json` identifies the
exact script snapshot and log for each failed preflight and build attempt.

The compiler is the existing immutable
`root-braced-string-compiler/immutable-tools/DotCC/dotcc.dll`, SHA256
`7a280dfe9557e6f42d3aefc4d6792489e76ee71f8cca3df9efc834101456d0b8`;
`DotCC.Lib.dll` is
`01c64a51ca9942dd78cd808a91f753288de48611beec07a97b1c3ad1d3de00bc`.
The unchanged original 223542 campaign postprocessor is
`922653528d7f17be78ac36e6f06a77267c4c00f0ee8662f8ba2aae8336d0de99`.
Both use private copies through the shared legacy snapshot path. Their historical
snapshots omit localization satellites declared by their dependency manifests;
this failed the newer strict tool validator in the first preflight. The supplied
runtime assemblies are identified, no tool was rebuilt, and no historical cache
was reconstructed. The actual complete build proves the identified tools execute.

.NET SDK is `10.0.112`; emission workers and `DOTNET_PROCESSOR_COUNT` are both two.
Every command's admission checks retain the 2 GiB filesystem reserve. The
pre-existing declaration-header stamp matches its source digest before recipe
preparation, and no native header preparation executes. Exact locked package
copies, CLI home, temporary paths, tools, successful managed outputs and final
publication reside externally, with no symlinks.

Private `Directory.Build.props` imports the original repository settings.
Global `UseArtifactsOutput=true` and a distinct `ArtifactsPath` for each delivery
phase place both the generated project and its original provider project outside
the checkout. `ProjectReference.AdditionalProperties` preserves that placement
for the semantic workspace. During processing only, byte-identical private copies
protect the two linked authored files; their bytes are verified unchanged before
restoring original authored links. Both final products retain ordinary references
to the original authored project and files. Semantic processing rewrites 20,773
`Cond.B` calls, simplifies 7,105 comparisons and removes 46 empty blocks across
35 generated files.

## Preserved failures and placement deviation

Three failed attempts are retained. The first preflight rejected absent tool
localization resources. The second incorrectly compared a signed package's raw
archive SHA512 with its signed content hash; the correction verifies the archive
sidecar, locked metadata, exact previous signed-content verification and copied
package bytes.

The first actual full-source attempt emitted and linked successfully, then failed
RAW build with `NETSDK1004`: NuGet restore did not propagate reference-only
`ArtifactsPath`, wrote restore files into the original provider `obj` directory,
and the later build could not find assets in its external directory. This is a
real deviation from the requested output placement. Original cache bytes before
that restore were not captured, so no restoration or fully external history is
claimed. The failed command/log and observed cache manifest remain in
[placement-failure.json](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/failed-attempt-03/placement-failure.json).
Later Rider metadata timestamps prevent complete attribution of cache changes.
No provider binary was built by that failed command.

Resumed delivery adds global placement properties to every build/restore command.
Its RAW, semantic, processed and final builds pass with external outputs. The
original provider `obj` bytes remain unchanged throughout this successful resume,
as recorded in `successful-resume-original-cache-postflight.json`.

## Verification boundary

The separate read-only verifier process reconstructs all 175 selections and
source/object mappings, checks each command/log and output, compares all 36 RAW
C# files with their bytes immediately after linking, verifies 352 compiler source
files against the immutable compiler identity, and independently evaluates four
final product/authored-project reference graphs. It hashes 9,185 evidence files
and verifies all 28,603 protected files by hash, size and mode. Original reference
trees, source/configuration files, original 175 objects/tool snapshots, published
products, current pointers and selected failure ledgers remain unchanged.
Administrative documents are recorded only as mutable coordination context and
are excluded from the immutable workload dependency closure.

[Verifier receipt](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/review/separate-verifier.json)
and [per-unit selection](/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/full175-build-only/review/unit-selection.json)
are available for supervisor review. The verifier is authored by this implementation
agent; it is a separate verification process, not an independent agent review.
Supervisor acceptance remains separate. This closes the independently executable
full-source compiler/build retry gate for these exact inputs and tools, with the
failed placement attempt disclosed; it confers no runtime assertion credit.

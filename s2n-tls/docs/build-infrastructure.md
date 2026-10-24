# Independent build and preserved-evidence tools

These tools perform ordinary source-build, packaging, evidence and storage work.
They do not run the translated product, provider, native oracle, C/Python suite,
campaign reproduction or verification gates. They do not update the campaign's
ledgers. Use a new private external output directory for each attempt; all
allocation filesystems must retain the normal 2GiB reserve.

## Reconstruct a source-only generated harness

`Scripts/managed_source_build.py` accepts an existing flat generated-source
manifest and a source-only SDK project. It builds the declared original source
paths, creates a portable source archive, and builds the extracted archive in a
second clean location. No generated C# is edited and no DotCC tool is built or
invoked. Projects containing undeclared package/project dependencies, imports or
unsupported properties fail before reconstruction. Use repeatable `--reference`
for explicit existing assembly dependencies and `--notice` for their notices.

For the preserved, independently qualified string-initializer reduction:

```bash
python3 -B Scripts/managed_source_build.py \
  --source-project /mnt/storage/dotcc-s2n-qualification-20261024/independent-infrastructure-20261024/compiler-connection/reduction-fixed/reduction-fixed.csproj \
  --notice /home/marius/p/dotcc/LICENSE \
  --output-root /mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/infrastructure/string-source-build-final
```

The receipt records exact original/generated/package sources, actual compiler
inputs and resolved reference assemblies, framework dependencies, outputs,
commands (including production lock/limit arguments), environment and logs.
NuGet sources are cleared for these source-only builds. Artifacts, temporary
files, CLI home and NuGet caches are private external paths. The source archive
includes original project/manifest provenance and supplied notices. A successful
result is `passed-build-only`; application execution remains unrun.

## Audit preserved results and stage dependency copies

`scripts/audit-preserved-build.py` requires an existing independent audit binding
the exact old manifest and actual qualification receipt. It rechecks all four
original assertion transcripts, source trees, execution identities and actual
output/runtime files. It separately inventories historical build-input hashes:
only missing paths under explicit, bound cache-cleanup entries are reported as
retired. Required source/runtime/output files must still exist and match.

Supply `--manifest`, `--qualification`, `--previous-audit`, `--case`, a new
`--output-root`, and explicit repeatable `--dependency-output-root` paths inside
that new root. The latter scopes are passed to the dependency-storage selector
at both staging and optimization. Fresh package copies have separate inodes
from the preserved actual output files; optimization acts only on the copies.
No NuGet/provider/application executable is launched by this audit.

Storage errors are optional only while original evidence and new package bytes
verify before and after the operation. After an error, allocation savings are
unknown. Successful receipts distinguish gross replacement savings, new store
allocation and net savings. Staged files remain labelled unexecuted package
copies. `passed-preserved-evidence` supplies no fresh runtime or canonical case
credit; original incomplete/failure statuses are retained verbatim.

Focused regression commands, with `TMPDIR` placed externally:

```bash
python3 -B -m unittest discover -s Scripts/production_tests -p test_external_source_build.py -v
python3 -B -m unittest discover -s Scripts/production_tests -p test_managed_test_storage.py -v
```

# Refreshed ordinary consumer builds

2026-10-24: raw and processed JIT builds and trimmed Linux x64 NativeAOT
publishes pass against the new full175 publication recorded by `d83e1a0b0`.
Each has zero build errors and the two existing generated `VectorIoEntry`
CS0649 warnings. Applications were never launched. This closes the new-tool
ordinary consumer compile chain only; runtime delivery, P4 and P6 remain open.

The original `samples/ManagedConsumer/ManagedConsumer.csproj` references the
original `src/Managed.S2nTls/Managed.S2nTls.csproj`. Global `S2nTlsProject`
selects the private raw or processed generated copy. Those copies retain their
original provider and authored C# references. No C# source was edited, no TLS
engine was substituted, and no C emission, linking, compiler regression,
provider/protocol test or test controller was run in this round.

The fresh external scope is
`/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/consumer-build-only`.
Only the refreshed published source trees were copied, using
`campaigns.delivery.ignore_build_caches` and `translation.relocate_project`.
Only copied project/props metadata changed to place references, artifacts,
restore configuration and editorconfig output in the new scope. Every build,
restore and MSBuild evaluation passed global `UseArtifactsOutput=true`, a
private `ArtifactsPath`, and the selected private `S2nTlsProject`, including
evaluations/builds of the original API and provider projects.

Exact output paths relative to that scope:

- Raw JIT: `artifacts/raw-jit/bin/ManagedConsumer/release/ManagedConsumer.dll`.
- Processed JIT: `artifacts/processed-jit/bin/ManagedConsumer/release/ManagedConsumer.dll`.
- Raw NativeAOT: `publish/raw-aot/ManagedConsumer`.
- Processed NativeAOT: `publish/processed-aot/ManagedConsumer`.

The [summary](consumer-build-only-summary.json) records absolute paths and SHA256
identities. The native outputs have ELF64 little-endian x86-64 headers. JIT uses
`UseAppHost=false` and produces framework-dependent DLLs; trimmed NativeAOT
produces native executables. No additional whole-library roots or descriptors
were introduced. Whole-library P5 evidence remains tied to its prior snapshot.

Dependencies are .NET SDK 10.0.112, Microsoft.NETCore.App 10.0.12 and the unchanged
BouncyCastle.Cryptography 2.7.0 package. The SDK implicitly selects ILLink and,
for AOT, ILCompiler 10.0.12 plus its exact declared runtime/compiler downloads.
Only seven selected packages were independently copied from preserved pinned
caches; no broad cache reconstruction or package download occurred. Original
provider lockfiles were used in locked mode. Sample/API have no own lockfiles;
none were invented. Restore graphs and assets distinguish their actual package,
framework and project references from unrelated central version declarations.
Signed runtime/compiler archives passed fresh offline NuGet signature/content
verification; the BC archive uses its unchanged retained signed-content proof.
Extracted files and private copies were checked against the exact archives.

All work used private TMP/CLIHOME/NuGet directories, two processors/jobs and the
unchanged 2 GiB reserve on both filesystems. The separate verifier checked
37,798 protected files, all 9,191 files in the fullsource evidence tree,
36 unchanged emitted C# files per form, 116 compile inputs and 180 assembly
reference inputs. Original sample/API/provider bin/obj caches were unchanged
during this round. The earlier fullsource provider obj placement deviation and
all three failed fullsource attempts remain preserved; no removal, restoration
or complete attribution of shared IDE caches is claimed.

The external scope contains the exact argv/logs in `commands.json`, sixteen
reference evaluations under `resolved/`, assets under `assets/`, four receipts
under `results/`, the staging/build driver, and a separate verifier. Two bounded
receipt-helper errors are retained in `harness-events.json`; their corrections
required no repeated compilation. Source/evidence before/after inventories and
all bin/publish outputs remain available for parent review.

`separate-verifier.json` SHA256:
`cf3e1ac08c8635bf7c5a78a6dbf7a658e5bf939bc3fb80cac8388a306f29e052`.
`evidence-inventory.json` identifies 2,104 retained files; SHA256:
`22dbe6154ed6541b4e3b2ea560c8c30b7cddad2c28d05af1c5fbc2ab3c33e04f`.
This verifier is a separate process authored by the implementation agent;
independent parent review is separate.

Required original C/Python qualification, held provider work, new-source consumer
runtime/delivery acceptance, canonical adoption and sealed P6 remain incomplete.
The unsupported test-host fork question is outside this consumer build chain;
no fork experiment or test-host execution occurred. Central ledgers remain
parent-owned and untouched by this round.

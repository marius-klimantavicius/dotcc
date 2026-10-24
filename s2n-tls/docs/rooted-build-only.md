# Refreshed whole-library-rooted NativeAOT builds

2026-10-24: both refreshed full175 raw and processed products pass Linux x64
whole-library-rooted NativeAOT publication. Each publish ran once, returned
exit 0, and reported two existing generated `VectorIoEntry` CS0649 warnings
and zero errors. Applications and identity entrypoints were never launched:
`runtime_executions=0`. This closes only the refreshed P5 rooted compile gate.
P5 rooted runtime, complete P4 qualification and P6 acceptance remain open;
captured older rooted runtime evidence remains specific to its source snapshot.

The private scope is
`/mnt/storage/dotcc-s2n-qualification-20261024/unaffected-continuation-20261024/rooted-build-only`.
Its `evidence-summary.json`, `separate-verifier.json`, `validated-builds.json`
and `commands.json` contain the results, exact commands, exit codes, warnings,
source identities, native inspection and resource observations.

Fresh private copies came directly from the raw and processed publication
recorded by `d83e1a0b0`, using unchanged compiler repair `260387e88`. All 36
emitted C# files per form remain byte-identical. Only copied project/props
metadata changed to relocate original authored references and build outputs.
No source, expected method or import declaration changed; no C emission,
original-C linking, shared tool build or test workload occurred.

The private harness derives from the original consumer project, links its three
original C# files, and references the original owning API project. It adds
`TrimmerRootAssembly Include="TranslatedS2nTls"`, matching the established
`tests/Delivery/run.py` root configuration. Evaluation confirms the generated
assembly name and whole-assembly root with no narrower descriptor. The actual
NativeAOT response file in each form contains `--root:TranslatedS2nTls` and
references that form's freshly built generated assembly. Both final files are
ELF64 little-endian x86-64 executables, inspected using `readelf` without execution.

Outputs, relative to the private scope:

- Raw: `publish/raw/ManagedConsumer`, 12,225,960 bytes,
  SHA256 `1d847255e50d1daf2b8df141ceeba409368cc3bfd4d08cd9298c177936f1ae61`.
- Processed: `publish/processed/ManagedConsumer`, 12,164,488 bytes,
  SHA256 `7394851a2cab6bb70639605a551da505e58ea5993777713a16a8a9d191938f3c`.

Nineteen recorded .NET commands include one locked restore and one publish per
form, source/reference evaluations, dependency graphs and SDK identification.
Every restore, publish and evaluation passes global `UseArtifactsOutput=true`
and a private `ArtifactsPath`. A private MSBuild import also sets those values
in every authored project reference's `AdditionalProperties`, including original
API/provider projects. Evaluated output paths and the actual four-project
reference graph stay in the private scope for build artifacts. SDK-created
transitive reference items are identified separately by their SDK origin;
each referenced project's actual settings and authored edge are audited.

Dependencies use SDK 10.0.112 and the exact current consumer cache bindings at
`consumer-build-only/{declared-package-selection.json,package-bindings.json}`.
The actual declaration selects seven packages, including SDK framework/compiler
download dependencies; this observed count supersedes the brief's six-package
shorthand. Only those seven package directories were independently copied and
checked against the bound archive, signed-content and extracted-member evidence.
BouncyCastle.Cryptography remains 2.7.0; SDK runtime/compiler packages remain
10.0.12. Provider lockfiles were used unchanged in locked mode; the harness and
API originally have no own lockfiles. No lockfiles were invented, no package
was fetched, and no broad old cache was reconstructed. Restore has no network
source; the SDK additionally lists its local `/usr/lib/dotnet/library-packs`.

All TMP, CLIHOME and NuGet outputs are external. Builds use two processors/jobs.
The unchanged 2 GiB reserve passed before/after every .NET command and at
2-second observations during longer commands. Minimum observed free space was
13,827,805,184 bytes on root and 373,208,690,688 bytes on external storage.

The separate verifier rehashed 40,007 protected files, including all 9,191 files
in the fullsource evidence tree and all 2,105 files in the completed consumer
evidence tree. Original sources/provider/current trees, original sample/API/
provider bin/obj caches, central documents and existing evidence retain their
bytes, sizes, modes and recorded tree membership. It also checked 108 compile
inputs and 174 resolved assembly inputs. This is a separate verifier process
authored by the implementation agent; independent parent review is separate.

Three receipt-helper failures are retained under `failures/` with the relevant
driver revisions in `attempts/`: a setup output buffer limit, a setup comparison
that confused signed-content and raw signed-archive hashes, and a post-publish
check that expected authored metadata on SDK-added transitive references.
Corrections required no repeated compilation. Both actual publishes passed on
their first attempts; this round has no observed output placement deviation.
The earlier fullsource failed restore's original-provider obj/cache deviation
remains preserved, with no all-attempts-external or restoration claim.

`separate-verifier.json` SHA256:
`a7eeb3b51c52c0e169ece9c6b1231f4042ff1bb4f3c1697f5df43cbe962e2f62`.

The prior automated provider rejection remains terminal. No private default-CA/
EC candidate, provider/protocol/cipher workload, Python/controller/test workload,
application entrypoint or TLS/network operation was run or applied. Untracked
BC/provider feasibility work is untouched. C62/10/185, Python13/1/58 and the
separate 2,588-case leaf remain unchanged. No canonical257 adoption, acceptance
masking, narrower rooting or suite reduction occurred. The fork/waitpid/signal
host gap remains unrun. Central ledgers remain parent-owned and unchanged.

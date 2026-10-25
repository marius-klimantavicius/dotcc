# Ordinary CoreCLR: normal ReadyToRun service profile

This profile publishes the **same** `WebHostBuilder` middleware source as the
successful `GuestKestrelMinimal` IL control. It changes the app deployment to
normal framework-dependent ReadyToRun (R2R). It remains a managed CoreCLR app,
not a NativeAOT guest. Runtime, JIT, generated native code, dependencies and app
instructions must all execute through Blink when the coordinator runs a guest.
This directory performs native preparation only; it never builds or runs Blink.

`Program.cs` is byte-identical to the successful control's retained source:
SHA256 `0090390e0cb8d705375b0272d735130f13b3e5d30429c905fcc4bf606820bf7b`.
The service keeps its finite 30-minute `RequestHeadersTimeout` and the public
`/health`, `/echo`, `/stop` contract. The original Slim app in
`../CoreClrRuntime/Service` is a distinct profile; none of its IL closures,
console results, service failures, or prior diagnostic evidence is replaced.

## Pins and reproducible preparation

`global.json` requires SDK **10.0.112**, with SDK roll-forward disabled. The app
targets **linux-x64**, `net10.0`, and both CoreCLR and ASP.NET **10.0.12**, with
framework roll-forward disabled. `Properties/PublishProfiles/Normal.pubxml`
sets `PublishReadyToRun=true`, `PublishReadyToRunUseCrossgen2=true`, and
`PublishReadyToRunComposite=false`; `IL.pubxml` supplies the matched IL control.
Both use `PublishAot=false`, `SelfContained=false`, and `UseAppHost=false`.

From the repository root:

```sh
DOTNET_PROCESSOR_COUNT=2 python3 blink/tests/CoreClrReadyToRun/build.py --samples 5
```

The default uses the existing successful minimal closure and local exact-version
NuGet packages. If an exact package is missing, explicitly add
`--allow-official-download`; only three named 10.0.12 packages from the official
NuGet flat-container endpoint are permitted. `--package-cache`,
`--package-feed`, and `--base-closure` can select retained inputs. Each invocation
creates a new `blink/artifacts/coreclr-runtime/readytorun-native/attempt-*`.
Generated sources, restore cache, SDK CLI home, binaries, logs and native work
directories stay there. No global package installation or settings change occurs.

The driver copies the exact nupkgs into a private feed, validates package IDs,
versions and full source commit, restores against that feed with no fallback
feed, then publishes with `--no-restore`. Every restore/publish uses
`production_support.locked_command`; locks enclose individual builds only.
MSBuild uses `-m:1`, shared compilation is disabled, and crossgen2 uses
`--parallelism:2` with `DOTNET_PROCESSOR_COUNT=2`. Restore has a 120-second bound,
publish 180 seconds, and downloads 90 seconds. Commands use owned process groups
and record cleanup. The compiler is the authentic linux-x64 crossgen2 **10.0.12**
package, not an older cached compiler. Its `--version` reports
`10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701`.
The selected SDK, retained CoreCLR/ASP.NET `.version` files and all three nupkg
repository records identify that same full commit.

The receipt binds exact commands, source snapshots, `project.assets.json`, nupkg
SHA256/SHA512, resolved tool paths, recursive compiler/runtime-pack file hashes,
the installed SDK and targeting-pack manifests, closure files and native logs.
The canonical NuGet linux-x64 implementation-reference assemblies differ in
bytes from all 313 retained Ubuntu runtime assemblies, despite the matching
version and source commit. Both sets are recorded; published framework files
are never substituted into the retained runtime. Native execution succeeds with
that closure. Static payload proof does not establish acceptance of every R2R
method by the runtime's versioning checks.

## Payload proof and positive regression

`inspect_pe.py` reads PE/CLI bytes without loading the assembly. It follows the
CLI `ManagedNativeHeader` to the `RTR\0` signature, version and section directory;
decodes `MethodDefEntryPoints`' sparse NativeArray; checks tokens against the CLI
MethodDef count and indices against x64 `RuntimeFunctions`; and verifies every
runtime function has file-backed executable code and unwind data. Reports contain
section hashes, compiler identity, native code byte counts/hashes, entrypoint
metadata, and counts of actual IL method bodies. It supports the x64 images used
by this profile; it is not a general verifier or disassembler. Composite owner
files and component headers are identified by their section types, not accepted
as proof of app-local native methods.

The format references are the official
[R2R specification](https://github.com/dotnet/runtime/blob/v10.0.12/docs/design/coreclr/botr/readytorun-format.md),
[runtime header definitions](https://github.com/dotnet/runtime/blob/v10.0.12/src/coreclr/inc/readytorun.h),
and the runtime's
[NativeArray](https://github.com/dotnet/runtime/blob/v10.0.12/src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/NativeArray.cs),
[NativeReader](https://github.com/dotnet/runtime/blob/v10.0.12/src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/NativeReader.cs),
and
[ReadyToRunReader](https://github.com/dotnet/runtime/blob/v10.0.12/src/coreclr/tools/aot/ILCompiler.Reflection.ReadyToRun/ReadyToRunReader.cs).
Copies and their SHA256 identities are retained in `readytorun-r23-discovery`.
The MIT notice for the runtime reader algorithms is in `LICENSE-DOTNET.txt`.

The authored regression compares **actual valid** IL and R2R publish outputs.
It checks retained IL method counts, absent IL native header, real R2R entries
and native code ranges, and no composite sections. No malformed inputs or fault
injection are used. To inspect the retained pair independently:

```sh
python3 blink/tests/CoreClrReadyToRun/test_inspector.py \
  --il blink/artifacts/coreclr-runtime/readytorun-native/attempt-wcalj6ct/il/published/CoreClrService.dll \
  --r2r blink/artifacts/coreclr-runtime/readytorun-native/attempt-wcalj6ct/normal/published/CoreClrService.dll
python3 blink/tests/CoreClrReadyToRun/inspect_pe.py --detail --require-native-payload \
  blink/artifacts/coreclr-runtime/readytorun-native/attempt-wcalj6ct/normal/published/CoreClrService.dll
```

## Native handoff evidence

Successful preparation: `readytorun-native/attempt-wcalj6ct/receipt.json`,
`passed=true`, `completed=true`, `native_validation.passed=true`.
The parent-ready closure is `normal/closure.json`; the matched IL closure is
`il/closure.json` in the same attempt. Top-level `closure`, `closure_sha256`, and
`image_files` always describe the normal profile. `pe_proof` and `source_manifest`
carry absolute paths and SHA256, and `profiles` supplies each closure's complete
file map, storage use, output assets and delta from the baseline. A separate
`blink/artifacts/coreclr-coordination/readytorun-r23-handoff.json` binds the full
native handoff, official references and final authored documentation.

The IL assembly is 10,752 bytes and has no managed-native header. The R2R
assembly is 19,968 bytes, format **16.0**, compiler identifier
`Crossgen2 10.0.1226.42308`: **16 MethodDef entrypoints**, **20 runtime functions**,
**3,061 native-code bytes**. Both retain 16 IL method bodies. Their SHA256 are
`05723e163933be51baadf74b2d70d6e5b297e392eb64b74fd26adcd9c0ee3d95` (IL) and
`bcc47fc61ae835ea5455fe989e7a0d2fbff028a510e28c5eb151ff24aae38b77` (normal R2R).

The retained CoreCLR framework has 92 R2R DLLs and 80 IL-only DLLs; ASP.NET has
138 R2R and 3 IL-only DLLs. **All 83 IL-only DLLs have zero executable IL method
bodies**, so these facade counts are not evidence of 83 uncompiled hot libraries.
CoreLib has 19,942 MethodDef R2R entries and Kestrel Core has 4,228. Distributed
app DLLs are inspected too; the sole app DLL is `CoreClrService.dll`, and its
deps file names no additional package/runtime dependencies.

Both closures preserve all **365** original files and the existing guest path
layout: `/dotnet/dotnet`, `/apps/service/CoreClrService.dll`. There are no extra
app dependencies. Only four app files differ from the minimal baseline: DLL,
PDB, deps and runtimeconfig. Baseline `web.config` is retained as support data;
the endpoints manifest remains identical. All runtime, framework, console, loader
and native library bytes remain unchanged. IL storage is **114,079,310 bytes**;
normal R2R is **114,088,526 bytes**, an increase of 9,216 bytes and below the
512 MiB guest storage budget. Licenses/third-party notices are retained in the
package closures and the attempt's `licenses` directory; retain these alongside
any redistribution of the image files.

Five fresh-process samples per profile, alternating launch order, each passed
real GET health, POST echo, POST stop, exact console output, clean exit and
released process group. Native launch invokes `dotnet` normally, using its
kernel `PT_INTERP`; invoking `ld-linux` explicitly would change `/proc/self/exe`
and violate the host basename guard. The kernel loader's hash matches the
retained guest loader. Readiness is bounded at 20 seconds and each HTTP operation
at 10 seconds; the process-exit wait uses the remainder of a 45-second budget.
These are per-step bounds, not an independent 45-second watchdog. No cleanup
signals were needed.

With the same environment and runtime-default tiering, median ready time was
**78.95 ms IL / 80.52 ms R2R**; first health completion since launch was
**114.32 ms IL / 123.89 ms R2R**; first health request latency was
**39.90 ms IL / 41.31 ms R2R**. These small, warm-filesystem native samples show
**no native speedup**. They do not predict interpreted Blink startup or instruction
counts. Input hashing is excluded from the timing origin; timestamps start just
before `Popen`, with a 1 ms readiness poll. There was no cache flush, CPU affinity
pinning or statistical claim.

## Normal versus composite and integration limits

[Microsoft's deployment guidance](https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run)
explains that R2R retains IL, framework libraries are already precompiled, and
generic instantiations or other cases can still require JIT work. Tiering can
replace R2R methods. Composite allows optimizations across a combined set, at
the cost of larger files and slower compilation, with a deployment tradeoff.
Normal R2R is the selected integration profile because framework payloads already
exist and there are no extra executable IL dependency assemblies. Subsequent
public execution passed three selected cells; measured cold latency remains high
and no R2R speedup has been demonstrated.

No composite profile was built or selected, and tiering was not disabled.
The public normal-R2R case used 1,268,294,227 instructions versus the historical
minimal IL-app control's 1,268,653,381. Different deployment metadata and probes
prevent a causal timing comparison. These nearly identical instruction totals
and the native controls provide no measured reason to add composite. A composite
experiment would require its own payload proof, native controls, runtime-pack
closure and storage check. No composite advantage or elimination of JIT is claimed.

This native preparation receipt remains native-only. Subsequent translated
qualification is recorded by the [public harness](../GuestReadyToRunPublic/README.md)
and [phase results](../../docs/P6-CORECLR.md#timed-normal-readytorun-result): processed
translation with a NativeAOT **host** in both modes, and raw/JIT-host/InProcess.
All three passed exact health/echo/stop responses and natural exit within the
original 3,600-second/3-billion-instruction bounds. Five other cells remain
unqualified. Host packaging does not make the guest NativeAOT or prove per-method
R2R acceptance. The earlier minimal IL control used 1,268,653,381 instructions
and 2,586.742 guest-command seconds (2,591.098 seconds for its full workflow).
Original Slim IL failures and the invalid R20 observer remain distinct results.

The prior coordinator automated content-filter stop is retained in receipts;
no safeguard was disabled or bypassed. The initial local preparation failure
`attempt-47c6duld` is retained: a closure-membership guard caught the SDK's absent
IIS `web.config` during replacement. The successful driver retains all baseline
files before overlaying the app's publish outputs. All commands and native
process groups have finished. There are no guest workers, commits, pushes, PRs,
delegated children or outstanding build jobs from this task. Keep successful
image/proof/tool artifacts while the coordinator uses the handoff; cleanup is
limited to these owned `readytorun-*` attempt directories after integration.

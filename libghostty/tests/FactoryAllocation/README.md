This is a private allocation diagnostic for the **actual canonical raw and
processed C# sources**. It adds no entrypoint to either product project. The
generator creates a separate executable project with absolute `Compile Include`
references to every canonical top-level `*.cs`, plus this directory's authored
`Driver.cs` and generated typed call sites. It never copies, rewrites, translates,
or builds the canonical projects and has no `ProjectReference`.

Run from the repository root, using a new private work directory:

```sh
python3 libghostty/tests/FactoryAllocation/run.py \
  --work libghostty/build/factory-allocation/my-new-run
```

The default runs raw/JIT, raw/NativeAOT, processed/JIT and processed/NativeAOT
sequentially. `--forms raw` or `--modes jit` selects a subset;
`--prepare-only` writes reviewable projects/call sites/inventories without a
build. `--raw-project`, `--processed-project`, `--raw-audit` and
`--processed-audit` override the canonical input locations. NativeAOT defaults
to the local Linux architecture; use `--rid` explicitly when necessary. The SDK
and NativeAOT toolchain must already be available. Each build/run has a bounded
1200-second timeout, adjustable with `--timeout`.

Every command, exit code, duration and complete stdout/stderr log is captured.
All generated projects, intermediates, native objects, executables and results
remain under the requested `libghostty/build` directory. `--artifacts-path`,
`--output`, `-m:1`, `IlcSingleThreaded=true`, `UseSharedCompilation=false` and
`DOTNET_PROCESSOR_COUNT=1` keep this independent of the ordinary product build
lock and limit CPU use. No campaign CLI, compiler/backend, root build, upstream
test campaign or direct Zig phase is invoked. Work directories cannot be reused,
so an unsuccessful attempt cannot overwrite a previous failure log.

The input accepted PE reports are
`libghostty/artifacts/aggregate-factories-{raw,processed}-accepted.json`. They
supply concrete factory return types and payload inventories. Preparation
checks their actual assembly hashes, strict IL/RVA/allocation acceptance,
canonical source count/bytes, exact source getter inventory, and every
zero-argument factory's name and return type. `Dotcc.SourceFiles.txt` must match
the complete top-level source set. The receipt records every source's SHA-256,
the canonical project and manifest identities, the accepted PE/report hashes,
the generated project/call-site/case hashes and the authored driver hash. Source,
project and manifest hashes are checked again around every private build/run.
The fingerprint is SHA-256 over the sorted concatenation of
`filename + NUL + uppercase-file-SHA256 + newline`.

The private NativeAOT executable roots its **entire assembly** with
`TrimmerRootAssembly Include="$(AssemblyName)"`. That assembly contains all
translated engine files, not just the measured factory graph. The generated
private project disables ambient `Directory.Build.props/targets` imports and
states its .NET 10 / C# 14 / optimized unsafe compilation settings explicitly.
It adds only the diagnostic assembly, startup object and source references.
The receipt identifies that private project separately from the canonical
library; this is source-identical execution evidence, not a measurement of the
existing product binary.

For the current inventories there are 3,277 getters and 92 `Create()` factories
per form. Each getter has a tiny no-inline wrapper whose body reads the actual
canonical property. Each factory call uses its actual named unmanaged return
type from the PE report. Managed function pointers provide statically visible
calls and NativeAOT generic instantiations. There is no reflection,
`MakeGenericMethod`, delegate construction, boxing or per-iteration allocation
in the diagnostic path. Dispatch consists of 53 methods of at most 64 calls;
the dispatcher itself also stays below 64 calls. Probe helpers have two
parameters and payload wrappers have none.

The driver first verifies the allocation counter using a deliberate escaping
managed array allocation. It then warms the **entire** getter/factory set before
the acceptance pass. Each pass does 256 local warmup iterations followed by
10,000 measured iterations per getter/small factory, or 1,000 for factories
returning at least 4,096 bytes. Measurement methods use
`NoInlining | AggressiveOptimization`; product attributes and runtime tiering
remain unchanged. The complete first pass's allocation total is recorded
separately. Every second-pass individual delta must equal **exactly zero**;
there is no threshold, retry-until-zero or skipped case.

Every getter iteration copies all bytes into a reusable destination and consumes
all of them through a no-inline checksum. Every factory iteration consumes every
returned value byte through the same checksum, using ordinary reusable typed
locals. Checksums reach a volatile sink. Destination allocation, static/type
initialization, BCL/hash setup, positive-control allocation and JSON/file output
are outside the counted loops. The runner also compares each copied getter's
SHA-256 and each factory's complete warmup/measurement checksum with the
accepted actual-PE report. It rejects incomplete passes, wrong sizes/iteration
counts, runtime identity mismatches and any nonzero allocation. The driver
records exceptions and incomplete results; the runner preserves build and
execution failures and continues to the remaining independent controls.

The one `Create(Args)` factory receives no invented argument. Its accepted
IL/callgraph audit is checked and its identity is explicitly listed in
`excluded_argument_factories`; this harness makes no dynamic allocation claim
for it. Actual consumer coverage remains separate. These measurements cover
managed allocations on the executing thread under the stated warmup conditions.
Native allocations are not measured by the GC counter; the existing accepted
IL/callgraph audit separately rejects known allocation services. These controls
do not establish terminal semantics, owning-consumer behavior, or upstream test
qualification. Canonical product sources retain zero diagnostic entrypoints.

The authoritative machine-readable evidence is `receipt.json`, each form's
`cases.json`, and `outputs/{jit,nativeaot}-result.json` in the chosen work
directory. A future campaign hook can invoke the same standalone command through
`ctx.run`; this addition deliberately changes no campaign configuration.

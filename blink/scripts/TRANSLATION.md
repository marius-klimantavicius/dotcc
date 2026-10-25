# Deliver the translated Blink library

From the repository root on Linux x64, with .NET 10, Python 3, GCC/binutils,
make and NativeAOT prerequisites installed:

```sh
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh
```

Use `--offline` once the manifest-pinned Blink source archive is cached in
`blink/ref`. This restricts Blink fetching; normal NuGet restore may still use
installed packages or configured package feeds. `--jobs 1..4` controls source
emission concurrency; `--timeout <seconds>` bounds each translation unit.
The script resolves repository paths itself, so it can be invoked from another
working directory. Its compiler and postprocessor must already be built; it
never rebuilds those shared tools during translation.

The stable result is:

```text
blink/generated/TranslatedBlink/
  TranslatedBlink.csproj
  Sources/                         # compiler-emitted, then postprocessed C#
```

`TranslatedBlink` is the assembly name. The translated type is
`Managed.Emulation.BlinkCore`; generated C records are nested as selected by
`--nest-types`. The compiler's actual managed-library link uses
`--instance-methods --runtime=c --literal-pool --deduplicate-inline --split=size --split-size=102400`
for the default threaded profile; object emission also selects the same instance ABI. No generated C# is manually
modified. A separate authored project file supplies relative build references.

An external application references `TranslatedBlink.csproj`. Host types in
`Managed.Emulation.Host` arrive through its original source project reference.
Bridge Compile items link original files under `src/Host`, with headers kept in
`src/Host/include`. Preserve these parent-relative source paths when relocating
the final project. The generated API remains unsafe and requires the host
binding/lifetime contracts. The separate authored `Managed.Emulation.ThreadedExecution` project owns
initialization, loading, budgeted execution and cleanup through upstream exports.
Ordinary consumers use the safe `Managed.Emulation` machine API, which runs
in-process by default or explicitly in a separate worker. Both modes have
actual JIT/NativeAOT public consumer qualification. The older single-thread
`Managed.Emulation.Execution` project remains a historical profile.

Each invocation performs pinned fetch verification, the existing pinned native
assembly tests, a new frozen core profile, identity-checked complete closure
emission/link, and a delivery link with size splitting. Object reuse is allowed
only when the existing assembler verifies full emission identity and object
hash; producing profiles and receipts are retained explicitly. It never treats
an older compiler's objects as compatible merely because a filename matches.
Set `DOTCC_BLINK_FRESH_OBJECTS=1` on translate/verify to pass `--fresh` to the
assembler and emit every selected object, retaining prior cache/evidence files.
The strict ELF qualification gate additionally requires zero reused objects;
strict hash checking alone permits valid identity-checked object reuse.

The shared framework builds the raw project, processes a separate candidate,
and builds both published forms. Raw output is published at
`blink/generated/TranslatedBlink.Raw/`, with processed output at
`blink/generated/TranslatedBlink/`. Both link current authored sources and carry
source ownership/hash inventories. These are buildable deliveries, not read-only
archival directories. Existing older immutable snapshots remain untouched.
Translation builds execute no guest code or custom failure tests.

Only a validated postprocessed source bundle replaces the stable directory.
Old output is archived under the new attempt's `previous-output/`; failure in
the directory replacement restores it. A single translation lock serializes
publishers. The two directory renames are not a cross-process atomic transaction;
finish active consumer builds before regenerating the stable path. Temporary
build caches are discarded before publication; the final direct-source build
may create fresh bin/obj output, excluded from the source manifest.

`blink/artifacts/campaign/<run-id>/receipt.json` records recursive tool snapshots,
frozen profile/object identities, source inventories and actual command results.
`artifacts/campaign/current-threaded.json` points to the published threaded
producer. Completed runs have `status: passed`; failed attempts retain their logs.
Specialist gates validate that receipt and pin a completed delivery snapshot
before verification appends new command results. Hash policy does not relax
source selection, executable dependency availability or behavioral comparisons.

`bash blink/scripts/verify.sh --tools reuse --fetch never` translates and runs
source-input controls, independent host files/sockets/I/O checks, the consumer
build/help matrix, native plus four-mode managed GuestThreads, and the actual
public machine API in JIT/NativeAOT with both execution modes. Linux x64,
GCC/binutils, make, `strace`, .NET 10 and NativeAOT prerequisites are required;
missing prerequisites fail explicitly. `test.sh` alone defaults to the consumer
build/help smoke test. `test.sh --suite guest-threads` and `--suite machine-api`
select maintained runtime qualification against the published delivery without
retranslating. These specialist gates have their own fixed runtime matrices;
`--mode`/`--form` selects the generic consumer matrix only. The Kestrel service
`consumer-delivery` suite remains explicit and additionally requires Docker and
its pinned NativeAOT guest build. The [ELF runtime suite](../tests/ElfRuntime/README.md)
separately qualifies finite static/static-PIE and explicitly enabled guest-glibc
loading. Resource limits alone do not enable ELF interpreters or establish
general runtime support.

This gate establishes translation, semantic postprocessing and buildability of
the selected core. Runtime qualification, service startup, sample lifecycle,
platform coverage and indirect/framework boundary limitations remain separate.
Current test scope excludes custom fault injection and malformed-ELF tests;
only existing pinned upstream tests and ordinary functional execution belong
in reproduction commands.

## Authored source links and execution ownership

The active `generated/TranslatedBlink` project contains generated C# sources and
links original bridge files from `src` with parent-relative Compile items and
IDE Link metadata. Its Host ProjectReference points to the original
`src/Managed.Emulation.Host` project. No copied Host or Bridges directory is an
active product input. Edit authored sources in `src` and rebuild the solution;
regeneration preserves those edits. Immutable raw/profile copies remain archival
inputs for qualification. Delivery records the original source closure and
checks its hashes before and after generation and the final direct-source build.

Semantic postprocessing runs against private frozen copies for compilation
context. Only transformed generated sources are retained; private authored
copies are restored for validation and discarded from the active delivery.
The published project is built again at its final path against original `src`
files. Failed final builds roll the generated product back without writing to
those authored files.

The product excludes `CoreProbe`, its test `main`, and C execution orchestration.
A separate authored C# consumer uses the exported upstream functions/types and
jump-buffer transport. The test runner may add a probe frontend in a derived
test-only link with its own receipt. Canonical and delivery links enable
`--literal-pool` and `--deduplicate-inline`; these are compiler link options,
not postprocessor features. Deduplication merges provably equivalent static
inline implementations while preserving required address/state distinctions.
Core probe/replay and derived CoreExecution links also enable deduplication.
These script options apply to the next generation; existing generated files
and historical receipts are not rewritten or requalified by this change.

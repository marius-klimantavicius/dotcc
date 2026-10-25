# Valid Linux ELF execution

This suite exercises the current public `BlinkMachine` API and its matching
app-local worker. It qualifies a finite set of valid Linux x86-64 executables
using the translated upstream loader and the actual supplied guest glibc
interpreter/libc. It does not establish CoreCLR, general .NET, arbitrary distro
userspace, Windows, or broader ISA compatibility.

`build.py` builds and natively runs seven ordinary fixtures:

- Static `ET_EXEC` and static PIE entry probes check Linux RDX, initial argc,
  initialized data, zero-filled BSS and actual memory writes.
- The [private mapping workload](../GuestFileMapping/README.md) checks file
  tails, protection changes, BSS replacement, fixed/no-replace mappings,
  anonymous discard and descriptor-close lifetime through the public API too.
- A static baseline instruction guest checks real advertised FPU/MMX, x87
  load/add/store/FWAIT, selected FXSAVE/FXRSTOR round trips, MMX packed
  addition/shifts, EMMS and subsequent x87 use. Upstream stores x87 values as
  binary64; full hardware 80-bit precision and complete FXSAVE image fidelity
  are not claimed. Optional unqualified CPUID bits remain clear.
- Genuine glibc static `ET_EXEC` and static PIE run normal `main`, stdio/file
  I/O, `atexit`, destructors and libc shutdown. Static PIE retains actual
  `R_X86_64_RELATIVE` relocations.
- Dynamic PIE uses the retained glibc loader/libc and a fixture DSO, including
  dynamic data and TLS relocations. The DSO checks its own TLS/data/BSS and
  reads/writes that storage. Native execution invokes the retained loader with
  its explicit library directory and cache disabled.

Executable and DSO initialized/zero TLS are volatile, as are the data-pointer
relocation witness and BSS. `objdump --disassemble=main` must contain actual
FS-relative TLS reads and writes; the DSO must call `__tls_get_addr` and retain
`DTPMOD64`/`DTPOFF64` relocations. Native stdout, stderr, exit and written file
bytes must match fixed expected witnesses. Readelf, disassembly, commands,
tool/guest/library hashes and process-group cleanup are retained in the receipt.
Guest files are built by the installed compiler, never downloaded or executed
through a native fallback by the managed product.

The upstream entry-register bug is tracked here explicitly. Pinned
`blink/argv.c:LoadArgv` writes the pushed program-string address into RDX for
every binary. That is the upstream APE convention. Linux x86-64 glibc `_start`
instead consumes RDX as `rtld_fini`; a program string is later called during
shutdown. The clean-baseline investigation at
`/tmp/blink-elf-investigation-f5yfjns6/summary.json` records native success for
all five original guests; static glibc and static PIE reached `main` then
faulted during shutdown, and clearing only entry RDX made both finish normally.
Its baseline is `7cc9e373d2eda03ba787942db04510d8734cb271`; this transient evidence
is background, not a prerequisite or a replacement for the maintained suite.

The narrow [staging helper](../../src/Host/scripts/stage_elf_entry.py) changes only
the exact `Write64(m->dx, dx)` statement to choose zero for Linux and preserve
the program string for `system->iscosmo` (APE) and flat binaries without ELF
program headers. It does not change instruction
algorithms or immutable upstream files. The host-binding receipt records the
original/staged source hashes, helper hash and correction semantics. The native
`entry-register.c` control compiles the **actual adapted upstream LoadArgv body**
with a minimal guest-memory host and asserts Linux zero RDX, APE/flat program
strings, stack alignment and argc. The managed runner requires that
exact body in the fresh product's producer input.

Run from the repository root after the coordinator completes a wholly fresh
strict threaded translation:

```bash
DOTCC_BLINK_FRESH_OBJECTS=1 bash blink/scripts/translate.sh \
  --tools reuse --fetch never --hashes strict --jobs 4
python3 -B blink/tests/ElfRuntime/build.py
python3 -B blink/tests/ElfRuntime/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --native-receipt <printed-native-receipt> --form all --aot
```

The campaign entrypoint is `bash blink/scripts/test.sh --suite elf-runtime
--form all --mode all`, after a completed strict translation. This specialist
includes JIT alongside NativeAOT when AOT is selected.

The default is processed JIT. `--form raw|processed|all` selects product sources;
`--aot` adds NativeAOT to JIT. Omitting `--native-receipt` builds/runs native
controls first. Each selected form/runtime executes both `InProcess` and
`SeparateProcess`, with ordinary worker deployment through the maintained public
consumer targets. Static fixtures run with the default interpreter option.
The dynamic fixture explicitly opts in and also checks default denial, missing
interpreter, a directory at the interpreter path, and missing libc in otherwise
valid guest images. Denial and unavailable interpreters must execute zero guest
instructions; missing libc must exit through guest `ld.so` with its normal
diagnostic. All cases require released resources, and workers must be gone.
The libc omission confirms that host-native libc is never supplied implicitly.

The runner reuses `campaign_delivery.load_clean_delivery`, the maintained
MachineApi source-closure validator, and WorkerInstances snapshots/process-tree
cleanup. It checks today's entry and private-mapping adapter input hashes and
staged C bodies, rejecting stale producers. It copies all selected compiled
sources and references to private build trees; neither shared generated C# nor
compiler binaries are changed. Builds use one MSBuild node and production
command locks; every guest has a separate process, instruction/deadline limits
and an external timeout. Only valid ELF and ordinary functional/dependency
cases are created. No malformed ELF or custom fault injection is included.

Every attempt prints `passed=... receipt=...` on success or failure.
`blink/artifacts/elf-runtime/index.json` indexes the most recent native and
selected managed attempts. Attempt directories preserve failures, logs,
complete executable/worker closure hashes, native image identities and every
public API result. A suite pass applies only to its actual recorded matrix.

Observed on Linux x64 on 2026-10-25: the maintained campaign entrypoint passed
at `artifacts/campaign/20261025-092528-84a088c0/receipt.json`. Its managed receipt
is `artifacts/elf-runtime/managed-all-jit-aot/attempt-_zkql1l9/receipt.json`:
88 executions, comprising 56 successful fixture runs and 32 policy/dependency
cases. Native controls are pinned at
`artifacts/elf-runtime/native/attempt-bgvwb7_w/receipt.json`. The strict producer
is `artifacts/campaign/20261025-092144-2bee3a12/receipt.json`, with all 110 objects
freshly emitted and zero reuse. The combined index and selected regression
receipts are documented in [validation](../../docs/VALIDATION.md).

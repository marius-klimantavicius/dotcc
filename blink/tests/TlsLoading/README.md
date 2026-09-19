# Valid static TLS runtime startup

This focused fixture is a normal, fixed x86-64 Linux executable with one
`PT_TLS` segment: eight initialized bytes, an eight-byte zero tail, and
eight-byte alignment. Its own guest startup copies the template and zeros the
tail into a 16-byte writable, nonexecutable runtime block. It calls
`arch_prctl(ARCH_SET_FS)`, checks the successful result, reads the initialized
value and zero tail through FS, writes and rereads the tail, verifies a sum,
and exits zero only when every assertion succeeds. It emits no guest output.

```bash
python3 blink/tests/TlsLoading/run.py --assembly-receipt <qualified-core-objects/receipt.json>
```

Run after the native oracle and current complete core are qualified, with their
compiler and profile frozen. `--native-only` runs the fresh guest on Linux,
native Blink with JIT/linear memory disabled, and the unchanged native archive
adapter; it records `native_passed` without claiming managed completion.

The runner builds the authored assembly and linker script, validates the
resulting valid ELF's TLS/program headers and template bytes, records source,
tool and binary hashes, and snapshots exactly those bytes. The managed consumer
accepts only that generated fixture hash. No malformed variants or custom
fault-injection cases are constructed or executed.

Linux and the native Blink CLI prove the guest's internal assertions through
exit status zero. They do not supply a hardware register-state transcript. A
separate native adapter invokes unchanged `LoadProgram` and
`ExecuteInstruction`, requiring 25 completed instructions before the normal
exit trap within a 128-instruction bound. It observes the actual FS base, TLS
values, sum, exit IP, guest permissions and exit status. It reads the loaded
guest program-header entry through `CopyFromUser` and verifies every PT_TLS
field, independently of the fixture's on-disk header checks. Raw/optimized JIT and
whole-library-rooted NativeAOT must match that deterministic state transcript.

The managed link retains the exact qualified canonical objects and physical
headers, replaces the core probe driver with this bounded TLS driver, and uses
the qualified software-protection HostMemory implementation. All retained
producers and new source/object hashes are recorded. The unchanged native
archive/profile and the staged managed derivation remain distinct; this runner
does not claim separately executing a rebuilt staged native archive or freshly
emitting every retained core object. Generated C# is never edited.

The consumer binds private host services on one thread, verifies its three
standard descriptors and zero charged mappings after teardown, then discards
the process. It never reuses global interpreter state after disposal. Captured
guest stdout/stderr must remain empty. Receipts record execution-time binary
hashes before and after each run and preserve failed attempts.

This qualifies explicit runtime TLS setup in one valid static image. It does
not claim that the ELF loader allocates TLS implicitly, implement a complete
libc thread-control block or dynamic TLS ABI, or qualify guest threads,
relocations, service execution or Windows. Execution evidence is required;
the presence of these sources alone is not a passing gate.

## Observed Linux x64 qualification

`artifacts/tls-loading/attempt-4arxvilg/receipt.json` passes, SHA256
`459a13a7df40990e692f0e23ba115758da144f28231850c513839ea7aea6c7d9`.
The guest exits zero on Linux and the original native Blink CLI. The native
adapter and all four raw/optimized JIT/NativeAOT consumers agree exactly:

```text
tls filesz=8 memsz=16 align=8 phdr_preserved=1 runtime_rw_nx=1 fs=401010 initial=1122334455667788 updated=9 sum=1122334455667791 steps=25 halt=-10 exit=0
```

The fixed ELF SHA256 is
`eac738a2b874ebf6bf0dc51b3b4138c3ee5914abbf1f85b4c31ab4f5b4a1519b`.
All consumers pass descriptor/mapping cleanup checks. The derived link retains
107 qualified objects and emits the HostMemory replacement plus TLS driver;
its receipt records their source/compiler/producer identities and execution
binary hashes. `launch.json` records the isolated temporary directory and exact
command. No compiler, profile, host implementation or generated source changed
during the run, and no command failed.

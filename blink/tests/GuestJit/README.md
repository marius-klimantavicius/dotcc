# Experimental guest JIT qualification

`run.py` owns bounded processes and serial builds. It prepares normal static ELF
fixtures with Linux GCC/binutils, runs their hardware witnesses, freezes authored
C# plus the unchanged translated producer, and runs the same tests under CoreCLR
and optionally NativeAOT. The consumer deploys its matching worker.

```sh
python3 blink/tests/GuestJit/run.py --output /absolute/new-attempt --aot --bench
python3 blink/tests/GuestJit/run.py --output /absolute/new-raw-attempt --form raw --aot
```

The suite checks exact final CPU state and issued/completed counts at small
limits, scalar flags/widths/branches, SIMD and checked-memory fallback, private
code patch/remap byte validation, shared file/memfd coherence and permissions,
futex/signal/TLS/churn regressions, ordinary stop/deadline/cancellation, bounded
cache teardown, and two/four independent owners with moving collections.
The scalar fixture covers 32/64-bit LEA (base/index/all scales, signed
displacements, RIP, wraparound and aliasing), register MOVZX/MOVSX/MOVSXD,
high-byte sources, padding and ENDBR64. Its full-state comparisons stop at every
boundary 1..128 and 255/256/257, require entirely native prefix execution, and
preserve exact rejected issuance. The original limits and fallback controls
remain in the suite. Blink instrumentation NOPs and segment/address prefixes
remain interpreter boundaries.
The `loads` fixture checks private initial-data MOV/MOVZX hits and legal
cross-page misses at every boundary 1..65 and 127/128/129. `static-workers`
requires native instructions after child creation and permanent revocation of
an isolated main ELF code page before legal patch/MAP_FIXED replacement, while
the child executes another page. Anonymous and shared replacements interpret.
Public guest cases run in both process modes. A diagnostic threshold of one
forces native coverage in short arithmetic/semantics fixtures; normal runs use
the production threshold. The benchmark alternates fresh interpreter/JIT owners
on identical register/stack/file-data ELFs, marks first trials as host warmup,
and reports raw timings without speed assertions. Memory controls can complete
entirely in the interpreter and remain valid complementary measurements.

If `artifacts/campaign/current-threaded.json` is absent, the receipt records
`producer: null` and `producer_provenance.qualification: source-snapshot-only`.
The frozen authored/generated source hashes still identify the tested snapshot;
this runner never fabricates a producer receipt or asserts fresh translation.

After the audited producer and parent lifetime protocol are qualified,
`--emitter-contract` additionally builds/runs `ContractTests` serially in the
selected host runtimes. It links the actual emitter/ABI sources and checks all
load widths, the fourth descriptor, bad tags/permissions/identity/generation,
null host/array, bounded counts, invalid extents, canonicality, page crossings,
stop/attention, rejected issuance, partial-block misses and complete Machine
bytes. Native capability denial prints SKIP and does not qualify native entry.
No hand-edited translated sources or ordinary guest fault injection is used.

For real Windows x64 qualification transfer a frozen source closure and Linux
fixtures, then use `qualify-windows.ps1`; see the architecture documentation for
prerequisites and outstanding native Win64 ABI/debugger checks. Linux passes or
win-x64 cross-compilation do not qualify Windows execution. OS policy denial is
an expected interpreter fallback, not a native pass.

[Architecture, API and qualification limits](../../docs/EXPERIMENTAL-GUEST-JIT.md).

The address-plan milestone adds `address-plan.S`, an ordinary private-memory
prologue/load-chain witness, with complete CPU comparisons at boundaries 1..192
and 255/256/257. `AddressPlanTests` links the actual bounded value/effect planner
and checks reaching addresses, aliases, unknowns, stack/control flow and bounds.
`ContractTests/AddressPlans.cs` compares complete Machine/RAM over caps 1..32 and
budgets 0..21 for the retained twenty-instruction hot prologue, plus each memory
miss boundary. The full-descriptor native control is supplemented by the public
interpreter comparisons; it does not qualify resolver/ownership paths alone.
`EmitterAllocationTests` compares the r16 bytes and records exact warmed
allocations; the first milestone's corpus is byte-identical on the same OS.
Later backend ABI changes require semantic qualification rather than assuming
that historical byte comparison still applies.

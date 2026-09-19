# CPU conformance seed corpus

Run `python3 blink/tests/CpuConformance/run.py` on Linux x86-64. It builds an
independent hardware witness and a standalone interpreter consumer linked to the
pinned native archive, then runs each of twelve cases in a separate subprocess.
It does not build or qualify the managed core.

Both consumers execute the same literal instruction bytes and start with the
same RAX/RCX/RDX, arithmetic flags, XMM0/XMM1 and memory contents. `describe.c`
exports every input byte, code/data placement, step bound, fault expectation and
flag mask into the receipt's `corpus.stdout`. All 4KiB/8KiB mapped data contents
are compared, not just the location an instruction should modify.

| Case | Witness |
| --- | --- |
| ADD overflow | Signed boundary `INT64_MAX+1`, six arithmetic flags |
| ADD carry | Unsigned all-ones plus one, six arithmetic flags |
| SUB borrow | Zero minus one, six arithmetic flags |
| SHL count64 | Masked-zero shift preserves value and arithmetic flags |
| SHR count1 | Defined CF/PF/ZF/SF/OF; AF excluded |
| SAR count63 | Defined CF/PF/ZF/SF; AF and OF excluded |
| Signed IDIV | Negative dividend -17 divided by5; quotient/remainder from hardware; flags excluded |
| IDIV overflow | INT64_MIN divided by -1; real divide fault and faulting IP |
| SSE2 PADDD | Four differing 32-bit XMM lanes, wrapping values, flags preserved |
| Decode boundary | Ten-byte MOVABS begins three bytes before a page boundary |
| Data boundary | Eight-byte load/add/store straddles two guest pages |
| Data fault | Eight-byte load straddles an accessible page and unavailable second page |

SSE2 remains advertised by the campaign CPUID profile and its XMM execution
handlers remain selected (`docs/HOST-CPU.md`). No x87/MMX/BMI2/ADX/AVX behavior
is assumed here. This small corpus is not exhaustive coverage of any family.

## Independent hardware witness

`hardware.c` maps private test-only code/data, copies the corpus bytes, changes
code pages from writable to executable, initializes registers with a short
inline assembly trampoline, then jumps to the bytes. No arithmetic, division,
shift or SIMD result is computed in C. An appended INT3 stops successful cases;
Linux `ucontext` captures the actual registers, flags and XMM bytes. Division and
memory cases instead capture actual synchronous SIGFPE/SIGSEGV. Signal handlers
and executable mappings exist only in these short-lived native test processes,
not in the product host boundary.

Hardware INT3 advances RIP by one beyond the corpus bytes; the witness subtracts
only that sentinel byte. Interpreter output uses IP relative to its corpus
start. Success is compared at the next instruction; faults at the faulting
instruction. The hardware inaccessible second data page uses a PROT_NONE guard;
the interpreter's second guest page is absent. Only the common fault category
and restart IP are compared. Linux si_code and Blink's halt/signal-code outputs
are preserved independently and are not claimed identical.

Only architecturally defined flags are compared. IDIV flags are excluded;
fault cases compare fault/IP/memory and retain, but do not compare, general
register/XMM/flag captures. Other cases compare all initialized general
registers and both initialized XMM registers. Reserved/privileged flag bits and
Blink's internal lazy-flag bookkeeping are retained in raw captures but masked
out. The hardware does not count retired steps (reported -1); the interpreter
must meet its explicit successful-step/fault bound.

## Interpreter and provenance

`interpreter.c` uses unchanged pinned `NewSystem`, `NewMachine`,
`ReserveVirtual`, `CopyToUser`, `ExecuteInstruction`, `CopyFromUser`, and
synchronous `sigsetjmp` halt handling. Its only frontend signal hook records
upstream outcomes. Guest instructions, decoding, memory translation, fault
construction and SIMD/ALU algorithms are not replaced.

The runner snapshots the exact native archive, headers and config, corpus and
harness sources. It verifies the pinned source-inventory hashes and retains
compiler version, CPU/kernel identity, disassembly, link map, binary hashes,
full unmasked captures, complete mapped-memory dumps and comparison masks.
Each child has a bounded timeout; mismatch or abnormal exit fails the run and
preserves artifacts. It does not turn an unsupported hardware platform into a
passing reference.

`CpuInterpreterCase(index)` is the shared authored fixture entry. The native
runner above does not qualify managed execution. The separate translated-core
runner below reuses this same fixture and compares it with fresh hardware and
native interpreter outputs; P3 remains open (see `COVERAGE.md`).

## Actual translated-core consumer

Run `python3 blink/tests/CpuConformance/run-managed.py --core-receipt
<qualified CoreExecution receipt.json>`. The baseline must be a passing,
non-diagnostic actual core matrix produced by the same frozen compiler.

The runner retains all108 upstream/host objects from that109-object baseline
and replaces only its authored frontend/driver object. The new driver suppresses
the native CLI main, owns the single frontend signal hook, and supplies
`CpuConformanceRun(index)`. It seeds guest resource limits before NewMachine and
uses the same memory, file-reader, virtual-signal-action and exit-callback owner
lifecycle as the qualified core driver. No instruction implementation, upstream
source, existing profile, or generated C# is edited.

The exact canonical include paths are required, not only equal header bytes:
anonymous aggregate identities in the existing object format incorporate those
paths. Copies of these headers and their verified hashes are retained as
artifacts, while emission uses the canonical paths of the reused objects.
The derived object set and replaced object are recorded explicitly; it is not
presented as an unchanged canonical whole-profile cache entry.

Raw JIT, raw NativeAOT, postprocessed JIT and postprocessed NativeAOT each execute
all12 cases in separate processes, using copied frozen host sources/bindings.
Each process binds real private owners, forces compacting GC, executes the actual
translated core, runs exit callbacks before tearing down owners, and exits.
No translated call follows mapping disposal: upstream slab globals still point
into those mappings. The managed consumer never invokes the native witnesses.

Every managed result compares defined state with actual hardware and the halt,
completed-step and captured signal results with native Blink. Full mapped-memory
contents are compared; retained mapping counts and charged bytes are recorded
and bounded. The semantic postprocessor touches a copy, and raw generated source
hashes are checked at completion. A failed comparison retains its exact inputs,
outputs, compiler/object identities and diagnostic logs.

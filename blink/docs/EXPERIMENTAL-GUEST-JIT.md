# Experimental guest JIT modes

`GuestJitOptions.Mode` selects `Interpreter`, `Hybrid` or `Full`. Interpreter is
the default. Both JIT modes are experimental and opt in. Full's retained mixed
translation-block implementation is under qualification; current results,
failures and source pins are in the [paused checkpoint](../artifacts/jit-implementation/full-jit-20261027-r1/CHECKPOINT.md).
No complete Linux compatibility or acceleration claim is made.

```csharp
await using var machine = new BlinkMachine(new MachineOptions {
    GuestJit = new GuestJitOptions {
        Mode = GuestJitMode.Full,
        CacheLimitBytes = 128L << 20
    },
    InstructionLimit = 100_000_000,
    ExecutionDeadline = TimeSpan.FromSeconds(30)
});
```

- Interpreter uses the existing translated instruction interpreter.
- Hybrid opportunistically executes its native allowlist, with interpretation
  for other instructions or unavailable native admission.
- Full compiles retained blocks with generated native ordinary computation.
  Syscall, mapping/fault and authenticated clock/entropy data services are
  separate runtime boundaries. Remaining ordinary helper forms are coverage
  gaps. It has no interpreter fallback, including recursive signal execution.
  Unsupported executable publication fails explicitly; the old interpreter
  entry points have strict tripwires.

Legacy `Enabled=true` selects Hybrid when Mode is Interpreter. A nondefault Mode
wins. A per-run `ExecutionOptions.GuestJit` replaces all machine JIT options.
Both InProcess and SeparateProcess carry options and statistics; controller and
worker must use the same closure. CoreCLR/NativeAOT host and guest runtimes are
independent axes. Guests always use Linux x86-64 ELF and the Linux guest ABI,
including on Windows hosts. Actual Windows x64 CoreCLR and NativeAOT execution
remains **UNVERIFIED**; no mitigation is disabled to enable it.

## Retained blocks and reuse

Full separates a small per-worker nonowning lookup cache from an exact
owner-wide retained directory. Lookup collisions can require a directory lookup;
they cannot evict or recompile a fitting resident body. A retained body has up to
512 contiguous decoded instructions on one checked page, selected typed helper
plans, one native body, a generation and two optional successor links. The first
instruction may cross a page through ordinary checked demand fetch; such a body
uses bounded single-instruction admission.

Trusted resident lookup can reuse checked authority without comparing the entire
body again. The key and trust marker require a validated flat long/protected
context; nonflat and legacy contexts retain checked byte validation. Mapping,
physical-borrow and observed host-write generations revoke this shortcut.

Admitted scalar, memory, atomic, vector/MMX, floating-point, canonical x87 and
ordinary/REP string operations compute in emitted native code. Exact valid-form
coverage remains incomplete. Selected special helpers return from native code,
copy their precise typed plan and release native capabilities before managed
execution. No managed call, blocking syscall, GC transition or guest exception
unwind crosses a generated native frame. REP computation uses an independent
issued native kernel and precise mapping/fault continuations.

Compiler IR and emission scratch use four preallocated owner-wide unmanaged
slots for serialized translation and bounded recursion. Code staging is bounded
to64KiB; oversized emission halves/retries a decoded prefix before publication.
Code,
mutable descriptors, indices and payload append to dynamically charged arenas.
Tiny bodies share code pages; a staging transaction closes native admission,
requires all native epochs to drain, then seals the affected pages RW→RX before
publication. There is no writable executable alias or per-body code-page run.

Buffer exhaustion requests a quiescent whole-buffer flush, following QEMU TCG's
storage/reclamation principles without reusing its source. The flush unlinks
bodies, clears the retained index and worker hints, then rewinds reusable arenas.
Managed leases, native epochs and escaped drafts must all drain before reset.
Busy admission returns to the controller for an approved-issue retry. No
probation/protected or other per-body capacity replacement policy remains.
Content invalidation removes lookup/link authority but does not individually
recycle arena bytes. Identical-byte authority revalidation can retain emission.

Full defaults to a charged 1 GiB owner quota; Hybrid retains its 8 MiB default.
Explicit quotas are never silently raised. Charge includes owner/worker metadata,
physical authority tracking, compiler scratch, unused arena/region reservation and
page slack. An underfunded configuration fails before guest execution; a tiny
legacy quota is not guaranteed to admit the new engine. Capacity pressure has
explicit reclamation/denial paths and cannot escape to interpretation.

## Execution authority and precision

Retained native entry authenticates immutable body identity; mutable code/global
epoch, stop state and remaining issue allowance remain execution checks. Links are authenticated RW data
cells; invalidation unlinks them before body retirement. One global native epoch
protects every chained target from recycling. Normal invocation cap0 permits
linked execution until a real budget, stop, attention, authority or helper exit;
explicit nonzero caps accumulate across tail jumps. Recursive signal actors exit
after one. A designated compiler closes new native admission before draining old
readers through publication or cancellation. Native execution retains its epoch,
body lease and physical lifetime pin outside the authority gate.

Mapping mutation scope remains active until the C helper/cleanup transaction
finishes. Writable and unknown data-pointer borrows are tracked per worker and backing page,
including aliases and exposure before first code registration. Audited read-only
paths retain physical lifetime protection without revoking code. In
unconditionally paged machine modes, early virtual data writes avoid unrelated
global code revocation; successful writable resolution still applies the physical
code authority callback before exposing a pointer. Other modes remain conservative. Fetch provenance
records actual virtual/physical pages; writes revoke the affected aliases, and
identical-byte remaps refresh those links before reuse. A writer drains
polled native execution before its pointer escapes; a page with active borrowers
cannot acquire multi-instruction authority. No mapping/native gate spans a helper
or blocking I/O. Registry exhaustion fails closed for multiple execution.

Host memory-file writes and logical changes use scoped mutation observation.
Writer admission and native enrollment use a counter/epoch handshake without an
I/O lock in native code. Shared multi-instruction admission additionally requires
an exact active local memfd lease whose writers this owner observes. Mutable
regular-file backing can have unobserved external writers and remains on checked
single-instruction entries/resumes into retained bodies. This is a qualification
limit, not an external-file coherence guarantee.

Demand fetch reserves its issue before possible guest faults. An already approved
first instruction executes before attention handling, preserving recursive
execute-then-check ordering; later native boundaries poll attention normally.
Rejected budget attempts remain issued but uncompleted, matching the interpreter.
Helpers materialize precise IP/oplen/decoded state, commit once on normal return
and preserve fault/unwind cleanup. Passive memory pins retain anonymous backing
and page tables without holding a mapping mutex over helpers.

## Guest CPU, counters and qualification

Helper selection derives from the selected software CPU dispatch closure.
`BlinkFullJitSelect` runs during translation; runtime invokes the retained typed
pointer. It never reenters `GetOp`, `JitlessDispatch` or `ExecuteInstruction`.
The selected source inventory is separate from runtime encoding/operand/fault
coverage. See the [CPU/signal audit](../artifacts/jit-implementation/full-jit-20261027-r1/cpu-signal-audit.md)
for inherited ISA/FP deviations. Complete routing does not repair those semantics.

Guest CPUID and optional host emitter capabilities remain separate. BaselineOnly
excludes optional native operations while retaining software guest semantics.
Guest #UD is Linux SIGILL with guest context recovery; host SIGILL is not used as
an instruction implementation. Signal frame/return preserves the audited guest
flags, selectors and MXCSR. Linux asynchronous native stack walking is unqualified.

Statistics distinguish inline completions, helper calls/completions, actual block
compilations, worker-hint misses, explicit shared-index queries, token resumes,
resident/peak blocks, whole-flush/content retirements,
publication/emitted bytes and charged resources. `UniqueBlockKeys` is the current
retained directory count, not a lifetime distinct-PC claim. Arena diagnostics
separate useful/aligned code, payload, descriptors, indices, fixed owner charge,
reservation slack and cumulative emission. `BudgetEvictions` is a compatibility
count of bodies discarded by whole flushes; `TotalFlushes` counts reset events.
Managed allocation
windows separate owner setup, worker compilation, publication, helper/host-service
work and caller diagnostics; they do not count guest malloc or every host-runtime
allocation. Native reservations are a separate resource. Zero warm lookup or
binding allocation never means the whole application is allocation-free.

Retained failures and prior cache8 results remain historical evidence. New source
requires fresh consumers, differential/native controls, signal and lifetime tests,
and matched serial service CPU/wall observations. Finite tests do not prove every
encoding, alias schedule or inherited software CPU behavior.

The current default threaded project requires the separately retained exact
[FullCore source archive](../src/Managed.Emulation.FullCore/README.md). Generated
bulk is excluded from the checkpoint commit. The workspace has that input
installed; a fresh checkout must restore it explicitly before building.

At the c179 checkpoint, Linux CoreCLR/NativeAOT family and broad gates pass.
The completed four-cell service pilot still fails the performance goal and has
only one matched pair per cell. Full remains opt-in, Windows execution is
unverified, asynchronous unwind is unqualified, and R101-04 cleanup remains open.
Development is paused until the user resumes. See the checkpoint for current
coverage limits, CPU/wall/allocation results, preserved failures and resume commands.

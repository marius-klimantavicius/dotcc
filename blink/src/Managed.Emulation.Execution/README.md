# C# execution owner

This separate assembly references `generated/TranslatedBlink` and calls its
translated upstream loader, machine, interpreter and cleanup functions directly.
The translated library contains the narrow `HostGuestSignalsBridge` callback,
not this owner or a C execution frontend.

Create `GuestExecution(io, stop)` with a private `InstanceIo` image and a
`HostExecutionStop`, then call synchronous
`Run(imagePath, argv, env, instructionBudget)` on the dedicated owning thread.
The caller keeps IO and stop alive until `Run` completes. No other thread may
access translated machine pointers. Other threads may request cancellation and
observe `InstructionsCompleted` or `IsSleeping`; these expose cached progress
and the bound host sleep state, not guest memory.

The default profile still requires a static image. The optional
`allowInterpreter: true` argument permits upstream PT_INTERP loading from the
same explicit private filesystem for P5 compatibility qualification. The caller
must supply the actual interpreter/library closure. This does not imply a
native-host library fallback or a qualified dynamic/runtime-threading profile.

`Run` binds its host contexts, initializes the upstream system, applies resource
limits, loads the valid static guest executable, installs standard descriptors,
and executes instructions with the upstream attention/signal ordering. The
exception boundary uses the same public numeric jump-buffer identity and virtual
signal mask adapter as translated `sigsetjmp`. Unhandled guest signals unwind
through a private owner exception. Unrelated runtime or host exceptions propagate.
After the owning thread has completed, `LastFailureState` provides an immutable
scalar snapshot captured before exception cleanup: IP, RAX, the six Linux x64
argument registers and host errno. These are current registers, not a syscall
entry trace or a synthetic execution result. Register reads use upstream's
register-only ModRM accessor, avoiding generated anonymous-field names and
hard-coded machine offsets.

An optional `syscallTrace` callback receives scalar SYSCALL observations on the
owning thread. Diagnostic mode uses upstream's no-fault debugger decoder before
normal instruction dispatch and preserves host errno across that inspection.
It observes Linux x64 SYSCALL instructions, not other trap encodings or recursive
internal syscall calls. ReturnValue is null if dispatch unwinds, including a
guest exit; no return value is invented. The callback must stay bounded and must
not access translated state. Callback failures propagate. Default execution
does no extra decoding; tracing changes diagnostic overhead and is not a
performance measurement mode.

Normal cancellation must leave syscall depth, syscall flag, temporary allocations
and active page locks clean before frontend cleanup; failure is reported. Guest
halts follow upstream reset/collection. A returned result captures stop reason
before cleanup, so a later deadline does not relabel a completed guest exit.
The instruction count includes only calls to `ExecuteInstruction` that returned;
an exiting or faulting instruction can unwind before incrementing that count.

Machine pointers are cleared before freeing. Normal guest exit runs registered
exit callbacks while host services remain bound. Independent cleanup stages
release memory even if destruction or a callback throws. Retained mapping counts
and charged bytes are sampled before final release; `MemoryReleased` records
that release returned. No translated function runs afterward.

The current profile retains upstream static caches and supports one execution
per process. `Run` enforces this, including after failure. The process must then
be discarded. Restart, multiple instances, a controller protocol and cross-thread
machine inspection are outside this API's scope. Dynamic guest execution is
under separate P5 qualification.

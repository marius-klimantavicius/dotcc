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

`Run` binds its host contexts, initializes the upstream system, applies resource
limits, loads the valid static guest executable, installs standard descriptors,
and executes instructions with the upstream attention/signal ordering. The
exception boundary uses the same public numeric jump-buffer identity and virtual
signal mask adapter as translated `sigsetjmp`. Unhandled guest signals unwind
through a private owner exception. Unrelated runtime or host exceptions propagate.

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
be discarded. Restart, multiple instances, a controller protocol, dynamic guest
interpreters, and cross-thread machine inspection are outside this API's scope.

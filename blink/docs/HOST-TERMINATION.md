# Unexpected host termination boundary

Normal guest exit uses upstream `System.trapexit`, which records guest status
and unwinds before a host exit call. The opt-in host-termination header separately
redirects exit, _Exit, _exit and abort into authored callbacks that throw a typed
`HostTerminationException`. Reaching this boundary is an owning-worker failure,
not a normal guest exit or permission to terminate the controller process.

The exception preserves the requested raw status and distinguishes exit,
immediate exit and abort. It contains no pointer into C storage and invokes no
process termination API. The worker must catch it at its outermost boundary,
report failure, cancel outstanding I/O and discard complete upstream state;
normal C cleanup is not presumed to have completed.

`tests/HostTermination/run.py` checks native direct and C function-pointer calls
in separate subprocesses (including actual SIGABRT), then raw/optimized JIT/AOT
consumers. Native operations do not return and retain the expected exit kind/
status; translated calls throw the corresponding typed failure while managed
controllers remain alive. Two threads test independent statuses and compacting
GC. All modes pass. This qualifies generated managed C function pointers; it
does not authorize exceptions crossing arbitrary native ABI callbacks.

The initial generated consumer exposed a generic address-thunk owner defect:
unresolved callbacks were forced into Libc even when direct calls resolved an
authored partial-class method. The compiler now uses lexical method resolution
inside the owning translated class, preserving runtime fallback and authored
shadowing. Native/runtime pointer regressions and the full repository suite pass.
No generated C# was hand-edited. Full-core binding and worker lifecycle remain
separate pending gates.

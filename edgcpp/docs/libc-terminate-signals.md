# Default C++ termination and source abort

The default terminate handler must call abort. The selected synchronous signal
provider already owns abort and its SIGABRT dispatch; bypassing it with a direct
managed FailFast skipped source handlers. The backend now resolves the exact
live external source `void abort(void)` definition and calls it from its default
terminate helper. An absent source provider keeps the prior fatal fallback.
Malformed, ambiguous or unavailable source definitions are rejected explicitly.

This follows the original EDG runtime path:
`upstream/lib_src/eh_util.c::__default_terminate` calls `__abort_execution`, whose
implementation in `error.c` ends in abort. EDG source/lowering is unchanged.
The private `__edg_cxx_terminate_default` hook uses the same helper, so invoking
`std::get_terminate()` directly or restoring the previously returned default
handler preserves signal-aware behavior.

[C++17's terminate-handler contract](https://timsong-cpp.github.io/cppwp/n4659/exception.terminate)
requires the default handler to call abort and requires any configured handler
to terminate without returning. The existing internal fatal guards for returning,
throwing and recursive custom handlers remain unchanged. This patch does not
reinterpret every internal runtime failure as a public abort call.

For runtime-initiated termination, the active native/managed exception frame is
marked caught before choosing either the default or custom handler. This
preserves current_exception and the existing uncaught-count mask. Synthetic
checks observe these in an ordinary custom handler before invoking its saved
default. A separate current_exception observation inside the synchronous SIGABRT
callback is recorded as implementation behavior: that standard-library call is
not C++17 signal-safe and is not an asynchronous-signal conformance claim.

## Transfer boundary

A source C abort handler may leave via longjmp, as accepted in the standalone
signal component. Such a transfer cannot escape a C++ terminate handler under
its required termination contract. The generated default helper therefore uses
a CLR first-pass exception filter around source abort. Foreign CLR exceptions,
source longjmp and C11 thread-exit transfers cause fatal termination during the
first pass, before automatic destructors, managed finally blocks or worker exit
cleanup can run. No new ambient process or signal state is introduced.

A deliberately hostile authored abort provider exercises this containment.
It is an implementation-boundary test, not a claim that throwing from a user
signal handler or redefining abort with incompatible behavior is valid ISO C++.
The source has no throw types, which also exposed an empty generated type switch.
Adding a default break arm keeps that C# switch valid while retaining its
existing unknown-type FailFast path.

## Evidence

All evidence remains beneath `artifacts/campaign/`:

* `libc-terminate-signals-candidate-20261003-061000` preserves the original and
  drafted backend helpers.
* `libc-terminate-signals-20261003-061700/results.json` has six focused checks:
  native C++, original EDG C/runtime, static/instance JIT/NativeAOT. Each generated
  mode exercises nine default/direct/restored/current-exception/owner/fatal cases.
* `libc-terminate-abort-boundary-20261003-061700` retains the no-source-throw C#
  empty-switch failure and successful source-ABI rejection diagnostics. The
  corrected boundary matrix passes seven checks at
  `libc-terminate-abort-boundary-20261003-062000/results.json`: three exact source
  ABI rejections and static/instance JIT/NativeAOT first-pass containment of
  foreign exceptions, longjmp and worker thrd_exit. All managed finally/source
  destructor/worker cleanup markers remain absent before termination.
* `libc-terminate-regression-20261003-062000/results.json` passes 78 existing
  termination ordering/handler/owner/global/main/thread/array/shutdown checks,
  including their original EDG C references and JIT/NativeAOT paths.

The total is 91 focused checks (6 + 7 + 78).
* Initial `061500` failures retain fixture setup problems: the original C oracle
  needed header-free C signal declarations and the hostile abort declaration
  needed the existing noreturn annotation to agree with its secondary TU.

External OS signal delivery and POSIX masks/actions remain outside this change.
Current execution evidence is Linux x64; the implementation uses portable CLR
control flow and existing source dispatch, but other hosts still need execution
coverage.

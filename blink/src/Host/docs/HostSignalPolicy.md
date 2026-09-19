# Private asynchronous-signal policy

The selected single-worker profile has no asynchronous signal-delivery source.
It cannot arm process interval timers, send signals, or suspend for a signal.
These operations fail with EOPNOTSUPP; they never invoke the controller's signal,
timer, or process APIs. This policy is distinct from the real synchronous
guest-fault/nonlocal-unwind mechanism and private signal-action/mask storage.

The three interval timers have the real invariant that they remain disarmed.
`getitimer` reports zero records, and `setitimer` accepts only the all-zero
idempotent disarm request. Invalid selectors/time fields fail before touching
outputs. Even an inactive nonzero interval is outside this restricted policy.
Input/output aliasing is preserved. `alarm(0)` returns the actual zero remaining
time; a positive request returns UINT_MAX with EOPNOTSUPP, an explicit private
extension surfaced as -1 by upstream's signed `SysAlarm` result. Native alarm
has no error convention, so this is not claimed as a general POSIX replacement.

`kill(pid,0)` checks the bound private identity or its sole group (0/-pid).
Foreign targets fail ESRCH. The broad -1 broadcast selector is outside this
namespace and fails ESRCH. Positive signals to the private target fail
EOPNOTSUPP; invalid signal numbers fail EINVAL. This does not enqueue guest
signals. `pause` and `sigsuspend` fail without waiting or changing the stored mask.
Null sigsuspend masks fail EFAULT. Missing identity bindings fail ENODEV.

Supporting asynchronous guest signal delivery requires a future explicit queue,
worker wakeup, signal disposition, and budget/stop contract. The service profile
does not require these timer or signal-wait syscalls. Refusals are observable
profile behavior, not successful placeholder operations.

# Threaded .NET guest diagnostic

The first actual raw-JIT diagnostic completed with a failed guest result in
`artifacts/dotnet-threaded-guest-execution/attempt-2fpjx0g2/receipt.json`
(SHA256 `419e2d6a7da05a65ef9692e6217f5384c477c5192238c7fb40cbd2a21f8e047b`).
The threaded layout and ordinary thread-lifecycle gates had passed before this
execution. Preparation alone is never a passing guest result.

The consumer calls the separate authored `ThreadedGuestExecution` owner against
an explicit passed `build-threaded-delivery.py` receipt. It does not regenerate,
postprocess, patch, or replace the product. The runner verifies the 108-object
assembly/profile/compiler chain, raw and optimized delivery manifests, and
original Host/bridge sources before and after use. It privately copies the raw
library sources, Host/bridges and current separate owner for this diagnostic;
the owner identity is recorded independently of the delivery producer identity.
The optimized delivery is identity-checked but is not executed by this initial
runner. Only an explicit `--run` builds and executes raw JIT.

```sh
python3 -B blink/tests/DotNetThreadedGuestExecution/run.py \
  --translation-receipt <passed-threaded-delivery-receipt> \
  --guest-receipt blink/artifacts/dotnet-guest-musl/attempt-8za50rji/receipt.json \
  --native-gc-receipt blink/artifacts/dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json
```

Add `--run` only after review and release of the serial execution slot. A failed
actual guest returns nonzero, with the outcome preserved in the attempt receipt.
Each subprocess has an isolated temporary directory and process group, bounded
wait, and SIGTERM/SIGKILL cleanup on interruption or timeout.

The only mounted image is the genuine static .NET NativeAOT service from the
pinned musl receipt (ELF SHA256
`b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3`).
The runner independently checks absence of ELF INTERP and NEEDED entries. It
uses the exact native small-GC witness environment:

```text
LANG=C
DOTNET_GCHeapHardLimit=1000000
DOTNET_GCRegionRange=2000000
DOTNET_GCRegionSize=100000
```

These strings are passed to the guest, not assigned to the emulator host's
runtime environment. No further tuning or native-emulator fallback is used.
The baseline remains 20 million total guest instructions, a 30-second owner
deadline, and the owner's 64 MiB shared backing limit. These are diagnostic
bounds, not a claim of sufficient resources for every .NET workload.

Readiness requires exact `READY 8080\n` output and an actually published private
listener. The two HTTP requests and entire responses are copied from the pinned
native small-GC witness and compared byte-for-byte. Ordinary completion requires
exit zero, no owner stop, exact final stdout, empty stderr, every Machine
released, all workers joined, shared memory released, and `IsQuiescent`.

The owner runs on a dedicated thread. Its syscall observer copies scalar records
under a lock into at most 16 guest-thread slots, each with 4096 retained rows;
observed counts and truncation are reported explicitly. This preserves each
thread's observation order, not a reconstructed global syscall ordering. A null
return means that the syscall unwound before the owner could observe a return.
The JSON report uses explicit `Utf8JsonWriter` calls and records exceptions,
thread results when available, join/quiescence state, captures and HTTP outcomes.
A failed live-owner case can report a lock-protected partial trace, marked
incomplete; it never reads live Machine memory from the harness.

IO and stop owners are disposed only after the owning thread has joined and
`IsQuiescent` is true. If workers remain live, resources stay intact for process
discard and the result fails. No execution owner is reused or translated method invoked
after disposal. No AOT-host, optimized execution, or full threaded-runtime
qualification is implied by this initial raw-JIT diagnostic.

## Observed first diagnostic

The private consumer build succeeded; the raw-JIT process returned 1 because
the guest exhausted exactly 20,000,000 instructions before readiness. No HTTP
request ran; stdout and stderr were empty. The two guest threads had distinct
IDs 1 and 262144 and completed 19,998,387 and 1,613 instructions respectively.
Both Machines were released, all workers joined, shared memory was released,
and `IsQuiescent` was true. No execution exception or stop-notification failure
was reported. This demonstrates progress beyond clone, not a running .NET HTTP
service.

The retained trace shows successful membarrier query/registration, stack
mmap/mprotect, clone with flags `0x7d0f00`, and a `0x2001000`-byte GC reservation.
Later main-thread rows repeatedly call mremap (syscall 25) with oldsize 4096,
newsize 8192 and flags 0, returning -12 (ENOMEM) while scanning descending
page addresses. This is observed syscall behavior, not yet a diagnosed adapter
or upstream defect. The main thread observed 440,030 syscalls; only its first
4096 rows are retained and marked truncated. The child's seven rows include a
futex wait returning EINTR during the budget stop. Final thread IPs and all
retained scalar rows are in `result/result.json`.

The runner independently reverified 716 frozen inputs, 104 prepared files,
11 consumer binaries and 33 artifact hashes. Its frozen SHA256 was
`70c49757cd123ad2bdb87d51d467053e17eeb790e5603dd09b9730a12c471374`.
The earlier `attempt-uzblk6s5` stopped before any build because the private Host
copy omitted non-code entries present in the exact delivery manifest. Its
receipt is preserved; the successful preparation copied every verified Host
entry without weakening the closure guard. Build-server reuse is disabled and
recorded for the actual consumer build. No guest/product implementation or GC
settings were changed in response to the failed runtime result.

## Post-mremap validation diagnostic

The unchanged runner, guest, native GC environment and bounds were run against
mremap-validation delivery `artifacts/threaded-delivery/attempt-wwrpenct/receipt.json`.
That assembly reused 107 exact producer objects and changed only the staged
syscall object. The actual diagnostic remains a failed guest result:
`artifacts/dotnet-threaded-guest-execution/attempt-n9cjykot/receipt.json`, SHA256
`270edb094faf8b72fc9858b0bf0f86bdd2c781d198cca19a4853c7998c878a37`.

The stack-discovery scan now terminates after 2048 mremap calls: 2047 return
ENOMEM, followed by EFAULT for the absent page at `0x4fffff7ff000`. This is the
narrow source-validation behavior; successful resizing or relocation is still
not implemented or claimed.

The next observed missing capability is `epoll_create1` (syscall 291), with
`EPOLL_CLOEXEC` (`0x80000`), returning ENOSYS. The guest prints a genuine .NET
`SocketAsyncEngine` initializer exception naming ENOSYS, then raises SIGABRT
(signal 6). It stops after 4,388,440 instructions, before readiness or HTTP;
stdout is empty. The main and child traces contain 2159 and 7 observations,
respectively, with no truncation. The child returns EINTR from its futex wait
during cooperative group shutdown. Both workers join, both Machines and shared
memory are released, and the owner reports quiescence without a CLR execution
or stop-notification exception. The external stop reason remains None.

Independent verification rechecked 723 frozen inputs, 104 prepared files,
11 binaries and 33 artifacts. The consumer build returned 0 and the actual
raw-JIT diagnostic returned 1. No extra tuning, retry after the guest failure,
or epoll implementation was included in this attempt.

## Empty-epoll delivery diagnostic

The unchanged guest, runner, four environment entries and bounds were run against
`artifacts/threaded-delivery/attempt-jttvocmh/receipt.json` (108 fresh producers,
zero reused objects). The guest now reaches `READY 8080\n`, publishes its listener,
and accepts the normal HTTP connection. This is the first observed readiness;
the HTTP service check still fails.

Receipt `artifacts/dotnet-threaded-guest-execution/attempt-4nsdxnrz/receipt.json`
has SHA256 `cf15e01a693614a61d1927783171b7336230c07dc6bdec0df297eee1bf9226b3`.
Its result SHA256 is
`c1ab0945c167e86e64279477ca46a3130a4e3505392b2674fed4aee3872cf560`.
The actual next failure is `setsockopt(8, SOL_SOCKET, SO_SNDTIMEO_OLD, ..., 16)`
returning -92 (ENOPROTOOPT) on the accepted socket. The .NET fixture catches the
socket exception, prints `SocketException: Protocol not available\n`, and exits
its group with status 1. The client observes a connection reset and no HTTP
response case passes. No timeout implementation or extra tuning was attempted.

`epoll_create1(EPOLL_CLOEXEC)` successfully returned fd 3. A real child issued
`epoll_pwait(3, ..., 1024, -1, NULL, 8)` and returned EINTR during cooperative
shutdown. All four workers joined, all Machines and shared memory were released,
and `IsQuiescent` was true. No execution or notification exception occurred.
The owner latched execution StopReason None at the genuine group exit; the outer
harness later requested stop while handling the failed HTTP exchange. These are
distinct observations, not a deadline or budget exit.

The guest completed 1,249,598 instructions. Its four complete syscall traces
contain 2201, 7, 6 and 3 rows, with no truncation or unrecorded thread observations.
Independent verification checked 733 frozen inputs, 106 prepared files,
11 binaries and 33 artifacts (883 identities). The build returned 0; the actual
raw-JIT diagnostic and runner returned 1 with `guest_passed=false`.

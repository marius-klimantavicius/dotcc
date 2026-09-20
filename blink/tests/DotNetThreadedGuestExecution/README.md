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

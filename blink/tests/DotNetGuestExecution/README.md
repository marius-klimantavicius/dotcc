# Valid .NET guest startup diagnostic

This consumer uses the same authored `GuestExecution` C# owner as the qualified
C guest. An explicit static or dynamic configuration selects the private image
and whether an ELF interpreter is permitted. It records the actual startup/runtime result. It does
not call a native emulator. No translated run has been qualified by merely
preparing these files.

Preparation performs identity checks and read-only ELF inspection; no build or
guest execution happens without `--run`:

```sh
python3 blink/tests/DotNetGuestExecution/run.py \
  --image-mode dynamic \
  --translation-receipt blink/artifacts/translation/attempt-wd_77mip/receipt.json \
  --guest-receipt blink/artifacts/dotnet-guest/attempt-5130ydrk/receipt.json
```

The separately native-qualified static musl guest uses:

```sh
python3 blink/tests/DotNetGuestExecution/run.py \
  --image-mode static \
  --translation-receipt blink/artifacts/translation/attempt-wd_77mip/receipt.json \
  --guest-receipt blink/artifacts/dotnet-guest-musl/attempt-8za50rji/receipt.json
```

Static mode requires the passing receipt's `static_elf_verified` flag and fresh
`readelf -lW`/`-dW` checks showing no `INTERP` or `DT_NEEDED`. It mounts **only**
the exact guest ELF, sets only `LANG=C`, and calls the owner with
`allowInterpreter: false`. No loader/library paths or GC environment tuning are
added. The musl receipt identifies its separate SDK/runtime/container toolchain;
it is not the ordinary glibc guest binary or emulator host NativeAOT output.
Both modes serialize the mode, interpreter permission, executable path, argv
and environment in `image/configuration.json`; the consumer validates this
configuration instead of guessing from filenames.

The dynamic image contains the exact native-qualified .NET service at
`/bin/dotnet-service`, plus separately hashed private copies of
`/lib64/ld-linux-x86-64.so.2`, `/lib/x86_64-linux-gnu/libc.so.6` and
`/lib/x86_64-linux-gnu/libm.so.6`. Every transitive `DT_NEEDED` name must belong
to that reviewed closure. The loader/library paths were observed in the native
trace; their bytes are identified when the diagnostic snapshot is prepared.
`LD_LIBRARY_PATH=/lib/x86_64-linux-gnu:/lib64` supplies the ordinary private
library search path without requiring a host `ld.so.cache` or a host-filesystem
mount. No procfs, sysfs, cgroup tree or `/dev/urandom` file is mounted. Native
probing of those paths is recorded; whether a particular probe requires a
successful result remains a question for the actual diagnostic.

Use a passing delivery that includes all bindings required by the current C#
owner. The runner records that delivery's producer compiler identity and copies
its immutable raw source and frozen Host/bridge closure. The current C# owner
is copied and identified separately. The runner does not rebuild the compiler,
re-emit C, postprocess or mutate the product. Historical runs below used delivery
`4yjaed1_`; that frozen closure predates the new membarrier owner binding and is
no longer compatible with the current owner. Their archived owner copies and
receipts remain evidence for those historical runs only.

Baseline limits remain 64 MiB of owned backing memory/guest address-space
policy, 20 million completed dispatches and a 30-second owner deadline. Runtime
GC configuration is not altered to make the guest fit. These are diagnostic
limits, not an assertion that they are an adequate NativeAOT execution profile.
Only the interpreter thread accesses guest machine state. If it fails to join,
the fixture leaves borrowed owners alive for failed process discard.

If real readiness is reached, the harness publishes the actual private listener
and compares the two original native HTTP requests/responses byte for byte.
Otherwise it records the first result or exception, available completed count,
IP/halt/signal/status, stdout/stderr and cleanup result. If the owner throws
before returning its result, unavailable machine fields remain absent rather
than fabricated. A recorded failure is never marked `guest_passed`; preparation
alone also leaves `passed` false. No musl, guest threading, broader lifecycle or
P5 completion claim follows from this diagnostic.

The first actual raw JIT diagnostic completed with **guest startup failure**:
`artifacts/dotnet-guest-execution/attempt-twmyfw31/receipt.json`, SHA-256
`7ac6b0e12d77ba9ca2ca3f3a8421f6bed0871c99a4eeabb59eaf87c7a1987a5d`.
The build succeeded with 18 historical generated-source warnings and no errors.
After 18,791 completed dispatches, `ReserveVirtual` received a null return from
`AllocateBig` in its per-page mapping path and invoked `PanicDueToMmap`, which
raised `HostTerminationException` with exit request 250. The underlying mapping
request/errno was not captured, so this evidence alone does not determine the
allocation failure's cause. No readiness or HTTP cases were reached; captured
stdout/stderr were empty, the owner stop reason was None and the thread joined.
The owner rethrew after cleanup, so its normal result, IP and memory-release
fields are unavailable. `guest_passed` and `passed` are false. The Python runner
returned zero because it successfully recorded the diagnostic; callers must
inspect these explicit receipt fields instead of treating that exit as a guest
pass. No second execution or implementation change was made at that checkpoint.

For the next reviewed diagnostic, the owner exposes an immutable
`LastFailureState` copied before cleanup. The report writes its IP,
accumulator, six argument registers and current host errno explicitly. This is
state at the managed exception boundary, not an entry-syscall trace; intervening
code may have changed registers or errno. It does not retrospectively supply
missing fields to the preserved first receipt.

The second diagnostic also failed at 18,791 completed dispatches, with no
readiness or HTTP cases:
`artifacts/dotnet-guest-execution/attempt-046ir0z5/receipt.json`, SHA-256
`eee2ab47b268fa9fd83582660eb32dc28c2c71a2a3e46e6d3124dcfe50b9faa7`.
The pre-cleanup snapshot recorded IP `0x110000025d2c`, accumulator `9`, and
arguments `(0, 2170256, 1, 2050, 3, 0)`: Linux x64 `mmap(NULL, 0x211d90,
PROT_READ, MAP_PRIVATE | MAP_DENYWRITE, 3, 0)`. This matches the native loader's
initial libc mapping. The private libc file is 2,125,328 bytes (`0x206e10`);
the requested span includes 11 whole pages beyond rounded EOF `0x207000`,
before the loader's later segment/BSS replacements. That is consistent with
the adapter's beyond-EOF policy being the cause, but the adapter's actual
per-page rejection reason has not been directly captured.

The snapshot's host errno was **9 (EBADF)**. It was sampled after
`PanicDueToMmap` attempted `WriteErrorString`, so it cannot be presented as the
original mmap errno. The thread joined, stop reason remained None, and captures
were empty. Neither receipt is a guest pass.

Both historical runs used a runner that returned zero after recording a failed
diagnostic. Its exact source is preserved beside the second receipt. The
current runner now returns nonzero for `--run` when `guest_passed` is false,
after persisting the receipt; preparation still returns zero while leaving
`passed` false. This exit-code-only correction was checked without repeating
guest execution.

The first static musl diagnostic also failed before readiness:
`artifacts/dotnet-guest-execution/attempt-_772un12/receipt.json`, SHA-256
`009f215fd3c969da4022da503f0ad4aae43ea68e0a9b5e4b6a9a19fc3e72a3b9`.
It completed 139,230 dispatches and exited through the normal exit trap
(`halt=-10`) with guest status `-1`, IP `0x472a1c`, and no signal or owner stop.
No managed execution exception was raised. Stdout/stderr were empty and no
HTTP cases ran. The owning thread joined; 270,450 bytes/two mappings remained
before final release, which completed. Both receipt pass fields and the runner
exit status report failure. This result does not identify the startup cause.

The next source revision enables the owner's optional scalar syscall observer.
It stores at most 4096 records, reports the total observed count and truncation,
and writes explicit JSON fields for entry IP, syscall number, six arguments and
the optional return value. A null return means the observed syscall unwound;
register values are kept unsigned without interpreting a particular return as
the cause of failure. The observer does not stop execution when the list fills.
The report reads the trace only after the owning thread has joined; otherwise
it marks it unavailable. Earlier receipts remain unchanged and have no trace.

The first current-compiler delivery diagnostic uses `translation/attempt-wd_77mip`
and passes the real membarrier query/registration. It then reaches clone56 with
flags0x7d0f00, receives ENOSYS from the disabled-thread profile and exits -1
after151,283 instructions. `attempt-xyn4u0iw/receipt.json` SHA-256
`3c02bc464ff04ad6fa9ac6d95e2e1fbb270063ba22cbdabc0e90613010e2502f`
preserves40 untruncated observations, joined cleanup and released memory. No
HTTP case ran; this remains a failed guest diagnostic. Build was warning-free.

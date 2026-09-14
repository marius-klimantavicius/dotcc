# Clock and entropy callbacks

`Managed.Emulation.Host.HostEnvironment` owns a clock provider and an entropy
provider per instance. Production defaults are the BCL `TimeProvider.System`
and `RandomNumberGenerator.Fill`. Realtime is Unix time; monotonic time measures
elapsed time from instance construction. Both return normalized seconds and
nanoseconds. Deterministic providers are injected only by the test consumer.

The authored partial `Blink` bridge compiles alongside unchanged generated C#
and binds the environment explicitly on a dedicated C worker thread. No managed
reference resides in translated C storage. Calls before binding return ENODEV;
provider exceptions become errno failures. A null output pointer returns EFAULT.
Pointer buffers here are trusted host storage supplied by translated C; guest
address validation remains the upstream syscall boundary's responsibility and
is not yet qualified by this focused test.

Entropy supports flags 0, GRND_NONBLOCK and GRND_RANDOM through the secure BCL
provider. Requests return at most 256 bytes, preserving legitimate short reads.
`getentropy` completes requests up to 256 bytes and rejects larger requests with
EIO. Unknown flags return EINVAL. There is no deterministic production fallback.

`python3 blink/tests/HostEnvironment/run.py` compares native C behavior with
authored callbacks reached through the same translated C probe in raw/optimized
JIT/NativeAOT. The separate consumer also checks exact injected clock values,
monotonic elapsed time, deterministic entropy bytes, short requests, provider
failures and unbound operation errors. The generated library is fully rooted
for AOT, raw sources are retained unchanged, and receipts hash the compiler,
profile, generated sources, host sources and executable. All four modes pass.

These callbacks are not yet selected by the full core profile. The clock helper
header and random header make the intended C ABI explicit; integrating them
requires preserving the qualified bridge and selecting only corresponding
capability macros. Timer signals, sleep scheduling, guest CPU clocks and actual
service startup remain separate work.

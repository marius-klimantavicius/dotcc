# Socket and guest record include order

`python3 blink/tests/HeaderOrder/run.py` compares native Linux headers with the
authored profile, then emits three independent C objects and links them into a
managed library. Raw/optimized JIT and rooted NativeAOT executions must match
the native values, sizes, alignment, and actual field placement.

The two source files include `sys/socket.h` and untouched upstream `blink/linux.h`
in opposite orders. The old `#define linger blink_host_linger` changed
`linger_linux.linger` only in one declaration order, making otherwise identical
objects incompatible. The authored socket header now declares the standard
`struct linger` tag directly. Its native layout remains 8 bytes, alignment 4,
with fields at offsets 0 and 4; the guest byte-record remains 8 bytes, alignment
1, with the same field offsets and original field spelling.

The complete matrix passes in `artifacts/header-order/attempt-017h8krz/receipt.json`.
This is a header/type and object-link regression, not a guest CPU execution test.

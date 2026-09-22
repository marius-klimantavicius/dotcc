# Coarse monotonic host clock

Native and all four managed modes pass; see the observed receipt below. Run `python3 blink/tests/HostCoarseClock/run.py` from the repository root.

This fixture compares successful normal Linux `CLOCK_MONOTONIC_COARSE` calls with the authored host callbacks in raw/optimized JIT and NativeAOT. It checks 64 nondecreasing, normalized live readings, positive normalized resolution, valid optional resolution output, adjacent guards, preserved errno and single evaluation of call arguments. Native and managed origins and output quantization differ, so their absolute timestamps are not compared.

Each managed execution additionally checks 13 controlled-provider rows: a nonzero shared monotonic origin; exact readings immediately around millisecond and second boundaries; unchanged fine monotonic and realtime readings; and valid slow provider frequencies. Coarse time floors the same owner's elapsed time to a millisecond. Its reported precision is the larger of one millisecond and the provider timestamp period rounded up to nanoseconds; this describes software/provider limits, not accuracy. Timestamp invocation counts prove one provider read per time request.

The runner freezes and rechecks authored/copied host sources, profile headers, compiler/postprocessor identities and raw generated sources. It retains command output, separate JIT execution copies, executable identities and a receipt for success or first failure. There are no invalid-access, unsupported-clock or provider-failure rows. Full upstream `XlatClock` dispatch and real Kestrel behavior require the separate regenerated-product qualification; this fixture alone does not establish them.

## Observed focused qualification

Native and raw/optimized JIT/NativeAOT pass at
`../../artifacts/host-coarse-clock/attempt-ny75u0ux/receipt.json`
(SHA-256 `814a144c582b76521d9b429def663464a66afc1a11c66ac436eeddd3400e3841`).
All 15 commands exit zero. Each managed mode checks 64 valid live calls and
13 exact controlled-provider rows. Independent review rechecks 260 recorded
source/tool/log/generated/binary identities. This proves the host clock boundary;
fresh product generation and actual Kestrel clock-6 dispatch remain separate.

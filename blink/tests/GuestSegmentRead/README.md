# Ordinary user segment reads

CoreCLR's ordinary context-capture routine reads CS and SS with opcode `8C`.
The portable Blink profile previously mapped that opcode to `OpUd` whenever
metal mode was disabled. The authored CPU adapter now reads the guest's existing
visible selectors in long user mode; the ELF loader already initializes CS and
SS. Segment loads, privileged operations and metal-mode support are unchanged.

The valid static Linux fixture checks all six selectors, 16/32/64-bit register
destinations, exact two-byte memory stores (including REX.W), adjacent bytes and
a store across two mapped pages. Linux is the native oracle. No host selector
query, malformed executable or fault injection is used.

```sh
python3 -B blink/tests/GuestSegmentRead/run.py
```

The attempt receipt retains the fixture source, linker input, tool/ELF identities,
stdout and process-group cleanup. It is native evidence only. Supply that
receipt as `--segment-read-receipt` to
`blink/tests/GuestFutexBitset/run-managed.py` with a fresh producer for public
raw/processed, JIT/NativeAOT-host and both-mode execution. Guest code always runs
through Blink's interpreter.

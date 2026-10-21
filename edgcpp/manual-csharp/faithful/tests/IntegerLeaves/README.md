# Integer leaf tests — authored, execution pending

The supervisor owns all builds, runs and native preparation after shared writers
HOLD. This child did not execute these tests. `Program.cs` provides a console
entry point compatible with the supervisor-owned project manifest.

With no arguments, the runner checks the source-derived 16-byte/two-byte-aligned
inline part layout, boolean/host widths, value-copy independence, signed and
unsigned host setters, both aggregate setter overloads, comparison signedness
pairs and every first-different part, host conversion/truncated result and error
outputs, OR/AND/XOR/complement, increment and signed-shift macro defined counts.
Boundary vectors cover 16-bit part transitions, signed/unsigned 64-bit limits,
128-bit sign/wrap boundaries and alternating/asymmetric patterns. Test-only
UInt128/Int128 expectations do not participate in the production implementation.

Alias cases include destination/source at both distinct address orders, exact
reference self-copy, binary operand reversal and exact operand/result alias, and
comparison of one object under each signedness pair. Binary routines return the
result in operand 1; they have no independent third result address. Conversion
uses separate correctly typed host/error storage and verifies input preservation.
No claim is made for invalid, unpinned, expired or mis-typed pointers, or partially
overlapping objects lacking a proven valid native object-lifetime contract.

`--abi` prints the measurement fields used by `tools/integer_oracle.cpp --abi`.
`--stream` accepts one well-formed case per line:

```
op alias signed1 signed2 count A32hex B32hex
```

The operations and output match the supervisor-owned original-source oracle:
`set-signed`, `set-unsigned`, `copy-signed`, `copy-unsigned`, `extract`, `compare`,
`or`, `and`, `xor`, `complement`, `increment`, plus adapters calling the separately
owned arithmetic batch's `add`, `add-mixed`, `subtract`, `subtract-mixed`, `left`,
`right`, `mask`, `extend`, and `negate`. The stream contains only transport calls,
parsing and formatting; it does not implement arithmetic. A nonzero alias makes
Other point to A; otherwise Other points to B. Scalar setters obtain their input
from Other's low 64 bits before mutating A. Output is:

```
A32hex Other32hex error comparison host16hex alias
```

Hex output is lowercase with leading zeros. Host output preserves the signed
host result's bit pattern. The scalar signed setter clears the transport host
field again, matching the oracle. Both input and output pointer alias identity
remain observable. The supervisor supplies valid shift/mask/extension counts;
the adapter adds no clamp or source-domain repair. Frontend initialization and
the INIT-001 excessive-width finding remain outside this bounded leaf caller
domain. Malformed transport is a harness input error, not an EDG diagnostic case.

Required execution evidence remains open: independent original/C# source review,
held native/managed ABI comparison, stream differential vectors, JIT checks and
NativeAOT checks. The initial ABI contract review is source inspection only.

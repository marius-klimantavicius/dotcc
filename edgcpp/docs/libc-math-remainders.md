# Selected musl remainder and quotient functions

The six remainder/remquo functions at float, double and binary80 widths use
pinned musl1.2.6 algorithms. Public wrappers add EDOM for a zero divisor or
infinite dividend when neither operand is NaN. Successful finite operations,
infinite divisors and NaNs preserve errno. Original signed-zero, nearest-even
quotient selection and quotient-sign behavior are retained.

Legacy native drem/dremf weak aliases are omitted from this selected component;
they are not silently exported. Original files, zero-fuzz patches and the
portable ABI header are hashed. Dynamic rounding and floating exception flags
remain a separate implementation gate. Binary80 arithmetic performance also
remains open.

The native original/adapted matrix covers200 random pairs at each width and
extended-value cross products. It hashes exact result bits (normalizing NaN
classes) and quotient integers. Nine component checks include this oracle and
C/C++ static/two-owner JIT/NativeAOT, domain errno, forced GC and TLS isolation.
Eight automatic provider checks verify173 packaged source units, all six GNU
aliases, runtime function-pointer identity and actual calls. One source-replay
check reconstructs all seven pinned inputs. Evidence is indexed in
`evidence/milestone-math-remainders.json`; execution is Linux-x64 only.

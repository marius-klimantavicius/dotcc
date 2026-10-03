# Selected musl logarithms

`--libc musl` now packages log, log2 and log10 at float, double and binary80
widths. Eighteen translated source units include the original table and
polynomial algorithms. The source manifest pins all 23 original inputs and
exact adaptation patches; the generated ABI header is hashed as well.

Public wrappers set ERANGE for a zero pole and EDOM for negative arguments,
leaving successful calls, positive infinity and NaN errno unchanged. Two data
initializers are reordered to declaration order for C++ compatibility without
changing constants or layout. Dynamic rounding and floating exception flags
remain a separate fenv implementation gate. Binary80 arithmetic still uses the
existing allocating arithmetic implementation; allocation-free comparisons do
not make the whole logarithm algorithm allocation-free.

Nine component checks compare original/adapted native algorithms with C/C++
static/two-owner JIT and NativeAOT, covering random bit patterns, subnormals,
near-one inputs, maximal values, poles and domain errors. Eight automatic
provider checks verify all source hashes, packaging, direct GNU alias calls,
runtime function-pointer identity and errno. One replay check reconstructs all
adapted inputs from their originals. Evidence: milestone-math-logs.json.

The first packaging attempt used a direct source comparison of a C++ function
and its GNU builtin spelling. The unchanged original EDG C backend folds that
comparison to unequal before either output backend resolves symbols; this is
retained at artifacts/campaign/log-builtin-address-oracle-001. The accepted
fixture uses volatile function pointers to exercise runtime address resolution.
No EDG lowering was changed. Execution evidence is Linux-x64 only.

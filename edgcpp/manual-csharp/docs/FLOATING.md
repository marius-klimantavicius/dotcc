# Target floating values and frontend integration

The numerical foundation has independent isolated acceptance:131/131 JIT/AOT,
four matching125-file source/data/tool maps,15 owned and13 consulted-source/
configuration hashes, three output fingerprints. The actual combined frontend
gate is separate; its independently accepted final round3 executes164/164 JIT/AOT with
four current175-file maps,3artifacts and10native6accept/4reject matches. Final
review is `../evidence/review-floating-integration-final.md`. No
full M1/M3/M4 or language/product completion is inferred.

## Values and source correspondence

FloatingValues.cs and FloatingLiterals.cs are handwritten target binary32/64
algorithms. Immutable exact payloads, BigInteger significands/rationals and
quotient/remainder nearest-even rounding avoid host floating parsing/arithmetic.
Fixed independent bits cover halfway/sticky/carry/normal/subnormal, conversions,
nonfinite flags, arithmetic, resource limits and concurrent reuse. Integer
conversion truncates before range checks. Flags are outcomes, not automatically
source errors; exact subnormal Tiny differs from Underflow/Inexact.

FloatingSemantics.cs and ConstantFloating.cs adapt actual EDG literals.c
conv_float_literal, exprutil.c floating common-type selection, overload.c
standard sequence ranking and interpret.c conversions/arithmetic/compound-store
work. source-floating-integration.json fingerprints9 consulted files; the
foundation handoff records13 source/configuration inputs and precise omissions.
Explicit rational rounding replaces EDG approximate-first/intermediate storage
mechanisms while preserving the reviewed target invariants; no whole-file port
is claimed.

Canonical float/double identities retain32/64 target width and24/53 precision,
const qualification and recursively owner-aware compound caches. Actual literal
nodes expose immutable FloatingConstantValue Bits/Classification/IsNegative and
retained conditions. Integer ConstantValue remains absent for floating values.
Storage, reference binding and assignment operation types use SemanticType;
actual cells retain one identity/lifetime/readonly/origin/access model and a
separate floating payload. Assignment IntegerValue is nullable; the old Value
accessor throws for floating observations instead of fabricating integer data.

Float-to-double is Promotion, other mixed numerical changes Conversion. Actual
common operations precede destination conversion (integer3 *= float1.5 yields4).
Calls, by-value template deduction, references/materialization, array elements,
loads/stores/loops and cleanup retain typed values and earlier frozen candidate
identities. The selected pinned profile permits IEEE division-zero/invalid-operation Inf/NaN
results with retained flags; a separately selectable strict constant-result policy
rejects those exceptional outcomes. Both reject operation overflow and invalid
integer conversions. Arithmetic/conversion losing a nonzero finite result to
zero fails, while nonzero inexact subnormal results and directly decoded HEX literal
underflow-to-zero remain permitted. DECIMAL literal nonzero-to-zero underflow
is an out-of-range source error in the selected profile. Literal diagnostics
therefore distinguish decimal versus hexadecimal conversion branches.
Ordinary const floating globals
do not acquire const-integral constant-read privileges from optional values.
Optional numeric warnings inspect bounded closed arithmetic syntax only. A
per-parser immutable memo saturates at ConstantEvaluationStepLimit and then
abstains; references/calls/stores are skipped. This is not full warning parity.
Required evaluation still executes the original graph with its ordinary budgets.
FloatingIntermediateBitLimit is immutable64–65536/default32768, independently
of expression/call/work budgets; resource outcomes remain explicit.

## Boundaries, oracle exceptions and coupled proof

Decimal/hex literal grammar, separators and float/default-double suffixes are
implemented. Exponent-like valid UDL suffixes are recognized as Unsupported
literal-operator lookup, including decimal/hex integer ambiguity. Missing signed
exponents and malformed suffixes remain InvalidSource. The exact llL versus lL
split follows pinned scanner restoration; full user-defined literal semantics
remain required. Inactive/dropped numeric values remain lazy after preprocessing.

The source gate combines macros/includes, actual float/double template instances
and reuse, earlier const-reference helpers despite later void overload/caller
hiding, typed array/alias stores and loops, required3.5/2.5 results and payload/
identity/origin queries after disposal. Exact witness includes unchanged gate
and header and discriminates result types; numerical/global-read negatives are
separate. Passing foundation decoder tests alone never proves this path.

The pinned oracle's hexadecimal subnormal carry/exponent-reset defect has a
narrow root-authorized intentional exception in
../evidence/floating-subnormal-qualification.md: retain mathematically correct
nearest payloads, exact rational proof and positive native half-value witnesses.
Six incorrect design expectations and the subnormal fixture are excluded. The
unsupported native builtin bit-cast proposal is separately excluded; managed
signed-zero bits are not claimed as native bit-cast parity. All remaining
floating differences require independent qualification.

Long double80 and its16-byte target storage, extended formats, alternate rounding/
excess-precision modes, complete NaN/IEEE exception policies/builtins, full UDL/
cast/declarator/reference/class/template/dialect behavior remain required.
Parallel binary80 authoring uses new isolated modules; it is not production
long-double support until actual type/value/conversion/evaluator integration.

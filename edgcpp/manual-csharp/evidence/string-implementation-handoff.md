# String literal, array and storage source handoff

Handwritten main-checkout implementation against accepted Unicode6c3848ca5.
Implementation agent performed no build/test/native/Git execution. This is the
first integrated source hold, not accepted execution or full-plan completion.

## Source and representation

Read literals.c conv_string_literal_chars2204–2322, conv_string_literal2325–2525,
concat_string_literals2547–2747, conv_single_char1238ff and
conv_single_wide_char1743–1820; lexical.c concat_adjacent_string_literals15980ff;
il.c string_literal_type12779–12825; types.c array transformation6290ff;
interpret.c ck_string7081ff, array decay27927ff and subscript31730ff;
decl_inits.c check_string_constant_initializer_full101–223. Existing pinned
source hashes are retained in the Unicode handoff and source inventory.

StringLiteralExpression is a real const-element ArrayType lvalue, with immutable
typed CodeUnits including exactly one final zero, retained component Token /
origin / encoding / raw facts, and no fake integer ConstantValue. Decoding
preserves embedded zeros and target unit widths. Adjacent strings are assembled
after authoritative PP tokens, with original per-part conversion followed by
converted-byte rescan for ordinary widening. Intermediate terminators are not
inserted. Ordinary strict width and numeric overflow remain pending until the
final kind; wide rescan falls back to individual numeric bytes on invalid UTF8,
masks ordinary numeric overflow before widening, and never recognizes escapes
a second time. Raw spelling/provenance retains CRLF; decoded raw units use LF.

The native oracle aborts internally for invalid-byte ordinary+UTF8 concatenation.
That unqualified branch is explicitly Unsupported (UNS041), distinct from valid
direct u8 numeric units and observed wide rescan. It is not claimed native-invalid
or accepted. UDLs/floating/legacy encoding and other full inventory gaps remain.

Array lengths include null, omitted bounds infer from supported string copy
initialization, explicit zero and C++ terminator truncation diagnose, padding is
zeroed. Public parameter types adjust arrays to pointers retaining element cv;
ParameterDeclaredType preserves the written form. Array function returns diagnose.
Literal array type reuse is bounded at80 as in EDG; structural comparison handles
larger alias/signature equivalence. Indirect Pointer/Reference/Function caches
bypass owner-dependent or large-array types and never retain literal AST/source.

## Storage and evaluated/unevaluated paths

ConstantCell remains the scalar/element storage object and stable identity.
ReadOnly is an actual allocation fact, defaulted from the allocated type, with
nullable public IsReadOnly for unknown referents. Existing scalar/location
factories and ConstantSlot remain. New ConstantArray, ConstantArrayAddress and
pointer-slot variants separately represent arrays, pointer values and pointer
object loads. Runtime payloads do not convert pointers/strings into integers.
ConstantArrayObject / ConstantPointerValue are immutable retained query facts.
Array cells use existing frame/lifetime-owner allocation and cleanup, so aliases
share element identity and escaping local arrays expire. Existing mutable global
array values are unreadable and expose no invented current scalar Value; their
initializer graph still retains the authored units.

Explicit array decay, simple scalar pointer declarations/parameters/results,
qualification/null conversions, scalar offset arithmetic and subscript work are
implemented. One-past pointer transport is distinct from an actual OOB element.
SubscriptExpression preserves source-left/source-right and PointerOperandIsSecond;
source E1 evaluates before E2 even in integer[pointer] spelling. sizeof records
an unevaluated expression/type, suppresses evaluation/loads/decay and unneeded
selected template bodies while keeping semantic analysis. Dependent sizeof is
an explicit size_t-dependent node, replaced by normal token replay.

SetVariableType is owner-journaled for both Symbol.Type and Declaration.Type.
New tests restore both exact original identities across nested/root-child undo.

## Isolated control-flow merge contract

Do not overwrite either branch's interpreter. New partial ConstantArrays.cs
owns aggregate payload/work helpers. Existing interpreter adds only dispatch,
conversion, parameter/declaration, result-snapshot and cleanup hooks. Preserve
its LeaveAggregateScope call when CF LeaveBlock becomes LeaveScope, and remove
aggregate activation state when a frame exits. Every element is an existing
ConstantCell; CF stores must check real ReadOnly and BeganInEvaluation and record
that same cell identity. Pointer transport performs no scalar store/load.

Future HasSideEffects overrides must recurse through SubscriptExpression
SourceLeft/SourceRight and PointerArithmeticExpression Pointer/Offset;
SizeOfExpression and DependentSizeOfExpression are unevaluated. String literal
parts have no evaluation effects. ArrayInitializationExpression carries only a
decoded immutable literal. Unknown future node effects must remain conservative.

## Authored gate

Eight StringTests groups add typed literal/raw/null graph facts, exact widening
and numeric fallback controls, sizeof and selected-only template noninstantiation,
inferred/padded/local arrays, pointer/null/one-past and expiry, reference element
identity, pointer overload conversions, owner rollback/retention/resource bounds,
and an exact included Unicode+macro+template+frozen-overload+reference-storage gate.
The shared .cpp/.h bytes plus invalid-read complement and native-only type witness
are under fixtures/m1-string-storage-*. Existing112 groups are preserved; only
actually obsolete pointer/array/sizeof Unsupported expectations were replaced.
Expected first integrated group count120. All execution remains supervisor-owned.

General pointers/compound/dependent arrays and refs, assignment/control-flow
(isolated worker), classes, full conversions, remaining literal dialects and all
other full-plan rows remain required; no bounded checkpoint closes them.

## Finite pre-execution review repairs

Wide rescan now follows the actual is_rescan byte fallback (STR01); unknown
readonly facts are nullable (STR02); raw CRLF values normalize separately from
physical spelling (STR03); inferred length0 differs from invalid explicit[0],
reference sizeof uses referent, and dependent sizeof is an explicit node
(STR04–06). Large arrays bypass bounded80 and indirect caches with structural
alias/signature equivalence (STR07), and array returns/parameter adjustment are
handled (STR08). Reversed subscripts expose PointerOperandIsSecond and retain
original range/evaluation order, and indices retain unpromoted integer type
(STR09–10). Array type bounds are independent of string/storage budgets; very
large unrepresentable bounds are ResourceLimit (STR11). Global array/pointer
readonly is separate from constant readability: ordinary const global objects
cannot be read solely on account of cv; constexpr aggregates/pointers and actual
local invocation storage can be read (STR12). Address-only formation is still
allowed and ordinary readonly array element snapshots have null current Value.
These repairs and all authored120 groups await the first actual main execution.

First supervisor execution: buildround2 had zero warnings/errors and JIT119/120;
all112 baseline groups and seven new groups passed, including the coupled gate.
The sole failed test unwrapped only one of the required load/promotion graph
nodes; the assertion now unwraps value conversions before inspecting subscript.
STR13 preallocation work arithmetic is widened before adding an array length.
The existing resource group adds int.MaxValue limits plus an actually evaluated
local char[2147483647] initializer, requiring ResourceLimit before any array/cell
allocation. This source repair/regression still requires fresh execution.

STR14 now classifies valid unimplemented pointer comparisons, compatible-pointer
subtraction and unary-plus-pointer as Unsupported, while invalid operators and
incompatible scalar/pointer operands keep InvalidSource. Direct controls assert
both classes. Fresh ArrayInitializationExpression construction already passes
constantReadable:true in the held evaluator; the new query assertion proves
its unnamed initializer ArrayValue carries decoded units, independently of
ordinary named global array storage remaining unreadable with null current Value.

The later exact native one-past split supersedes the provisional reject-all
reference-binding interpretation. RuntimeValue.NonObjectLocation is a distinct
array/index marker, never a scalar cell/identity. Direct subscript discard and
direct reference_to transport into an ignored parameter or bare constexpr
reference initializer succeed. NonObjectReferenceValue and
NonObjectReferenceBindings expose those address facts separately from actual
object ReferenceValue/ReferenceBindings. A named reference's ref_indirect use,
actual scalar load/store and expired persistent result reject. Pointer-to-bool
of a returned dead-array address is native-accepted in this profile, while a
persistent pointer result rejects; no false extra boolean lifetime gate was
added. STR17 array/pointer-to-bool candidate conversions are now viable at
Conversion rank with explicit decay/load/boolean descriptor facts; an exact
pointer candidate still wins. Exact positive/negative controls cover each split.

Supervisor buildround3 and JITround3 passed120/120 including guarded huge-bound
execution. The further exact native STR16 return-call control rejects using a
one-past location exposed by a reference-returning call, even as an ignored
reference argument. LeaveCall now applies that ref_indirect gate after returning
and cleaning up the frame, preserving direct subscript reference_to transport
and bare reference initialization. The source assertion verifies the observed
return binding, no fabricated extra cell and UndefinedBehavior at call exposure.
This finite repair requires the next supervisor gate; prior120 receipt remains
historical for the earlier bytes.

The array-bound parser helper in StringLiterals.cs now evaluates the original
integer width/sign after only glvalue load/top-cv removal. It checks nonpositive
values before representation limits; literal-1 and a constexpr call returning-1
are InvalidSource ARR001, rather than unsigned wrap misreported as ResourceLimit.
Actual positive unrepresentable bounds still use the explicit resource outcome.

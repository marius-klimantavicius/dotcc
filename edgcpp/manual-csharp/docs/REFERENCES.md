# Scalar reference semantics

The bounded integration passes 95/95 groups under direct-dotnet managed execution
and Linux-x64 NativeAOT, zero failures/skips. Release has zero warnings/errors.
Four round2 receipts have identical73-file maps, including three explicitly
fingerprinted oracle/receipt tools, matching held bytes. Independent bounded
acceptance is `../evidence/review-references-final.md`; complete M3/PLAN is not implied.

The isolated authoring handoff in `../evidence/references-implementation-handoff.md`
records exact routines, branches, source hashes and additive integration hunks.
`source-references.json` separately fingerprints the seven consulted files.

| EDG contract | Preserved behavior and managed representation | Remaining branches |
| --- | --- | --- |
| `types.c` reference relatedness/compatibility; `il.c` reference creation/collapsing | Distinct canonical lvalue/rvalue ReferenceType over qualified referent; alias collapse and ignored alias-applied cv | Volatile/restrict, pointer/array/function/class and dependent-reference forms |
| `declarator.c:pointer_declarator`, `decl_inits.c` missing initialization | Interleaved scalar declarator/binding checks, separate invalid versus Unsupported outcomes | Complete compound declarators, storage/linkage and initialization forms |
| `overload.c` argument match/rank/reference/cv comparison | Viability before ranking; pairwise exact/promotion/conversion then strict Core1374 kind/cv tie breaks; by-value/reference ties remain ambiguous | UDC/classes/pointers/deep qualification/list/implicit object and dialect branches |
| `overload.c` direct reference/initializer preparation | Compatible glvalues bind without loading; prvalues/materialized or converted temporaries retain rank/cv/context and identity | Complete object cleanup/lifetime and reference constexpr execution |
| `expr.c` identifier/call/return; `exprutil.c` indirection/category | Named references are lvalues; calls expose referent type and LValue/XValue while retaining declared return ReferenceType; value use loads explicitly; returns bind first | Dependent references, general member/cast/object paths |
| Return-local inspection and managed graph retention | Valid lifetime hazards stay successful with bounded origin/temporary/owner facts; completed bindings propagate facts in constant time through long alias chains | Full dangling-use analysis and exact diagnostic parity |

Binding-expression type/category describe the referent; Binding.ReferenceType
preserves distinct declared identity. This is an explicit graph adaptation from
EDG's reference-to operation representation. Binding/materialization nodes never
invent folded ConstantValue facts. Unknown origin is not a safety assertion;
binding another reference does not extend a temporary's existing lifetime.

Native lifetime input produces three warnings for direct local/by-value parameter/
direct temporary returns. Managed known-temporary facts also warn for two named
temporary/rvalue-reference hazards. These extra truthful warnings are an
intentional diagnostic adaptation, not native warning parity or invalid binding.

The recursive ownership walk covers both current compound types, references and
functions, through all return/parameter positions. Scope-owned/dependent graphs
are never session-interned, including failed/unsupported analyses. Every future
compound type must extend this walk before interning. Source-level rollback,
discarded live-session WeakReference tests and512-link alias chains execute.

The character coupling fixture checks char16 reference aliases/returns/categories,
char32 promotion to unsigned-int reference versus int conversion, exact char32
binding, wchar selection and char8 materialization payload. Template body replay
keeps frozen candidates despite later declarations/caller hiding, reuses one
instance and retains origins after disposal. A separate shared early-int/later-
void helper fixture makes used-instantiation native acceptance discriminate
definition-context lookup without relying on unsupported reference constexpr.

Eight native controls accept0 and seven reject with language status2. One of the
accepts deliberately witnesses valid dependent-reference syntax still Unsupported
by managed analysis; it receives no managed-support credit. Native-only constexpr
reference assertions establish future oracle facts, not managed interpreter
support. Ten adapter controls pass, including omitted versus explicit source-kind
arguments; Unicode option execution belongs to the following Unicode gate.

Evidence is `references-{build,tests,nativeaot-publish,nativeaot-tests}-round2.json`,
`references-held-snapshot.json`, `references-built-artifacts.json`,
`native-references-*-round1.json` and `oracle-references-unit-round1.json`.
Round1's single nullable test warning is retained; only its assertion flow was
corrected. Remaining reference/template/constexpr/class/pointer/lifetime and
M5/M6 work stays required in FEATURE-INVENTORY.

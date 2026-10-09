# Initial owned semantic object contract

This is the first executable profile of the fresh design, not the full C++ graph.
No code or schema from the prior semantic-export experiment is used.

Native stdout wire is one JSON object (transient packaging protocol only):

```
{
  "formatVersion": 1,
  "profile": "scalar-control-v11",
  "sourceFiles": [{"id":1,"path":"/absolute/main.cpp","parentId":null}],
  "macroDefinitions": [{"id":1,"name":"LIMIT","form":"object","variadic":false,"dynamic":false,"definitionText":"#define LIMIT 4","origin":"source","sourceFileId":1,"line":1,"column":9}],
  "macroParameters": [],
  "macroEvents": [{"id":1,"kind":"define","name":"LIMIT","definitionId":1,"origin":"source","sourceFileId":1,"line":1,"column":9}],
  "types": [{"id":1,"kind":"void","pointeeTypeId":null,"elementTypeId":null,"elementCount":null},{"id":2,"kind":"i32","pointeeTypeId":null,"elementTypeId":null,"elementCount":null},{"id":3,"kind":"u8","pointeeTypeId":null,"elementTypeId":null,"elementCount":null},{"id":12,"kind":"pointer","pointeeTypeId":2,"elementTypeId":null,"elementCount":null}],
  "sourceTypes": [{"id":1,"kind":"int","sourceName":"","underlyingTypeId":null,"executionTypeId":2,"isConst":false,"isVolatile":false,"sourceFileId":null,"line":0,"column":0,"elementCount":null}],
  "recordTypes": [],
  "recordFields": [],
  "sourceFunctions": [{"entityId":1,"returnSourceTypeId":1,"capturePhase":"before-lowering"}],
  "sourceParameters": [{"entityId":1,"ordinal":0,"sourceTypeId":1}],
  "sourceDeclarations": [
    {"id":1,"kind":"function","sourceName":"add","sourceTypeId":1,"entityId":1,"bindingId":null,"functionEntityId":null,"sourceFileId":1,"line":1,"column":1},
    {"id":2,"kind":"parameter","sourceName":"x","sourceTypeId":1,"entityId":null,"bindingId":1,"functionEntityId":1,"sourceFileId":1,"line":1,"column":1}],
  "entities": [{"id":1,"sourceName":"add","linkage":"external","externalKey":"exact EDG linker symbol","definition":"strong","returnTypeId":2,"sourceFileId":1,"line":1,"column":1}],
  "parameters": [{"entityId":1,"ordinal":0,"typeId":2}],
  "functions": [{"entityId":1,"bodyNodeId":10}],
  "bindings": [{"id":1,"functionId":1,"kind":"parameter","ordinal":0,"typeId":2,"sourceName":"x","scopeNodeId":10,"sourceFileId":1,"line":1,"column":1}],
  "nodes": [{"id":10,"functionId":1,"kind":"block","typeId":null,"entityId":null,"bindingId":null,"operator":null,"constantText":null,"valueCategory":null,"evaluationOrder":null,"sourceFileId":1,"line":1,"column":1,"fieldId":null}],
  "edges": [{"parentId":10,"role":"statement","ordinal":0,"childId":11}]
}
```

All fields shown on each record are required (nullable values explicitly null).
IDs positive signed int64; ordinals/line/column nonnegative. Entity definition is
`strong` or `declaration`; linkage external (nonempty exact externalKey) or
internal (externalKey=null). Unique local entity/binding/node IDs; no serialized EDG memory addresses.
sourceFiles contains include occurrences; parentId nullable root; sourceFileId
nullable only for synthetic/unknown origins. Integer constants are exact decimal text;
floating constants are exact fixed-width hexadecimal target bits.
Function bindings exist only for defined functions, parameters describe all
signatures. `scopeNodeId` references the declaring block (parameters root block).
Function body must be block. Locals use ordinal=null; parameters zero-based.

Supported node kinds and ordered edge roles:

- `block`: statement[0..n).
- `return`: value[0] optional for void only.
- `expr-stmt`: value[0].
- `declare`: bindingId + initializer[0] optional.
- `register-cleanup`: bindingId + entityId, no children or other payload.
  Activates one callback for a declared local in the immediately owning block.
  The remaining block suffix is its protected region; see the v10 contract below.
- `if`: condition[0], then[0], else[0] optional.
- `while`: condition[0], body[0].
- `for`: initializer[0], condition[0], step[0] each optional expressions;
  body[0] required statement. Missing condition means true. Header declarations
  use an enclosing block binding and declaration; initializer remains an
  expression. Step executes after normal body completion and `continue`, never
  after `break` or `return`.
- `do-while`: body[0], condition[0]; body executes before the condition, including
  the first iteration. `continue` evaluates the condition before repeating.
- `switch`: condition[0] is a promoted i32/u32/i64/u64 expression; body[0]
  is a block. Evaluate the selector once and dispatch to the matching case, or
  default when present, otherwise exit. Body declarations precede every label
  and have no initializer; source initialization remains ordinary assignments
  at its execution position. Other statements must follow the first label.
- `case`: value[0] is an optional integer constant; absent means default. Cases
  are immediate children of a switch body block, in lexical order. Adjacent
  cases share the following statements; reaching the next label falls through
  without evaluating the selector again. Case constants convert to the promoted
  selector's width/sign representation before comparison. Duplicate converted
  values or default labels are invalid. Nested case labels, GNU ranges and
  arbitrary jumps into cases are unsupported.
- `break`, `continue`: target[0] is a non-owning metadata identity reference.
  Break targets the nearest lexically enclosing switch/while/for/do-while;
  continue targets the nearest while/for/do-while, skipping intervening switches.
  Missing or nonlocal targets are invalid. Arbitrary source jumps remain
  unsupported. C# emission retains switch sections, inserting `goto case` or
  `goto default` only where needed for C fallthrough, and a final `break` when
  the last section can complete normally.
- `constant`: integer or bool type + canonical exact decimal constantText.
  Floating types f32/f64 use exactly 8/16 lowercase hexadecimal digits, without
  a prefix, representing IEEE binary32/binary64 bits including sign and payload.
  A pointer constant must have text `0`, representing a typed null pointer.
  Integer types i8/u8/i16/u16/i32/u32/i64/u64 have their corresponding signed
  or unsigned width ranges; bool permits only 0/1. No leading plus, leading
  zeroes (except zero itself), whitespace or negative zero. Unsigned 64-bit
  values remain text, including values beyond SQLite's signed INTEGER range.
- `binding`: bindingId and matching type. Denotes scalar place; consuming
  expression operations read its current value; assignment and mutation write it.
- `address`: operand[0] is a binding or dereference place; the result is a
  pointer whose pointee is exactly the operand type. Evaluates the address of
  the place without reading its stored value. `&*p` retains pointer identity.
- `dereference`: operand[0] is a pointer to a nonvoid type; the result type is
  exactly that pointee, and valueCategory is place. Reading or writing requires
  a valid source pointer; no additional bounds/lifetime guarantee is invented.
- `call`: entityId + argument[0..n), type matches target return.
- `binary`: operator add/sub/mul/div/rem/eq/ne/lt/le/gt/ge/and/or/bit-and/
  bit-or/bit-xor/shl/shr; left[0],right[0]. Arithmetic and bitwise operations
  require matching promoted i32/u32/i64/u64 operand/result types; add/sub/mul/div
  additionally accept matching f32/f64 operands/results. Remainder and bitwise
  operations remain integer-only. Shifts have
  matching promoted left/result types and any promoted integer right type.
  Comparison operands have matching types (bool only allows eq/ne); comparison
  and logical results are bool/i32/u8. and/or short-circuit their right operand.
- `unary`: operator neg/not/plus/bit-not, operand[0]. Negation, unary plus and
  complement require matching promoted integer operand/result types; negation
  and unary plus additionally accept matching f32/f64 types. Logical
  not returns bool/i32/u8 from a scalar truth test.
- `conditional`: condition[0], then[0], else[0], scalar value result. Arms have
  exactly the result type. Evaluate condition first, then only the selected arm.
  A source scalar lvalue conditional consumed as a value selects and reads one
  arm; its place identity does not escape this value-only profile. Void arms,
  GNU omitted-middle conditionals, and conditional places used as assignment
  or mutation targets are not supported.
- `assign`: target[0] (binding or dereference place), value[0], result type
  target type. For right-to-left indirect assignments both operands must be
  recursively effect-free: no call, assignment or mutation. This bounded profile
  rejects effectful cases until an explicit sequencing representation is added.
- `mutate`: operator pre-inc/post-inc/pre-dec/post-dec, operand[0] (numeric
  or nonvoid pointer place), result type equals operand type. Prefix returns the new value;
  postfix returns the previous value. Unsigned mutation wraps at its width.
  Source bool mutation is rejected by the producer before physical u8 lowering
  loses its distinction; graph bool mutation is rejected by the verifier.
- `convert`: operand[0], destination numeric/bool/pointer type; explicit
  width/sign or floating conversion, nonzero truth conversion, pointer-to-pointer
  cast, or integer/pointer conversion. Floating/pointer conversions require an
  explicit intermediate integer conversion. Floating-to-integer conversion is
  supported only for source-defined values within the destination range. A graph bool-to-pointer conversion requires
  an intermediate integer conversion. Source target pointer width is 64 bits.
- Pointer binary operators are `pointer-add`, `pointer-subtract` and
  `pointer-difference`. Addition has one nonvoid pointer and one promoted integer
  operand, preserving their original left/right order; subtraction has a pointer
  on the left and promoted integer on the right. Their result is that pointer
  type. Difference has matching nonvoid pointer operands and i64 result.
  Displacements count elements, not bytes; difference divides by pointee size.
  Source object bounds and related-pointer rules still apply. Pointer comparison
  operands have matching execution types after EDG's explicit conversions.

Statements have typeId=null; expression nodes and array-element descriptors
must name a type. Array-element is an initializer descriptor, not a statement
or independently evaluable expression. Only `call`
may have void type and only in expression statements, for initializer/step,
or void-return-as-allowed contexts.
No payload for an unrelated kind; no extra edges/fields silently ignored.
Expression occurrences are not shared: each executable node has one parent,
except a function root. Scope/target edges are metadata, not evaluation edges.
Loop condition/body/step roles encode execution and sequencing; database edge
ordering is not itself an execution schedule. Initializer executes once, a for
condition precedes each body iteration, and step follows each completed body.
The structured scope profile excludes arbitrary jumps,
references, globals/TLS, nontrivial record operations, variable/incomplete execution arrays, function
pointers, EH/RAII, inline/COMDAT and runtime
providers. Unsupported constructs fail export rather than emit partial output.

Packager stores typed rows using schema.sql, including producer compiler/config,
original source/include bytes and exact arguments. Only one original TU is sent
to EDG; no .ii preprocessing subprocess or semantic merge. Objects advertise
sourceDeclarations=true, macroDefinitions=true, prelowerSource=false,
macros=false, lifetimes=false. V10 reports additionally advertise
lexicalCleanups=true; this narrower capability is fixed by the profile, not a
new object_info column.
The first flag covers only the scalar declaration/type provenance below; a full
source expression/statement view is still absent.
These explicit limitations must survive the link report; fixtures do not claim
full source-faithful C++ graph or DuckDB readiness. Namespace/class are link args.
The initial target is portable-lp64-binary80-v2; callable ABI dotobj-scalar-v1.
The scalar-control-v3 object profile adds 8/16/32/64-bit signed/unsigned
integers, bitwise/shift expressions and conditional values to v2's structured
loops and explicit loop exit identities. It does not change the callable ABI.
The current canonical schema reader requires scalar-control-v11 and rejects
v1/v2/v3/v4/v5/v6/v7/v8/v9/v10 objects; preserve their respective earlier tool build to read them,
or recompile with this producer.
The profile uses EDG's fixed portable LP64 target: long and long long share the
64-bit physical representation, while exact external keys retain C++ overload
identity. The v4 source declaration/type layer preserves the distinct original
integer type kinds and typedef names. Source enums remain unsupported.

The scalar-control-v4 profile adds **pre-lowering scalar declaration/type
provenance**, without changing the executable node set or callable ABI. This is
not yet the complete source view from the design. Four required wire arrays map
to four normalized tables:

- `sourceTypes`: positive `id`; `kind`; `sourceName`; nullable
  `underlyingTypeId`; `executionTypeId`; boolean `isConst` and `isVolatile`;
  nullable `sourceFileId`; nonnegative `line` and `column`. Builtin kinds are
  `void`, `bool`, `char`, `signed-char`, `unsigned-char`, `short`,
  `unsigned-short`, `int`, `unsigned-int`, `long`, `unsigned-long`, `long-long`,
  `unsigned-long-long`. Builtins have empty sourceName, no underlying type and
  no direct qualifier bits. `typedef` has a nonempty alias name and an
  underlying source type; `qualified` has an empty name, an underlying source
  type, and at least one cv qualifier. Each layer retains its direct qualifier
  bits. Alias/qualification references must be acyclic and preserve the exact
  executionTypeId. Builtin execution mapping follows the fixed LP64 profile;
  original `bool` maps to `u8`, and both `long` and `long-long` map to `i64`
  while remaining different source kinds. Plain `char` retains EDG's selected
  signed/unsigned byte representation. Source aliases are scoped by identity,
  not by spelling; same-named aliases in different scopes need not merge.
- `sourceFunctions`: one row per execution entity, including declaration-only
  functions; `entityId`, `returnSourceTypeId`, and
  `capturePhase="before-lowering"`. The source return type must map to the
  entity's execution return type. The canonical routine signature can originate
  from an earlier declaration; this slice does not enumerate redeclarations.
- `sourceParameters`: `entityId`, contiguous zero-based `ordinal`,
  `sourceTypeId`. Every execution parameter has a corresponding original
  signature type with the same execution representation. Unnamed and
  declaration-only prototype parameters are represented here.
- `sourceDeclarations`: positive `id`, `kind`, `sourceName`, `sourceTypeId`,
  nullable `entityId`, `bindingId`, `functionEntityId`, and source position.
  A `function` row has entityId only and sourceTypeId denotes its **return
  type**; exactly one such row must match each sourceFunctions row. A
  `parameter` or `local` row has bindingId plus functionEntityId and no
  entityId; exactly one row must match each execution binding's kind, source
  name, function owner and execution type. Prototype-only parameters do not
  have declaration rows. A definition's declared parameter source type may
  differ from its canonical signature source type in typedef or top-level cv
  spelling; their execution representations must agree. Bindings lacking a
  pre-lowering snapshot are rejected in this profile.

The producer copies these scalar type and declaration facts before EDG mutates
the corresponding IL during lowering, including signatures lowered early
through references from another function. Private EDG pointers are lookup keys
only and never appear in the object. Source positions denote EDG declaration
positions; these are not token ranges or a reconstruction of every syntactic
spelling. Unreferenced typedef declarations, statement/expression
provenance, initializer structure, and destruction boundaries are not captured
by this slice. Source snapshots alone do not imply those capabilities.

The linker validates source-to-execution mappings but uses the authoritative
execution graph for C# semantics. It also emits `source-map.json` for inspection
and navigation, retaining type/declaration IDs and source-file references. This
artifact is derived from the object tables and requires no original files.

The scalar-control-v5 profile adds **effective macro definition provenance**
captured from EDG's actual preprocessor. The executable graph and callable ABI
remain unchanged. Three additional required wire arrays map to normalized tables:

- `macroDefinitions`: contiguous positive `id` in installation order; nonempty
  `name`; `form` equal to `object` or `function`; boolean `variadic` and
  `dynamic`; `definitionText`; `origin` equal to `source`, `command-line`, or
  `predefined`; nullable `sourceFileId`; `line` and `column`. Ordinary definitions
  preserve EDG's canonical decoded complete `#define NAME...` text. Comments and
  whitespace may be normalized, so this is not a raw source token range. Dynamic
  predefined macros have `dynamic=true` and empty `definitionText`; the object
  does not invent a fixed value for `__LINE__`, `__FILE__`, or similar builtins.
  Only function macros can be variadic. Source-origin definitions require a
  source-file occurrence and positive line/column; other origins use null file
  and zero line/column. A redefinition that installs a new effective definition
  gets a new identity. A benign identical redefinition discarded by EDG does
  not receive a new installation event.
- `macroParameters`: `definitionId`, contiguous zero-based `ordinal`, and `name`.
  Only function definitions have parameters. Nonempty names are unique per definition;
  the final variadic parameter is included, retaining `__VA_ARGS__` or its GNU
  named spelling without an ellipsis. EDG dynamic predefined function macros
  may have an unnamed argument, represented by an empty name only for those
  predefined dynamic definitions. Zero-parameter function macros are distinct
  from object macros. Variadic functions must have a final parameter.
- `macroEvents`: contiguous positive chronological `id`; `kind` equal to
  `define` or `undef`; `name`; nullable `definitionId`; `origin` and source
  position in the same form as definitions. A `define` owns exactly one
  definition and must agree with its name, origin and position; definition IDs
  follow their define-event order. An `undef` has null definitionId. Undefining
  an already undefined name is valid and remains an event. Source directives in
  inactive conditional branches are not events. These identities are local to
  each translation unit and do not merge across separately compiled objects.

This is an effective definition history, **not** a replacement-token semantic
IR, complete directive history, or expansion/argument history. Parameter names
and canonical definitions support inspection of stringizing, token pasting,
variadic and function macros but are not linked renamable execution references.
The reader does not interpret the replacement text or infer exported C# APIs.
Source snapshots retain original text where available; the canonical definition
alone does not preserve every original token's location. Macro push/pop and
precompiled/module macro import paths are rejected until their state transitions
can be captured faithfully. Non-UTF-8 decoded macro text is rejected by this
profile. The `macros` capability remains false; the new narrower
`macroDefinitions` capability is true. Linking emits `macro-map.json` as inert
inspection metadata, separately from `source-map.json`, and does not add macro
wrappers, constants, initialization code, or runtime dependencies to generated C#.

Expression valueCategory is place for binding/dereference and value otherwise; statements
use null. evaluationOrder is null for statements, none for constant/binding,
and left-to-right/right-to-left/unspecified for operations. Single-operand address
and dereference nodes also permit none; their sole operand is evaluated once. Unspecified permits
the emitter to choose left-to-right; do not change a specified order. A
plain binding assignment needs only RHS-before-write. An indirect target rooted
in an owned binding address with constant offsets also remains invariant while
the RHS runs; other effectful indirect right-to-left assignments are rejected. Unsupported ordered expressions fail explicitly. u8 represents
EDG's lowered unsigned-byte bool and requires numeric 0/1 comparison results
unless expression result type is bool. Explicit conversions encode truth and
width behavior. Parameters/locals may use any supported integer, bool or pointer type, never
void. Function returns may also be void. C# integer literals preserve signedness and width;
unsigned arithmetic and negation wrap with explicit unchecked expressions,
including compile-time constant expressions. C# shift counts explicitly convert
to int where needed. Only source-defined shifts are promised: negative or
width-exceeding C/C++ counts remain outside that language's defined behavior.
Logical and conditional edges encode lazy execution independently of the
physical order of their rows. Integer promotions and usual arithmetic
conversions are performed by EDG and represented as explicit convert nodes;
the managed verifier does not infer C/C++ type conversion decisions.

SQLite names use snake_case counterparts of wire fields. functions, bindings,
nodes form deferred graph relationships: package using defer_foreign_keys=ON
inside the transaction, then foreign_key_check before commit. Source bytes and
producer/config provenance are added by the packager, not native JSON.

Logical SHA256 algorithm: initial bytes dotobj-semantic/1 + LF. For each user
table except content_digest sorted by name: byte T then frame(table name), byte
S then frame(sqlite_master.sql). Frame = ASCII decimal UTF8 byte length, colon,
bytes. Rows ordered by table primary key: byte R, then each cell's SQLite type
code as one ASCII digit followed by frame of value (integer invariant decimal,
text UTF8, blob raw bytes, null empty); LF after row. LF after each table. REAL
cells unsupported. source snapshots SHA256 additionally verified independently.

Required nodes_by_function(function_id,id), bindings_by_function(function_id,id),
and source_declarations_by_function(function_entity_id,id) indexes support
per-function graph/provenance loading; index definitions are validated
separately from the logical table-content digest. Changing physical indexes must
not change program identity, but an unsupported schema object is rejected.

The definition-text capture profile rejects actual `__VA_OPT__` replacement
operators: the stock inspection formatter is not a complete round-trip format
for trailing optional text. Quoted occurrences remain ordinary macro text.
EDG preprocessing semantics are unchanged; a future token-based macro graph can
represent this construct without relying on inspection formatting.

The scalar-control-v6 profile adds structured switch dispatch and case markers
to the owned execution graph without changing the callable ABI or source/macro
capabilities. A switch block's declarations are emitted in a surrounding C#
block to preserve scope across sections. Their default initialization only
satisfies C# definite assignment; it does not define source indeterminate reads.
The normal executable occurrence/ownership rules also apply to case constants.


The scalar-control-v7 profile adds **typed data pointers and places**. Every
execution `types` row has nullable `pointeeTypeId`; a `pointer` row requires a
valid child execution type, and primitive rows require null. Execution types
are canonical structural identities within an object; duplicates, cycles and
nesting beyond 128 are rejected. Across objects the linker compares recursively
resolved type shapes rather than object-local numeric type IDs. Primitive IDs
1 through 9 retain the producer's prior meanings; structural type IDs begin at 10.
`void*`, scalar pointers and nested pointers are accepted. Pointer-to-array,
record, function and reference types remain outside this profile.

Source `pointer` types have an underlying source type (including aliases and
qualifiers), an empty sourceName and no direct qualifier bits; pointer-level
qualifiers use a `qualified` wrapper. Its execution representation must point
to the underlying source type's execution representation. Source pointer/alias
chains are acyclic. `source-map.json` now also includes execution type rows so
pointer relationships can be inspected without inferring local IDs.

Generated C# uses an unsafe partial class and ordinary typed pointer declarations,
address/dereference operators, assignment and increment/decrement. No generated
heap allocation or pinning is needed for addresses of these unmanaged stack
locals. This does not add allocation, globals, lifetime tracking or bounds
checks. Unsupported sequencing is rejected instead of silently changing the
source's specified evaluation order.

When any input object uses pointer types, the first generated partial class
contains one owner static constructor checking `IntPtr.Size == 8`. An incompatible
32-bit process fails explicitly before translated functions execute. This
enforces the fixed target ABI while allowing any supported 64-bit OS/runtime.


The scalar-control-v8 profile adds **fixed local arrays**. All execution types
carry nullable `elementTypeId` and `elementCount`; only `array` uses them, with
positive extent and a nonvoid element type. Array `pointeeTypeId` is null.
Arrays and pointers compose recursively and participate in structural signature
matching. Checked recursive layout must fit 65,536 bytes per array; VLA,
incomplete execution arrays, array-valued parameters/returns and globals remain
unsupported. Original source array types carry the element source type as
`underlyingTypeId` and a nonnegative `elementCount`; other source kinds require
null extent. A sized source array maps to its execution array. Original array
parameters adjust to pointer-to-element for callable ABI matching. Unsized
source array parameters have extent zero and use that adjusted pointer as their
execution representation; this is provenance only, never an incomplete storage
layout. Nested incomplete elements, pointer-to-incomplete-array and incomplete
locals are rejected.

- `array-decay`: expression with `operand[0]` an array place; its result type is
  pointer-to-element and value category is value. Normal `address` instead
  produces pointer-to-the-whole-array, preserving pointer arithmetic scaling.
- `array-init`: statement with `target[0]` a local array binding place and
  ordered `element[0..n)` descriptors. Initialization clears all storage then
  evaluates and writes elements in descriptor order. It runs at the original
  declaration execution position, including each loop iteration.
- `array-element`: descriptor, allowed only as an immediate `array-init`
  element child; carries leaf scalar/pointer `typeId`, flattened element index
  as canonical unsigned decimal `constantText`, and one `value[0]` scalar
  expression of that type. Other payload, valueCategory and evaluationOrder
  are null. Index must be within the recursively flattened array extent.
  Sparse and repeated indices retain ordered write semantics.

Array expressions are places only and cannot be copied or implicitly converted;
address/decay/init explicitly identify allowed uses. Nested array initialization
is flattened while preserving structural array types. String initialization
records exact character bytes, including interior NULs. Source bounds/lifetimes
remain the caller's responsibility; this profile does not add bounds checks.

C# stores each structural array in one unmanaged sequential-layout value struct,
with exact byte size and a scalar/pointer leaf field preserving natural
alignment. Deterministic names (for example `__DotObjArray_A3_i32`) are shared
across translation units. Locals retain their source names; addresses and decay
use ordinary unsafe pointers. No heap allocation, managed pinning or dynamic
code is involved. Dense or sparse all-constant integer initialization is emitted
as one static `ReadOnlySpan<T>` property backed by a collection expression and
copied into the destination. Trailing zeros are represented by a destination
clear and a short prefix copy; densely populated full arrays need no separate
clear. Very sparse high-index initializers use ordered stores instead of a long
zero-filled property. Dynamic and pointer elements use a local typed pointer alias and
ordered index assignments after a clear. This is stack storage, so ordinary
source stack consumption still applies. Generated code remains NativeAOT
compatible within the existing fixed 64-bit target ABI.


The scalar-control-v9 profile adds **binary32 and binary64 floating point**.
The native producer reserves type IDs 10/11 for f32/f64, with constructed
pointer/array types starting at 12; consumers resolve object-local type IDs and
compare structural types independently of those numeric identities. Original
source `float` and `double` types are preserved distinctly and map to f32/f64.
Typedef, cv, pointer and array wrappers compose with these types as before.
Both execution constants and array leaf initializers retain exact target bits;
SQLite REAL and JSON numeric floating payloads are deliberately not used.

C# signatures, locals, pointers and fixed array storage use ordinary `float`
and `double`. Finite constants use invariant round-trip decimal literals with
explicit F/D suffixes; negative zero is retained as `-0.0F` or `-0.0D`.
Nonfinite scalar constants use `BitConverter.Int32BitsToSingle` or
`Int64BitsToDouble` to retain NaN payloads and signs. All-finite constant arrays
use typed `ReadOnlySpan<float/double>` properties. Arrays containing any
nonfinite constant instead embed `ReadOnlySpan<uint/ulong>` exact bit patterns
and reinterpret that data with `MemoryMarshal.Cast` before the bulk copy.
Neither path creates a managed array. Only positive-zero suffixes may be
omitted by the sparse initializer optimization; negative zero is data.

Floating arithmetic, comparison and truth tests use ordinary managed operators.
NaNs compare unordered, signed zeros compare equal and convert to false, and
all other floating values (including NaN) convert to true. Source-defined
floating/integer conversion and f32/f64 widening/narrowing are supported;
NaN/infinite/out-of-range float-to-integer casts have no portable guarantee.
No long double/extended precision, complex arithmetic, fenv/rounding-mode
control, floating exception flags or traps are implemented. Arithmetic NaN
payload propagation and cross-architecture bit-identical arithmetic are not
promised; exact constant and memory bit preservation is distinct from those
execution behaviors. Existing scalar-profile limitations remain in force.


The scalar-control-v10 profile adds **lexical cleanup activation**, initially
for GNU `__attribute__((cleanup(callback)))` on automatic scalar, pointer and
fixed-array locals. The attribute is semantically resolved by EDG; the object
contains the selected callback entity and local binding identities, never an
attribute string to reparse or C# text. EDG's `a_variable.cleanup_routine` is
implicit metadata: the frontend does not emit its calls. The stock C backend
preserves the GNU attribute through `form_variable_attributes`. This adapter
captures the callback and original initialization presence before lowering.

A `register-cleanup` statement has null typeId, operator, constantText,
valueCategory and evaluationOrder, and no edges. Its nonnull bindingId names
a local, previously declared in the same immediately enclosing block; entityId
names a callable entity with exactly one pointer parameter. The parameter must
point to the local's execution type, or be void*. Nonvoid callback results are
discarded. Array callbacks receive the address of the whole array, not decay
to an element pointer. Callback signatures and linkage are checked like other
calls, including references that occur only in cleanup registrations. A local
has at most one registration node, even in mutually exclusive subtrees.

Executing registration starts protection of the following statements through
the end of that block. This is a semantic region, independent of the selected
managed emission. The callback runs once on leaving that region by normal
completion, return, break, continue or supported managed exception propagation.
Later registrations nest inside earlier ones, so cleanup order is reversed.
A return operand is evaluated before cleanup; a for-initializer registration
protects the whole loop, whereas a loop-body registration is entered and left
once per executed iteration. Reaching an earlier return does not activate a
later cleanup merely because its variable's storage declaration was hoisted.

Initialized variables register immediately after their own retained `stmk_init`
completes successfully. Uninitialized variables register at their retained
`stmk_decl`. The producer verifies every captured cleanup variable receives
exactly one activation anchor. Partially dynamic arrays with split zeroing and
separate initializer stores are rejected until an exact initialization-completion
anchor is represented. Uninitialized cleanup variables in multi-declarator
declarations are likewise rejected because one declaration marker can precede
several interleaved initialization points. Immediate switch-body registrations
are rejected: dispatch could enter after registration. Braced case blocks with
their own locals and activation nodes are supported. Arbitrary jumps remain
unsupported; no control-flow path may jump into a protected suffix.

The C# emitter opens nested `try/finally` regions at registration and emits a
direct `callback(&local)` in each finally. It adds no heap allocation, pinning,
setjmp, runtime cleanup stack or activation flags. Storage declarations can
still be normalized to block entry; cleanup lifetime cannot. The derived
`lifetime-map.json` identifies each activation node, owning block, binding,
callback and protected statement-ordinal suffix. It streams alongside body
emission and is inspection metadata, never a second cleanup schedule.

Reports advertise `lexicalCleanups=true` for this bounded capability. The
existing database `lifetimes` flag remains zero: complete C++ object lifetime
capture and RAII are **not** implemented by this increment. Records and selected
constructors/destructors, temporary/full-expression boundaries, lifetime
extension, partial construction, source exceptions and termination on a second
exception still require their own representations. Existing C++ destruction
obligations remain explicit export failures, not omitted callbacks. Source
reads of indeterminate/dead storage remain invalid; this is not a liveness
optimizer or a promise to diagnose every pointer lifetime violation.


The scalar-control-v11 profile adds **named structs, layout and member places**.
Two normalized wire arrays/tables retain definitions and fields:

- `recordTypes` / `record_types`: `typeId` references an execution type of kind
  `record`; `identityKey`, `sourceName`, positive `size` up to 65,536 bytes,
  `alignment` in 1/2/4/8, and sourceFileId/line/column. Pointer/array child columns
  in the execution type are null. Exactly one definition describes each record
  execution type. Empty and incomplete records are rejected.
- `recordFields` / `record_fields`: positive `id`, `recordTypeId`, contiguous
  zero-based `ordinal`, `typeId`, nonempty `sourceName`, nonnegative `byteOffset`,
  `sourceTypeId`, and sourceFileId/line/column. Field IDs are object-local
  identities, not names or offsets masquerading as references. Original field
  types preserve typedefs and qualifiers and must map to the execution type.
  Every node now has a nullable `fieldId`, appended after location fields in
  the wire/schema; only a `member` node may use it.

Nominal keys use `record:` followed by the qualified source name. Named namespace
segments distinguish C++ records with equal basenames. Global tags shared by C
and C++ use the same identity, allowing mixed-language objects from a common
header to link. Names and keys are captured before lowering moves or renames
entities. Local, anonymous, nested-class and template record identities are
outside this increment. Forward declarations may precede complete definitions
in the same TU; an object containing only an incomplete record is rejected.
Deferred completion from another object remains future work.

Record identities are terminals in structural pointer signatures. A record may
therefore contain pointers to itself or mutually recursive records; its layout
is checked separately. By-value layout cycles are rejected, as is containment
nesting beyond 128 even when records arrive child-first. Field offsets must
follow natural nonoverlapping layout, record alignment must equal the maximum
field alignment, and size must include the corresponding tail padding. Packing,
over-alignment, unions, bitfields, inheritance, empty/flexible array members,
no_unique_address and synthetic subobject layouts are not supported. The native
producer also rejects user/nontrivial member functions and member initializers.
Field arrays of supported scalar/pointer elements and nested records work;
arrays of records remain excluded, including sized source array parameters.

The linker compares the complete ordered layout for each nominal key across
objects: source name, size, alignment, field names, offsets, execution types
and normalized original field-type signatures. Typedef spelling is not nominal
field identity; equivalent qualifiers through typedef/array layers compare
equally. Original distinctions such as long versus long long remain significant
even when both lower to i64. This is a supported-layout compatibility check,
not a complete C++ ODR token/body equivalence proof.

Two executable node forms are added:

- `member`: typed place with fieldId and one `operand[0]` record place whose
  type owns that field. Result type equals the field type. Other entity/binding/
  operator/constant payload is null. Value category is place; operand evaluation
  order is retained. Pointer selection uses a typed dereference operand, not a
  byte offset. Named fields work as loads, stores, addressable places, array
  decay targets and numeric mutation targets.
- `default-value`: record-typed value, evaluationOrder none, no operands or
  other payload. It denotes zero initialization of supported record storage.
  It is valid in a record declaration initializer or a record-place assignment.
  Ordinary record-valued copies, call arguments, returns, conditional arms and
  other scalar operations remain rejected.

Record aggregate initialization normalizes into a block containing a default
assignment followed by ordered field stores. Nested aggregates recursively use
named member places, and array fields reuse `array-init`. The latter now accepts
a local binding or a chain of member places rooted in local owned storage;
it does not accept an effectful or arbitrary indirect destination. This keeps
initialization at the original execution point and avoids reevaluating a target.
GNU cleanup may register a whole record after successful initialization or at
an uninitialized declaration, using the existing lexical-cleanup contract.
Ambiguous split initialization with cleanup still fails closed.

C# emits one public unmanaged explicit-layout struct per merged nominal record,
with Size, Pack and FieldOffset metadata matching the checked source layout.
Pack is not treated as a way to increase CLR alignment above its fields; the
natural-layout checks enforce that requirement. Source field names, ordinary
`value.member` and `pointer->member` access remain visible. Generated type/field
names are allocated by identity; existing function names win unavoidable C#
type/function collisions. No record heap storage, pinning, raw frame offsets or
per-member access helpers are introduced. Record definitions and original field
type references appear in `source-map.json`.

This completes a layout prerequisite for C++ lifetimes, not constructors or
destructors. Aggregate copies and by-value calling conventions remain excluded:
C# assignment alone does not prove correct copying of array-wrapper storage.
Next steps require selected constructor/destructor calls, nontrivial object
actions and their activation/cleanup semantics. General lifetimes remains false.

## Project C profile: scalar-control-v12

This coordinated C/runtime tranche passes 597 regression checks, including
NativeAOT. It does not yet satisfy SQLite or DuckDB project acceptance.
The campaign and acceptance gates are in
[`dotobj-project-campaign.md`](../docs/dotobj-project-campaign.md).

Additional normalized wire arrays (SQL names use snake_case):

- `routineTypes`: typeId, returnTypeId, variadic, exceptionPolicy. Each typeId owns an execution
  type of kind `routine`; `routineParameters` contains typeId, ordinal,
  parameterTypeId. A function pointer is an ordinary pointer to a routine type.
- `sourceRoutineTypes` and `sourceRoutineParameters` retain equivalent original
  source signature references, including source typedef/qualifier structure.
- `storages`: id, sourceName, linkage, externalKey, definition, typeId,
  sourceTypeId, isThreadLocal, isConst, alignment, sourceFileId, line, column.
  Source type is nullable for synthetic literal storage. Internal storage
  identity is local to its object; external identity uses the exact linker key.
- `storageData`: storageId, byteOffset, dataHex. The transient wire spelling is
  nonempty even-length lowercase hexadecimal; the database stores exact bytes
  in `storage_data.bytes` as a BLOB. Unspecified bytes start as zero. These are
  target-representation constants, not host-endian values or C# text.
- `relocations`: storageId, byteOffset, targetStorageId, targetEntityId, addend.
  Exactly one target is present. Address identity is resolved during linking;
  address values are installed after allocation, without embedded native process
  pointers. Addends are signed byte displacements.

Entities append required `routineTypeId`; return type and parameter rows must
agree with that signature. Definitions admit `linkonce` in addition to strong
definitions and declarations; link selection must not mistake duplicate strong
definitions for mergeable inline definitions.

Nodes append nullable `storageId`. `storage` is a typed storage place.
`storage-address` is a pointer value with a storage identity and canonical signed
byte addend in constantText. `function-address` references an entity and has its
routine pointer type. `indirect-call` owns one operand callee and ordered argument
edges; its signature is determined by the callee type. Other node payload rules
remain explicit and unrelated fields must stay null.

Record definitions append `layoutKind` (`struct`, `union`, `class`); fields append
nullable `bitOffset`, `bitWidth`, `bitSigned`. Ordinary fields have all three
null. Bitfields describe the exact starting byte/bit and signed value width,
allowing named C# properties without widening stores into adjacent fields.
Incomplete record rows have size/alignment zero and no fields; their nominal
identity may resolve to a compatible complete definition in another object.
Incomplete records cannot provide by-value storage. Record arrays and unions
retain their owned element/field types and target layouts. Representation bounds
and CLR storage constraints are verified rather than assumed from SQL validity.

Macro events now admit `push` and `pop`. Their definitionId is the saved/restored
previous installation, or null for an undefined macro. Push observes the current
definition without changing it. Pop must match the top saved definition for that
name and restores effective state. Definition rows still belong to exactly one
original define event. Stack events carry the pragma location, not the original
definition location. Only accepted frontend operations are observed; a pop with
no saved state remains the frontend's warning/no-op and creates no event.

The routine exception policy is `unwind` or `terminate`; it participates in
signature compatibility. Nonempty noexcept definitions and external forwarding
boundaries terminate when an exception escapes. Empty or single-void-return
bodies need no boundary. General C++ throwing and destructor lifetimes remain
outside this profile.

The v12 executable graph additionally owns volatile places, varargs operations,
intrinsics, labels/gotos and ordered sequence expressions. Shared authoritative
`Edg.Runtime` VaList/VaArg/VaPack sources implement variadic calls using
`params ReadOnlySpan<VaArg>` and callee-owned packing. Generated runtime imports
use `Edg.Runtime.LibC`; imports mentioning translated record types remain in the
translated owner. Imports reject unless explicitly allowed, and implementations
must satisfy checked signatures and layout metadata. External storage providers
return the stable address of their own cell; TLS accessors run on each access.

Complete nonvolatile POD records may be copied, passed and returned, including
array payloads represented by physical InlineArray fields. Incomplete and
flexible-tail by-value records, alignment above eight, nontrivial C++ records and
volatile aggregate copies still reject. Source bitfields remain named properties;
value truncation and arithmetic promotions follow the owned computation types.
Globals/TLS use pinned arenas; static data emits typed constant spans, UTF8
literals/comments and named object/table initializers when representable.

Ordinary source control flow stays structured. Legal C goto entries that C#
forbids trigger local-variable hoisting and control-flow adaptation only in the
affected function. Gotos with lexical cleanup and void-prefix comma expressions
inside value contexts still reject. Execution reachability skips unused inline
header bodies, but strong externally defined library APIs remain roots. A full
independent source semantic index, C++ lifetimes/EH, binary80, instance-owned
storage and actual SQLite/DuckDB execution remain outstanding gates. The current
execution target is 64-bit little-endian; Linux x64 is the tested platform.


## Provider profile: scalar-control-v13

Objects and readers explicitly change profile rather than silently reinterpreting
v12 artifacts. Bindings append required wire `isSynthetic` / SQL `is_synthetic`.
Original bindings retain exactly one original source declaration. A synthesized
local has no fabricated source declaration and is accepted only when EDG marks it
compiler-generated or lowering-generated; a missing capture alone is insufficient.
Parameters cannot use this escape. Execution type, identity, ownership and source
location remain recorded for synthesized locals.

The execution primitive `f80` carries x87 binary80 values in 16-byte storage with
16-byte native alignment. Constants use exactly 20 lowercase hexadecimal digits:
four sign/exponent digits followed by sixteen significand digits. Static storage
uses the ten little-endian payload bytes and zero padding. Source floating-kind
distinctions remain separate. The authoritative shared Float80 implementation
supplies arithmetic; CLR layout packing does not establish stack alignment, so
addressable locals/parameters require explicit aligned storage. Layouts containing
binary80 must retain native field offsets and stride. Volatile binary80 uses ordered non-atomic payload accesses as specified below.
These capabilities pass native/JIT/NativeAOT fixtures and real-provider probes;
complete project execution remains a separate gate.


The v13 source signature extension appends `parameterMode` to source routine
rows (`prototype` or `unspecified`) and required `routineSourceTypeId` to source
function rows. The latter references the actual signature captured before
lowering, including direct-only routines. Initially, unspecified C execution is
limited to real zero-parameter definitions and zero-argument calls; unresolved
unspecified externs, old-style parameter declarations and extra arguments remain
unsupported. This does not silently convert the original declaration into a
prototype.

Provider integration retains explicit volatile places for binary80.
Ordered BCL volatile accesses read/write the 64-bit significand and 16-bit
sign/exponent payload separately, leaving padding unspecified. This establishes
observable accesses without claiming atomic 16-byte operations. Floating-point
exception environment behavior remains a separate runtime capability.


Provider objects are explicit linker inputs (`--provider path.dotobj`, repeatable).
Positional objects remain primary libraries: all their materialized function and
storage definitions are roots, including exported APIs unused by an executable
entry. Providers contribute only definitions reached through typed entity/storage
references and storage relocations. Unselected provider contents remain in the
independent object and source index. `link-selection.json` records object roles,
selected providers and before/after definition counts. Without provider options,
linking retains the previous all-primary behavior. Complete-object verification
continues to inspect every body. Startup actions require separate owned metadata;
the current provider profile still rejects unrepresented constructor attributes.

## Startup profile: scalar-control-v14

Required wire `initializationActions` rows map to SQL `initialization_actions`:
`id`, `entity_id`, `phase`, `priority`, `ordinal`, optional `source_file_id`,
`line`, and `column`. The only phase is `startup`; each target is a defined,
nonvariadic `void(void)` routine. Priority is an unsigned 16-bit value (including
zero), defaulting to 65535. Ordinals and targets are unique per phase within one
object. Ordinals need not be dense. Source locations retain the constructor
declaration provenance. Duplicate cross-object registrations remain distinct
even when linking coalesces their function bodies.

Actions are ordered by priority, original linker input index, then the captured
EDG routine-list ordinal. Equal-priority source declaration order is not portable;
the fixture with a prior prototype follows stock EDG C output rather than GCC's
different order for that source. Selecting a provider translation unit retains
its startup actions as execution roots and follows their typed dependencies.
Unselected providers contribute no actions.

`Edg.Runtime.ObjectProgram.InitializeLibrary()` is an explicit hosting boundary.
It first completes physical translated storage setup, then invokes source actions
outside CLR type-initialization and state locks. Same-thread reentry returns;
concurrent initializing hosts wait; completed initialization is idempotent.
An escaping host/provider exception is cached with its dispatch information and
rethrown to later callers without rerunning actions. Executable `Run` initializes
after its entry hook and before invoking source `main`; its leave hook still runs
if initialization fails. Library initialization does not impersonate process entry.

This profile captures GNU routine constructor attributes only. GNU shutdown
attributes, C++ namespace-scope dynamic initialization and atexit/static destructor
ordering remain unsupported. In particular, cached host failure is not a proposed
substitute for C++ dynamic-global initialization's termination semantics.

## Record-origin profile: scalar-control-v15

The required `sourceRecords` and `sourceFields` arrays populate independent
`source_record_types` and `source_record_fields` tables. A source record has a
unique source-type reference, stable nominal identity, original spelling/kind,
completion state, native size/alignment, nullable trivial-copyability fact,
capture phase and location. Incomplete declarations have no completed layout or
copyability fact. Original fields retain declaration ordinal, original type and
spelling (including unnamed fields), mutable/no-unique-address facts, optional
bit width (including zero) and location. A zero-width bitfield can therefore
remain a declaration without becoming physical storage.

Execution records append `sourceRecordId`, `representationKind`,
`valueCopyKind` and `captureProof`. Complete representations refer to an original
record. ABI-created representations explicitly have no source declaration and
require observed ABI-creation provenance. Ordinary source snapshots and observed
class prelowering are distinguished; a lowered field list is not evidence that
its fields were source declarations. Physical fields append `sourceFieldId` and
`fieldRole`; original fields require original type/declaration links, while true
hidden vptr/ABI fields have neither.

The `ordinary` versus `explicit-only` value-copy policy prevents a class with
simple physical fields from accidentally gaining POD assignment or parameter/
return semantics. Static image population is physical initialization, not a
source C++ copy operation. Source metadata and physical layout are independently
validated and compared when linking complete definitions. Source capture phase
need not match between C and C++ compilations of a compatible shared C header.

This tranche initially admits complete own-vptr layouts and explicitly observed
ABI records. Base/subobject views, tail-padding reuse, hidden function parameters,
constructors/destructors and general C++ lifetime/exception semantics remain
separate required work. The new rows describe records materialized by this graph;
they do not imply that the complete independent source semantic index is finished.

The v15 emitter also honors C++17 right-to-left assignment evaluation using owned
`evaluationOrder`. When required, it captures the right value before evaluating
the destination and, for compound assignment, before reading its old value.
Expression lifting retains lazy branches and loop continuation targets. The
execution oracles use original C++17 compiled at O0/O2 plus JIT/NativeAOT; stock
EDG C output that retains ordinary C assignment expressions is not claimed to
provide a deterministic operand-order execution oracle.

## Integer128 profile: scalar-control-v16

The execution kinds `i128` and `u128` have 16-byte size and native alignment;
original source kinds remain `int128` and `unsigned-int128`. Integer constants
use exact canonical decimal text, including the full signed/unsigned ranges.
Generated code uses `System.Int128`/`System.UInt128` values and exact two-word
construction, not narrowed 64-bit intermediates. Static arrays use embedded
constant ulong-pair spans reinterpreted as the target values, because nonconstant
Int128 constructor expressions in a span collection would allocate an array.

Address-exposed locals/parameters use the existing explicit aligned storage;
CLR natural alignment alone is insufficient. Globals/TLS retain aligned pinned
arenas. Shared `VaArg`/`VaList` packing preserves all 16 bytes without introducing
a second runtime. Integer-to-pointer conversion truncates at the native pointer
width; pointer-to-integer128 follows this EDG LP64 target's observed signextension
policy. The latter is an implementation-defined target fact, not a universal C
rule. In-bounds pointer arithmetic with integer128 indices bridges through the
native-size displacement type.

The first tranche deliberately rejects integer128 switch selectors, volatile
accesses, bitfields and arithmetic intrinsics until each has a correct lowering.
Address relocation into an integer128 slot also rejects because the current
relocation contract describes an eight-byte pointer patch, not its required
128-bit extension. Ordinary pointer fields in records containing integer128
remain eight-byte slots. General C++ reference/object/lifetime support remains
independent of this arithmetic extension.

The runtime hook report also publishes `ObjectProgram.ReleaseThreadStorage`.
This internal bridge clears only the calling thread's owned TLS arena, after the
runtime has invoked C++ TLS finalizers, C11 TSS callbacks and libc TLS cleanup.
The next source TLS access creates its initial image again. Global state is
unaffected. The shared runtime validates this explicit bridge before enabling
thread services; a stale linker contract cannot silently omit owned TLS release.

## C++ source references and receiver ABI: scalar-control-v17

Original lvalue/rvalue reference kinds remain distinct source types, with their
referent's CV qualifiers and source identity preserved. Their execution type is
a pointer to the referent; that lowering is not permission to equate a reference
with a pointer in cross-unit source signature matching. Original routine types
recursively retain written return/parameter types. Function parameter matching
removes top-level CV and applies only the language's outer array/function decay;
references to arrays/functions do not decay. The currently admitted unspecified
C definition has no parameters and can match a prototype with no parameters.
The original parameter-mode fact remains in the object.

`source_routine_types` append `object_type_id`, `object_is_const`,
`object_is_volatile` and `object_ref_qualifier` (`none`, `lvalue`, `rvalue`). The
object type is the unqualified original nominal record, not a fabricated pointer
parameter declaration. Execution `parameters` append `parameter_role`
(`source` or `this`) and nullable `source_ordinal`. An instance routine has one
proved `this` parameter at execution ordinal zero, then a bijection between
written parameters and remaining execution positions. Static members and free
functions have no receiver. Source declarations for written parameters join
through `source_ordinal`; compiler-owned `this` has an explicitly synthetic
binding and no invented original declaration. Receiver ownership is observed
through EDG's actual receiver variable/flags, not the spelling of a variable.

Original record fields append `has_initializer`. This fact survives original
capture even when a particular object uses explicit aggregate initialization.
It does not by itself imply a nontrivial copy operation or authorize discarding
constructor, dynamic initialization or lifetime obligations. True member-pointer
values, hidden result buffers and parameters passed through copy constructors
remain separate ABI contracts and must reject until represented.

## Generated entry origins and nonvirtual subobjects: scalar-control-v18

An entity is either an original source routine or an explicitly owned generated
ctor/dtor entry. `abi_routine_origins` relates the generated entity directly to
its original primary entity, records complete/subobject/deleting entry kind,
alias evidence and the observed `primary-ctor-dtor-link` proof. Generated entries
have no fabricated `source_functions` or source declarations. Original routines
append `function_kind` (`ordinary`, `constructor`, `destructor`). A generated
receiver is proved by its generated scope; `forwarded-source` parameters map to
primary written ordinals. Those parameters and generated locals are synthetic.
Execution routine types and bodies remain authoritative, including deleting
entry bodies even when alias evidence is present. The initial portable ABI
admits only void-return primary/alternate pairs, without variadic forwarding,
hidden result buffers, VTT or delegation/returns-this variants.

`source_record_bases` retains original direct base ordinals, complete source
types, access and virtual facts. `record_types.complete_type_id` distinguishes
observed exact-sized subobject representations from their complete nominal type.
Subobject views have explicit-only standalone copy policy and retain native
alignment independently of the CLR packing needed for their exact extent.
Original source sizeof/alignment facts always describe the complete type.
Physical `base` fields refer to original base IDs instead of fabricated source
fields. `record_base_subobjects` connects each owner and original base to its
complete base type, offset, exact extent and optional physical view/field.
An optimized empty base has zero extent and no physical view or field: its
complete empty class still has its normal nonzero sizeof and pointer stride.

Complete-object copy permission still requires the original trivial-copy fact.
Physical payload recursion may enter a validated base view without thereby
making standalone view assignment or parameter/return passing valid. A typed
`block-copy` node expresses EDG's lowered memcpy-like operation: void result,
positive exact source byte extent, source/target place edges, sufficient target
extent and retained evaluation order. It never substitutes a whole complete
record assignment for a partial base copy. Source and destination addresses
are evaluated once; volatile/VLA/array-rvalue cases remain explicit boundaries.

This tranche represents direct nonvirtual base relationships first. Virtual
bases, ambiguous runtime casts, broader copy-construction/return ABI, unwinding
and complete independent source semantic indexing remain required follow-on
work rather than inferred properties of these physical representations.


Void conditional expressions retain two void branches and lazy evaluation.
The managed emitter expands only these incompatible expression forms to local
structured `if`/`else`, preserving their execution position and loop behavior.

Address constants may name a declaration of an incomplete external array.
Object verification retains the nonnegative displacement without pretending
that a zero declared size is a definition. Linking checks the displacement
against the selected complete definition, or records a minimum required extent
for an explicitly allowed external storage provider. Runtime requirements expose
`size` (minimum required extent), `declaredSize` and `extentKnown`; a provider
must own the actual aligned addressable cell. This does not synthesize missing
RTTI vtables or permit out-of-range displacements into known definitions.

Storage with EDG's authoritative COMDAT group is represented as `linkonce`,
following the stock C backend. Independent object selection must preserve one
canonical cell and its address; duplicate initializers do not create per-unit
copies. Source-only metadata remains independent of this execution linkage fact.

Generated constructor wrappers that supply default arguments remain a separate
origin boundary in v18. They are not mislabeled as complete/subobject aliases.
Their future contract must retain the exact selected call target, written
parameter default-presence facts and the helper's own execution exception policy.

## Default-argument constructor adapters: scalar-control-v19

`source_parameters.has_default_argument` retains an original parameter's default
presence as a declaration fact. It is not part of source routine type identity
or cross-unit callable signature equality. Original default-expression trees
are not claimed by this boolean; the executed default expressions remain in the
owned generated helper body.

`abi_routine_origins` adds `constructor-default-adapter` with
`observed-default-wrapper-creation` proof and a required `target_entity_id`.
The target is the original constructor or a directly observed nondeleting
alternate of that same primary. Older complete/subobject/deleting entries have
no target column value. An adapter is a distinct private entity, never merged
solely by primary and entry kind, and has an owned body without fabricated
source declarations. Its actual receiver representation is proved at creation.
Any forwarded explicit parameters map a source prefix; omitted parameters must
have recorded defaults. The initial producer supports receiver-only adapters
and rejects implied/VTT, initializer-list and copy adaptation forms.

The helper's execution exception policy remains independent of its target:
default expressions may throw before entering a noexcept constructor. Inlining
may remove the direct target call from its body, so verification relies on the
observed creation relationship and typed body rather than demanding a particular
syntactic call. Runtime array construction still requires real callback and
partial-construction cleanup implementations.


### Nominal identity and entry arguments

C++ specialization identity copies the frontend's finalized mangled name at the
final frontend boundary. It never invokes name mangling against mixed lowered
IL. Original namespace and template arguments remain part of this identity.
Unpromoted function-local classes use the retained original owning routine,
frontend declaration discriminator and named ancestry. External inline owners
share their types across units; private owners additionally carry the explicit
object unit identity. Anonymous namespace classes remain private even when the
same header location and layout occur in multiple units. Unsupported unnamed
or closure identities fail explicitly. Original C record identity is unchanged.

The frontend invocation receives `EDG_DOT_OBJECT_UNIT_ID`, captured in provenance
and checked against the packaging unit ID. This is an owned compilation identity,
not a guessed current directory or source-header path.

Executable entry accepts nonvariadic `void()`, `int()`, or exactly
`int(int, char**)`. The latter uses the shared `ProgramArguments` implementation:
one owned native allocation holds writable UTF-8 strings and the null-terminated
pointer table. Its lifetime encloses `ObjectProgram.Run`, including source normal
exit callbacks. It is disposed only after that boundary returns to the host.


## Original routine ABI mappings: scalar-control-v20

`object_info.source_language` owns the translation-unit language (`c` or `c++`),
also required as top-level `sourceLanguage` in the producer wire. Every original
source routine type, including callback-only and zero-parameter types, has one
`source_routine_abis` header. The header records `return_passing` (`direct` or
`result-buffer`) and `capture_proof` (`source-c-signature` or
`observed-routine-lowering` or `observed-runtime-signature`). The first proof is valid only for original C direct
signatures. C++ uses observed routine-lowering facts; an unobserved signature does
not receive an inferred observation merely because its physical types look simple.

`source_routine_abi_parameters` records each physical execution ordinal, role
(`source`, `indirect-source`, `this`, `result-buffer`) and original source ordinal
where applicable. This map is keyed by original source routine type. It cannot
be keyed only by the canonical physical routine type: a source class return with
a result buffer can have the same physical signature as an unrelated ordinary
pointer-taking function. The exact observed receiver/result order is retained;
a same-class receiver and result pointer cannot be distinguished by type alone.

Original indirect-value parameters keep nonsynthetic source class declarations
and their original names. Only the execution binding gains a pointer layer.
The result buffer and receiver are synthetic only through their exact observed
creation identities. An NRVO local is not proof of an ABI result parameter.
Entity parameter rows must match the verified source routine map. Generated
forwarded-source entries map their primary's verified physical slot by original
source ordinal; default-argument declaration facts remain separate.

A result-buffer return has a physical void result and exactly one pointer slot
to the original class result representation. Initial support does not permit
arbitrary scalar or reference values to masquerade as indirect class values.
Source signature compatibility includes return passing and ordered roles while
excluding observation proof and TU language, so compatible direct C/C++ header
declarations remain linkable. Original source qualifiers and nominal identity
retain their existing compatibility rules.

The mapping does not create extra object copies, infer destructor ownership, or
discharge cleanup obligations. Existing typed lowered calls and construction
operations target actual owned destinations. Unrepresented destruction, EH, VTT,
returned-this and variadic adaptation remain explicit boundaries until separately
implemented. Full DuckDB execution is not an acceptance claim of this contract.


Compiler-created external runtime declarations use `function_kind=runtime` and
`capture_phase=runtime-declaration`, distinguished from original before-lowering
source functions. This requires observation of the new runtime routine after its
prototype is complete, with external linkage and no owned definition. A reused
original source prototype retains its original classification. Callback types
created by the runtime-helper factories have their own finalized creation proof.
`observed-runtime-signature` permits only direct return/source-only parameter
slots, with no invented receiver, result buffer or copy indirection. The source
index records this as a generated declaration, never as an original user function
or fabricated parameter variable declaration.


## Observed packing and memory access: scalar-control-v21

Original records append `is_packed`, `max_member_alignment` and
`explicit_alignment`; original fields append `is_packed` and
`explicit_alignment`. Zero denotes no requested maximum/alignment directive.
Requested facts are captured before layout normalizes them, separately from
resulting native size/alignment. Incomplete declarations may lack facts supplied
by a later complete definition; complete cross-unit definitions compare these
original facts as well as their physical layouts.

Physical fields append nullable `observed_alignment`, retained from the actual
layout computation with ordinary-field semantics. The exporter never reruns a
layout helper that may diagnose or normalize directives. Original non-bitfield
members require this observation; copied fields inherit it only through their
proven original-field origin. Bitfields bypass that layout helper and have null
observations: their existing byte-backed representation is non-addressable and
alignment-one-safe. Generated vptr/base/ABI fields may also have null when no
such observation occurred. Null never means natural alignment or alignment one.

Named member accesses preserve their field identity and use ordinary C# member
syntax with explicit offsets and the correct CLR packing metadata. A missing
`unaligned.` IL prefix is not evidence that the JIT ignores field layout.
Packed record values likewise use their declared layout; the maximum natural
alignment of their scalar children is not a separate requirement for every
record copy.

Raw pointer accesses can lose that field context, for example after a packed
array member decays to a scalar pointer. Places derive address guarantees from
storage or owners, field offsets and array strides. Only such raw accesses with
insufficient guarantees use explicit unaligned reads or writes. Pointer values
use native integer bits where generic APIs cannot accept pointer type arguments.
Address and RHS evaluation retain the owned graph order and occur once.
Unsupported atomic/volatile accesses remain explicit boundaries until their
alignment guarantees and platform behavior are represented and verified.


## Empty GNU memory assembly: scalar-control-v22

An owned `memory-barrier` statement represents an observed empty GNU extended
assembly template with only `m`, `=m` or `+m` operands and an optional `memory`
retained semantic clobber. `memory_barriers` retains effective/explicit volatility, the inline
keyword, memory clobber and capture proof. Nonempty templates, register or
alternative constraints, other retained clobbers and asm-goto remain rejected.
The frontend explicitly diagnoses and ignores the `cc` clobber in this
configuration; the object follows that accepted semantic decision rather than
reintroducing a discarded clobber. Original source snapshots and diagnostics
retain its spelling.

`memory_barrier_operands` retains original ordinal, input/output/inout direction,
exact constraint, optional source name, element source type/size, source location
and object/fixed-array/VLA-array shape. Ordered `memory_barrier_dimensions` rows
preserve fixed dimensions or a rank-one variable bound. Each operand owns an
`address` edge and an `extent` edge with the same ordinal; these are typed graph
expressions, not rendered text or offsets into a source fragment.

The observer records original operand shape before `lower_asm_statement` lowers
its expression. For a VLA it then observes the existing synthetic ptrdiff binding
holding EDG's evaluated total element count. The lowered address expression
retains the bound's original side effects; its extent is an unsigned conversion
of that saved binding multiplied by the original leaf size. The original bound
expression is never copied or evaluated again. Fixed extents equal the checked
product of dimensions and leaf size; zero fixed dimensions retain zero extent.
General VLA storage and higher-rank variable shapes are not implied by this
bounded operand contract.

Managed verification checks metadata/edge ownership, source element size,
dimension products and exact saved-binding extent expression. Emission evaluates
each operand's address and then extent once, in original operand order, before a
BCL `Thread.MemoryBarrier`. It never reads or writes through operand addresses
merely to model a memory constraint. This is a conservative hardware fence, not
a claim of equal cost to the native compiler-only barrier or an independent
secure-zeroization guarantee. Existing calls that clear memory remain real calls.


Alignment analysis distinguishes unknown pointer origins from observed storage
alignment. An opaque `void*` or byte-pointer parameter/load does not by itself
prove that a later typed access is underaligned. Defined source accesses follow
the destination type's alignment contract. Observed packed addresses remain
constrained through casts; typed pointer contracts survive erasure to byte/void
representations before arithmetic; conditional joins retain unknown paths rather
than letting an unrelated aligned branch strengthen every possible address.


## Fixed zero-bound arrays: scalar-control-v23

Execution and source array types distinguish `element_count=0` (a complete
fixed zero-bound array) from SQL NULL (an unspecified bound). This follows EDG's
observed `bound_is_zero` flag; both representations may have a numeric bound of
zero in the frontend. Type identity, cross-unit compatibility and semantic
provenance retain the distinction, including nested arrays and array pointers.

Zero-bound fields retain exact zero extent, offset and alignment inside otherwise
representable structs/unions; they may overlap a following field. Genuine
incomplete/flexible fields remain limited to the final nonunion member. Record
copies and initialization do not invent storage for empty fields. C# uses an
address-only leaf-pointer view instead of a phantom `InlineArray(1)` wrapper;
owned type identity remains available independently of that execution view.

Pointer addition/subtraction by an integer and pointer mutations with zero-sized
elements retain zero stride and evaluate their source operands once. This also
applies to compound assignment and pre/post increment through supported volatile
or unaligned places. Pointer difference with zero-sized elements and arithmetic
on incomplete arrays remain explicit unsupported boundaries. Standalone zero-size
storage, all-zero records and zero-size aggregate values are not implied by this
bounded field/address contract and remain gated rather than receiving fabricated
physical bytes.


## Observed C++ file-scope initialization: scalar-control-v24

Generated file-scope initializer routines have explicit `generated_routine_origins`
rows. Their execution ABI is an actual owned `void()` routine; they do not acquire
fabricated original source-function declarations or source signatures. Observation
hooks at stock initializer creation and insertion record the relationship rather
than guessing generated name prefixes. The generated origin carries the observed
creation proof and `terminate` failure policy, separate from routine noexcept ABI.

Existing `initialization_actions` schedules these routines with the priority and
order provided by stock lowering. `dynamic_initializers` maps each original
source storage to its action, original pre-partition source ordinal, action-local
ordinal and source location. Ordered `dynamic_initializer_statements` identify
exact contiguous direct-body statement intervals for each original initializer.
No-op intervals may be empty. `generated_routine_scaffolds` separately identifies
the direct-body prefix of observed local declarations and their immediate pure
zero initializations. Together the ranges and scaffold prefix account for every
direct statement in the generated routine exactly once. Shared temporaries are
not falsely attributed to a particular source storage initializer.

The managed reader validates this exhaustive ownership, actual local bindings,
dense ordinals, statement order and pure scaffold shape. Linking consumes the
owned bodies and scheduling metadata after source removal. All selected static
storage is allocated and its zero/constant initialization installed before any
action executes. Priority, link input order and original action order determine
the existing scheduler sequence; source line sorting is not used. Exceptions
escaping a C++ file-scope initializer terminate at its dispatch boundary;
explicitly classified non-C++ runtime transfers retain their existing behavior.
The current boundary uses the backend's existing `Environment.FailFast` policy;
custom `std::set_terminate` handler dispatch remains a separate runtime gap.

GNU lowering can leave an already-consumed `dik_none` entry on the original
file-scope list. Only entries whose exact lowering was observed, or definitions
stock lowering suppresses, are accepted as residuals. This does not imply support
for unowned file-scope assembly, dynamic TLS, guarded inline/template global
initialization or destruction obligations. Those remain explicit feature gates.
Function-local initialization guards are a separate publication/lifetime contract;
plain generation of acquire/release calls alone is not a concurrency guarantee.


## Owned scalar C++ destruction and temporary lifetimes: scalar-control-v25

`source_lifetimes` and `source_destruction_obligations` retain the original
function's pre-lowering lifetime tree, sibling list order, destruction-list order
and exact parent destruction sublist relation. The function root is an observed
block lifetime with no parent destruction sublist; a null or global-static outer
context becomes the explicit boundary of this per-function graph. Other unowned
outer contexts are rejected. ABI constructor/destructor copies retain their exact
original primary owner and observed copy correspondence, rather than acquiring
fabricated source lifetimes.

`cleanup_actions` owns each exact execution destructor callback and a typed target
expression, mapped to its original destruction obligation. `cleanup_states`
retains the acyclic predecessor chain. `cleanup_transitions` owns the exact
state-setting expressions, including transitions inside comma expressions.
Unreachable-state annotations do not update the cursor. Separate `cleanup_frame_markers`
retain observed prologue/epilogue annotations; those are not invented resets.
Every captured destruction obligation must be represented or explicitly rejected.
`normal_destructions` also maps exact observed normal-path destructor call nodes
to those obligations. Completeness uses the union of these calls and unwind
actions, preserving no-exceptions source code without inventing CLR unwind work.
Call provenance comes from the stock destruction insertion context, not matching
names or source positions. Disappeared/inlined calls remain gated until their
exact replacement ranges are owned.

Target expressions are restricted pure recipes over verified storage, typed
member/base places and constant pointer adjustments. They do not serialize native
stack offsets or executable text. Captured locals must be declared at each actual
arming transition. Exact original-primary versus complete/subobject destructor
relations are checked independently of pointer ABI compatibility. Optimized empty
bases use address-only `base-subobject` nodes with `base_subobject_expressions`
source identity, resolving the existing observed zero-extent base row. No field,
physical byte or complete-record value load is invented for that view.

An `expression_lifetimes` row connects a transparent `lifetime-scope` expression
to its exact original temporary lifetime, including observed expression-tree
copies. Its operand has identical type and value category. The boundary remains
in the semantic graph and source map; emission forwards its expression/place
without adding cleanup or resetting state. Existing transitions already own
execution. Empty lifetime trees may be retained by boundaries without introducing
a runtime cleanup frame.

Emission preserves EDG's explicit normal-path destructor calls. Functions with
owned actions use named locals, hoisting only bindings needed by unwind code.
A first-pass filter marks exceptional exit; a finally dispatcher follows the
owned state chain and advances before invoking each callback. Cleanup failure
uses a first-pass termination filter. Cleanup eligibility is distinct from the
runtime-transfer classifier, so dedicated thread-exit transfers are not silently
excluded from destruction merely because they bypass noexcept termination.
The current termination policy is FailFast, not custom std::set_terminate dispatch.

The initial contract covers scalar locals, members and ordinary base rollback.
Arrays, conditional cleanup, virtual-base/VTT actions, deallocation/guard actions
and source throw/catch/type matching remain explicit gates. Ordinary CLR host
exceptions verify unwinding; they are not evidence of C++ exception matching.
The historical general `lifetimes` capability remains false; the bounded source
lifetime/state capabilities describe this implemented subset.

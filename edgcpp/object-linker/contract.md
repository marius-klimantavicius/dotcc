# Initial owned semantic object contract

This is the first executable profile of the fresh design, not the full C++ graph.
No code or schema from the prior semantic-export experiment is used.

Native stdout wire is one JSON object (transient packaging protocol only):

```
{
  "formatVersion": 1,
  "profile": "scalar-control-v7",
  "sourceFiles": [{"id":1,"path":"/absolute/main.cpp","parentId":null}],
  "macroDefinitions": [{"id":1,"name":"LIMIT","form":"object","variadic":false,"dynamic":false,"definitionText":"#define LIMIT 4","origin":"source","sourceFileId":1,"line":1,"column":9}],
  "macroParameters": [],
  "macroEvents": [{"id":1,"kind":"define","name":"LIMIT","definitionId":1,"origin":"source","sourceFileId":1,"line":1,"column":9}],
  "types": [{"id":1,"kind":"void","pointeeTypeId":null},{"id":2,"kind":"i32","pointeeTypeId":null},{"id":3,"kind":"u8","pointeeTypeId":null},{"id":10,"kind":"pointer","pointeeTypeId":2}],
  "sourceTypes": [{"id":1,"kind":"int","sourceName":"","underlyingTypeId":null,"executionTypeId":2,"isConst":false,"isVolatile":false,"sourceFileId":null,"line":0,"column":0}],
  "sourceFunctions": [{"entityId":1,"returnSourceTypeId":1,"capturePhase":"before-lowering"}],
  "sourceParameters": [{"entityId":1,"ordinal":0,"sourceTypeId":1}],
  "sourceDeclarations": [
    {"id":1,"kind":"function","sourceName":"add","sourceTypeId":1,"entityId":1,"bindingId":null,"functionEntityId":null,"sourceFileId":1,"line":1,"column":1},
    {"id":2,"kind":"parameter","sourceName":"x","sourceTypeId":1,"entityId":null,"bindingId":1,"functionEntityId":1,"sourceFileId":1,"line":1,"column":1}],
  "entities": [{"id":1,"sourceName":"add","linkage":"external","externalKey":"exact EDG linker symbol","definition":"strong","returnTypeId":2,"sourceFileId":1,"line":1,"column":1}],
  "parameters": [{"entityId":1,"ordinal":0,"typeId":2}],
  "functions": [{"entityId":1,"bodyNodeId":10}],
  "bindings": [{"id":1,"functionId":1,"kind":"parameter","ordinal":0,"typeId":2,"sourceName":"x","scopeNodeId":10,"sourceFileId":1,"line":1,"column":1}],
  "nodes": [{"id":10,"functionId":1,"kind":"block","typeId":null,"entityId":null,"bindingId":null,"operator":null,"constantText":null,"valueCategory":null,"evaluationOrder":null,"sourceFileId":1,"line":1,"column":1}],
  "edges": [{"parentId":10,"role":"statement","ordinal":0,"childId":11}]
}
```

All fields shown on each record are required (nullable values explicitly null).
IDs positive signed int64; ordinals/line/column nonnegative. Entity definition is
`strong` or `declaration`; linkage external (nonempty exact externalKey) or
internal (externalKey=null). Unique local entity/binding/node IDs; no serialized EDG memory addresses.
sourceFiles contains include occurrences; parentId nullable root; sourceFileId
nullable only for synthetic/unknown origins. Constants are exact decimal text.
Function bindings exist only for defined functions, parameters describe all
signatures. `scopeNodeId` references the declaring block (parameters root block).
Function body must be block. Locals use ordinal=null; parameters zero-based.

Supported node kinds and ordered edge roles:

- `block`: statement[0..n).
- `return`: value[0] optional for void only.
- `expr-stmt`: value[0].
- `declare`: bindingId + initializer[0] optional.
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
  require matching promoted i32/u32/i64/u64 operand/result types. Shifts have
  matching promoted left/result types and any promoted integer right type.
  Comparison operands have matching types (bool only allows eq/ne); comparison
  and logical results are bool/i32/u8. and/or short-circuit their right operand.
- `unary`: operator neg/not/plus/bit-not, operand[0]. Negation, unary plus and
  complement require matching promoted integer operand/result types. Logical
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
- `mutate`: operator pre-inc/post-inc/pre-dec/post-dec, operand[0] (integer
  or nonvoid pointer place), result type equals operand type. Prefix returns the new value;
  postfix returns the previous value. Unsigned mutation wraps at its width.
  Source bool mutation is rejected by the producer before physical u8 lowering
  loses its distinction; graph bool mutation is rejected by the verifier.
- `convert`: operand[0], destination integer/bool/pointer type; explicit
  width/sign conversion, nonzero truth conversion, pointer-to-pointer cast,
  or integer/pointer conversion. A graph bool-to-pointer conversion requires
  an intermediate integer conversion. Source target pointer width is 64 bits.
- Pointer binary operators are `pointer-add`, `pointer-subtract` and
  `pointer-difference`. Addition has one nonvoid pointer and one promoted integer
  operand, preserving their original left/right order; subtraction has a pointer
  on the left and promoted integer on the right. Their result is that pointer
  type. Difference has matching nonvoid pointer operands and i64 result.
  Displacements count elements, not bytes; difference divides by pointee size.
  Source object bounds and related-pointer rules still apply. Pointer comparison
  operands have matching execution types after EDG's explicit conversions.

Statements have typeId=null; expression nodes must name a type. Only `call`
may have void type and only in expression statements, for initializer/step,
or void-return-as-allowed contexts.
No payload for an unrelated kind; no extra edges/fields silently ignored.
Expression occurrences are not shared: each executable node has one parent,
except a function root. Scope/target edges are metadata, not evaluation edges.
Loop condition/body/step roles encode execution and sequencing; database edge
ordering is not itself an execution schedule. Initializer executes once, a for
condition precedes each body iteration, and step follows each completed body.
The structured scope profile excludes arbitrary jumps,
references, globals/TLS, records, arrays, function pointers, EH/RAII, inline/COMDAT and runtime
providers. Unsupported constructs fail export rather than emit partial output.

Packager stores typed rows using schema.sql, including producer compiler/config,
original source/include bytes and exact arguments. Only one original TU is sent
to EDG; no .ii preprocessing subprocess or semantic merge. Objects advertise
sourceDeclarations=true, macroDefinitions=true, prelowerSource=false,
macros=false, lifetimes=false.
The first flag covers only the scalar declaration/type provenance below; a full
source expression/statement view is still absent.
These explicit limitations must survive the link report; fixtures do not claim
full source-faithful C++ graph or DuckDB readiness. Namespace/class are link args.
The initial target is portable-lp64-binary80-v2; callable ABI dotobj-scalar-v1.
The scalar-control-v3 object profile adds 8/16/32/64-bit signed/unsigned
integers, bitwise/shift expressions and conditional values to v2's structured
loops and explicit loop exit identities. It does not change the callable ABI.
The current canonical schema reader requires scalar-control-v7 and rejects
v1/v2/v3/v4/v5/v6 objects; preserve their respective earlier tool build to read them,
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
plain binding assignment needs only RHS-before-write; effectful indirect
right-to-left assignments are rejected by this profile. Unsupported ordered expressions fail explicitly. u8 represents
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
1 through 9 retain the producer's prior meanings; pointer IDs begin at 10.
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

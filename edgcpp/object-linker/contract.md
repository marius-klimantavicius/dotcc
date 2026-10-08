# Initial owned semantic object contract

This is the first executable profile of the fresh design, not the full C++ graph.
No code or schema from the prior semantic-export experiment is used.

Native stdout wire is one JSON object (transient packaging protocol only):

```
{
  "formatVersion": 1,
  "profile": "scalar-control-v3",
  "sourceFiles": [{"id":1,"path":"/absolute/main.cpp","parentId":null}],
  "types": [{"id":1,"kind":"void"},{"id":2,"kind":"i32"},{"id":3,"kind":"bool"},{"id":4,"kind":"u8"}],
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
internal (externalKey=null). Unique local entity/binding/node IDs, no pointers.
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
- `break`, `continue`: target[0] is a non-owning metadata reference to the nearest
  lexically enclosing while/for/do-while node. The verifier requires that exact
  identity; an exit outside a loop or targeting another loop is invalid. No
  arbitrary jumps or switch exits are supported.
- `constant`: integer or bool type + canonical exact decimal constantText.
  Integer types i8/u8/i16/u16/i32/u32/i64/u64 have their corresponding signed
  or unsigned width ranges; bool permits only 0/1. No leading plus, leading
  zeroes (except zero itself), whitespace or negative zero. Unsigned 64-bit
  values remain text, including values beyond SQLite's signed INTEGER range.
- `binding`: bindingId and matching type. Denotes scalar place; consuming
  expression operations read its current value; assignment and mutation write it.
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
- `assign`: target[0] (binding node), value[0], result type target type.
- `mutate`: operator pre-inc/post-inc/pre-dec/post-dec, operand[0] (integer
  binding), result type equals operand type. Prefix returns the new value;
  postfix returns the previous value. Unsigned mutation wraps at its width.
  Source bool mutation is rejected by the producer before physical u8 lowering
  loses its distinction; graph bool mutation is rejected by the verifier.
- `convert`: operand[0], destination integer/bool type; explicit width/sign conversion or nonzero truth conversion.

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
references, globals/TLS, records, pointers, EH/RAII, inline/COMDAT and runtime
providers. Unsupported constructs fail export rather than emit partial output.

Packager stores typed rows using schema.sql, including producer compiler/config,
original source/include bytes and exact arguments. Only one original TU is sent
to EDG; no .ii preprocessing subprocess or semantic merge. Objects advertise
prelowerSource=false, macros=false, lifetimes=false until those captures exist.
These explicit limitations must survive the link report; fixtures do not claim
full source-faithful C++ graph or DuckDB readiness. Namespace/class are link args.
The initial target is portable-lp64-binary80-v2; callable ABI dotobj-scalar-v1.
The scalar-control-v3 object profile adds 8/16/32/64-bit signed/unsigned
integers, bitwise/shift expressions and conditional values to v2's structured
loops and explicit loop exit identities. It does not change the callable ABI.
The current canonical schema reader rejects v1/v2 objects; preserve their
respective earlier tool build to read them, or recompile with this producer.
The profile uses EDG's fixed portable LP64 target: long and long long share the
64-bit physical representation, while exact external keys retain C++ overload
identity. Source enum/typedef spellings are not yet captured as owned types.

Expression valueCategory is place for binding and value otherwise; statements
use null. evaluationOrder is null for statements, none for constant/binding,
and left-to-right/right-to-left/unspecified for operations. Unspecified permits
the emitter to choose left-to-right; do not change a specified order. The
initial assignment destination is a plain binding, so RHS-before-write honors
right-to-left. Unsupported ordered expressions fail explicitly. u8 represents
EDG's lowered unsigned-byte bool and requires numeric 0/1 comparison results
unless expression result type is bool. Explicit conversions encode truth and
width behavior. Parameters/locals/signatures may use any supported integer or
bool type, never void. C# integer literals preserve signedness and width;
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

Required nodes_by_function(function_id,id) and bindings_by_function(function_id,id)
indexes make per-function graph loading indexed; index definitions are validated
separately from the logical table-content digest. Changing physical indexes must
not change program identity, but an unsupported schema object is rejected.

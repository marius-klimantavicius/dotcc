# Initial owned semantic object contract

This is the first executable profile of the fresh design, not the full C++ graph.
No code or schema from the prior semantic-export experiment is used.

Native stdout wire is one JSON object (transient packaging protocol only):

```
{
  "formatVersion": 1,
  "profile": "scalar-control-v2",
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
- `constant`: type i32/u8/bool + constantText (u8 0..255, bool 0/1).
- `binding`: bindingId and matching type. Denotes scalar place; consuming
  expression operations read its current value; assignment and mutation write it.
- `call`: entityId + argument[0..n), type matches target return.
- `binary`: operator add/sub/mul/div/rem/eq/ne/lt/le/gt/ge/and/or;
  left[0],right[0]. and/or are short-circuit boolean operations.
- `unary`: operator neg/not/plus, operand[0].
- `assign`: target[0] (binding node), value[0], result type target type.
- `mutate`: operator pre-inc/post-inc/pre-dec/post-dec, operand[0] (i32/u8
  binding), result type equals operand type. Prefix returns the new value;
  postfix returns the previous value. Unsigned byte mutation wraps at 8 bits.
  Source bool mutation is rejected by the producer before physical u8 lowering
  loses its distinction; graph bool mutation is rejected by the verifier.
- `convert`: operand[0], destination type i32/u8/bool; width conversion or nonzero truth conversion.

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
The scalar-control-v2 object profile adds structured loops and explicit loop
exit identities; it does not change the callable ABI. The current canonical
schema reader rejects scalar-control-v1 objects; preserve their earlier tool
build to read them, or recompile from source with the current producer.

Expression valueCategory is place for binding and value otherwise; statements
use null. evaluationOrder is null for statements, none for constant/binding,
and left-to-right/right-to-left/unspecified for operations. Unspecified permits
the emitter to choose left-to-right; do not change a specified order. The
initial assignment destination is a plain binding, so RHS-before-write honors
right-to-left. Unsupported ordered expressions fail explicitly. u8 represents
EDG's lowered unsigned-byte bool and requires numeric 0/1 comparison results
unless expression result type is bool. Explicit conversions encode truth and
width behavior. Parameters/locals/signatures may use i32/u8/bool, never void.

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

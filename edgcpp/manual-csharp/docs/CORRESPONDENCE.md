# EDG algorithm correspondence

This ledger maps bounded handwritten routines and invariants. It does not claim
that any whole EDG source file is ported. Exact consulted working-file SHA-256
values and source baseline are in `../evidence/source-baseline.json`; reviewer
source inspections and additional branch references are in the review reports.
No generated EDG C# implementation is an input to this campaign.

## Produced foundations, awaiting integrated acceptance

| Authored component | EDG source/routines | Preserved invariant and managed adaptation | Outstanding branches |
| --- | --- | --- | --- |
| `Source.cs` source buffers/ranges | `lexical.c` source-input and error-position contracts | One immutable source string owns physical half-open ranges; line starts handle CR/LF/CRLF; public positions count UTF-16 code units explicitly | Include/macro expansion provenance, logical line remapping, Unicode/encoding phases |
| `Tokens.cs` token buffer/cursor | `lexical.c:get_token`, `terminate_token_cache`, `rescan_cached_tokens`, `rescan_reusable_cache` | Compact value tokens retain source ownership; longest punctuation forms win; independent cursors and retained intervals preserve token identity/ranges; EOF is stable; checkpoints reject another cursor | Macro expansion, nested rescan-stack semantics, parser-state transactions and actual template replay/instantiation; lexical checkpoints alone restore no semantic state |
| `Types.cs` session type table | `types.c:type_after_integral_promotion`, canonical/compatible type routines; `exprutil.c:determine_arithmetic_conversions_full` | Canonical integer and function types belong to one session; aliases refer to underlying types; rank and width are distinct; promoted signed/unsigned types are selected by rank then representability; target conversion applies explicit modulo width | Floating types/conversions, references, pointers, composite/class/dependent types, general qualification rules and target layouts |
| `SemanticGraph.cs` declarations/scopes | `il.h` declaration/expression relationships; `scope_stk.c`; `symbol_tbl.c:link_symbol_into_symbol_table`, `add_symbol_to_scope_list` | Managed symbol reference identity is separate from declaration identity; declarations remain in source order; lexical lookup walks innermost scope outward; read-only views retain graph facts | Active/inactive/other-symbol distinctions beyond lexical subset, namespaces/classes/ADL, overload sets, dependent lookup and scope activation |
| `Parser.cs` declaration/alias/function paths | `decls.c:declaration`, `scan_nonmember_declaration`, `alias_declaration`, `enter_local_symbol`; `func_def.c:function_definition` | Declarator insertion precedes initializer analysis; aliases retain their declaration while using canonical underlying types; compatible function redeclarations share a symbol; each declaration retains its own parameter scope and locations; a definition symbol is entered before its body is analyzed | Compound declarators, pointers/references, overload sets, default arguments, storage/linkage specifiers and general declaration ambiguity |
| `Parser.cs` blocks and returns | `statements.c:compound_statement_full`, `return_statement`; `expr.c:scan_return_expression` | A function body's outer block reuses parameter scope; nested blocks activate a child scope and restore parent afterward; return expressions bind/convert under the current function while parsing | Control flow, labels, exceptions, declarations requiring deferred processing and general flow analysis |
| `Expressions.cs` operand/operator/call paths | `expr.c:token_ends_expr`, `scan_expr_full`, `scan_identifier`, `scan_call_arguments`, `scan_function_call`; `exprutil.c:determine_arithmetic_conversions_full` | Scan an operand, consume postfix calls and recurse according to precedence/left associativity; immediately resolve identifiers and construct operand conversions and result types; calls select the visible single function symbol, then convert arguments | Assignment, logical/shift/conditional/comma operators, overload resolution, pointers/member access, casts and dependent expressions |
| `Expressions.cs` integer literals/folding | `literals.c:conv_integer_literal`; `const_ints.c` width/overflow contracts | Full number spelling is retained; radix/suffix choose an ordered target type candidate set; arithmetic uses explicit target widths; unsigned values wrap and signed overflow gives no folded constant | Extended integer types, floating/character/string/user-defined literals and constant-required evaluation |
| `ParseState.cs` checkpoints | `lexical.c:push_lexical_state_stack`, `pop_lexical_state_stack`; `scope_stk.c` scope activation contracts | Managed adaptation journals entered symbols, declaration lists, scope children and definition links; checkpoints save cursor, scope/current function, diagnostics and pending replay queue; registered checkpoint identity invalidates abandoned descendants before mutation | Owner-mediated state only; production speculative parser branches and template instantiation remain outstanding. R2 stale-branch regressions pass final managed/AOT checks; definition-link/current-function branches are source-inspected only |

The lexical scope dictionary is an intentional bounded host-container adaptation.
It is valid only while one active entity per name per lexical scope is supported.
It does not replace EDG's general ordered symbol lists or establish namespace,
class or overload correctness. No lookup outcome may depend on dictionary
enumeration order. Full parser transactions, when introduced, must restore
symbol insertion, scope/children, diagnostics, pending work and semantic flags.

The managed expression graph deliberately exposes conceptual integral promotion
nodes before conversion to a wider common arithmetic type. This is a richer
representation, not a claim of identical EDG cast-node shape. EDG
`exprutil.c:determine_arithmetic_conversions_full` (11876–11884) computes promoted
types without mutating operands, then `change_binary_operand_types`
(13583–13628) casts directly to the common type; `expr.c` uses those steps at
31753–31755. Managed intermediate nodes preserve the chosen types/values while
making both conceptual steps queryable.

`Parser.FunctionDeclaration` additionally follows
`declarator.c` (3481–3528) for a sole unnamed alias resolved to unqualified void:
it contributes no parameter and canonicalizes with `()`. Named void and void
mixed with other parameters diagnose invalid input. Scalar call expression
qualification follows `il.c:prvalue_type`/`il_return_type_of`; a declared const
scalar return remains in the function type while the call prvalue is unqualified.
Global `main` function/variable semantics are explicitly excluded with unsupported
diagnostics rather than accepting them through ordinary declaration paths.

## Native baseline witnesses

`tools/oracle-inputs/profile.cpp` establishes the native C++20 profile and LP64
integer/pointer sizes, 8-bit unsigned char, signed plain char and bool size.
The native executable SHA-256, exact arguments, input SHA-256 and output are in
`evidence/native-profile.json`. The adapter disables the predefined macro file;
it does not claim real-header compatibility or a selected external target ABI.

`positive.cpp` and `unresolved.cpp` establish acceptance and a negative control.
The preprocessing receipt currently covers macro-free input only. It is not
evidence for macro expansion or includes in the managed frontend.

`semantic-witness.cpp` includes exactly hashed `positive.cpp` and uses native
static assertions to establish `Number` is `int`, ordinary `add`/`caller` call
results are `int`, and the following conversion result types:

- `long + long long` is `long long`.
- `unsigned int + long` is `long`.
- `unsigned long + long long` is `unsigned long long`.

Those witnesses do not establish graph identities, declaration locations,
conversion-node structure, or managed behavior. Managed source-text tests and
independent code review must establish those separately.

`m2-semantic-witness.cpp` includes the exact hashed managed `m2-positive.cpp`
fixture and establishes `Count=long`, `widen`/`entry` call results `long`, and
`widen`'s function type `long (*)(int)` despite a const definition parameter.
`review-boundaries.cpp` separately establishes native acceptance for alias-void
empty parameter declarations, directives within declaration/parameter/body
positions, and direct initialization. Those unimplemented preprocessing/direct
initialization branches are diagnosed unsupported by managed analysis.

## Pending integration mapping

The independently accepted stable snapshot has a successful Release build,
25/25 managed test groups, and 25/25 groups under a published Linux x64 NativeAOT
executable. Exact commands, output and matching 16-file implementation/test/
fixture hashes are retained in `evidence/managed-*.json` and
`evidence/nativeaot-*.json`. The bounded approval is
`evidence/review-final-m0-m2.md`; no row alone establishes broader compatibility.

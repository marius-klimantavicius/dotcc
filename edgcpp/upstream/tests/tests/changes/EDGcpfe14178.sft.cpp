//type:fp
//options_all:--g++
//remark:[4.10] GNU statement expressions with class type results (IL CHANGE)
// 10/7/14  [EDGcpfe/14178,EDGcpfe/14813,EDGcpfe/15078,EDGcpfe/15134,
//           EDGcpfe/15144,EDGcpfe/15301]
//
// GNU statement expressions with class type results (IL CHANGE)
//
// Version 4.9 of the front end enabled the ability to have destructible entities
// in GNU statement expressions (GSEs), but with the restriction that the result
// expression of a GSE cannot be of a class type requiring nontrivial copy
// construction or nontrivial destruction.  (See the entry of 1/20/14 for
// EDGcpfe/7317 et al.)
//
// The latter restriction is now lifted.
//
// These changes required an IL CHANGE.  First, if a GSE ends in an expression
// statement (which is the result of the GSE), that statement will have a new
// kind "stmk_stmt_expr_result" (which may have an associated dynamic initializer
// entry to represent a "result by constructor").  (The now-redundant flag
// "is_statement_expression_result" has been eliminated.)  Second, the dynamic
// initializer kind "dik_call_returning_class_via_cctor" has been renamed
// "dik_class_result_via_ctor" because it now not only applies to calls returning
// certain class types, but also to GSEs returning such types (the synonym
// "dik_call_returning_class_via_cctor" is maintained for backward compatibility).
struct D { D(D const&); };
D g(D d) {
  return ({ d; });  // Now accepted.
}

//type:fp
//options_all:--gn 60000
//remark:[5.0] Incorrect deduction of decltype(auto) return type
// 12/14/17 [EDGcpfe/19043]
//
// Incorrect deduction of decltype(auto) return type
//
// The front end previously incorrectly applied certain operand transformations
// on the returned expression of a function declared with decltype(auto) before
// performing the return type deduction.  That caused it to deduce the wrong
// return type in some cases.
//
// This is now fixed.
decltype(auto) g() {  // Return type should be "char const (&)[1]", but
  return "";          // previously deduced as "char const*".
}
static_assert(sizeof(g()) == 1, "Unexpected!");  // Previously failed.
                                                 // Now okay.

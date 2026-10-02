//type:fp
//options_all:--c++20
//remark:[6.7] Abort on substitution of function-scope templated type alias
// 2/29/24  [EDGcpfe/26978,EDGcpfe/27051,EDGcpfe/27055]
//
// Abort on substitution of function-scope templated type alias
//
// A type alias declared in a templated function can later be used in a
// substitution context (e.g., a requires expression).  The front end, however,
// did not record the information needed for substitution processing of any
// contained expression.  This could result in aborts due to failed assertions
// and/or other kinds of incorrect behavior.
template<int I>
struct C {};
template<auto I>
void f() {
  using A = C<+I>;
  static_assert(requires { A{}; });  // Previously triggered an internal
                                     // error.  Now okay.
}
template void f<1>();

//type:fp
//options_all:--c++20 --c++20
//remark:[6.8] Abort due to error node on pack expansion in requires expression
// 2/20/25  [EDGcpfe/27931]
//
// Abort due to error node on pack expansion in requires expression
//
// With the changes for EDGcpfe/27599 (in version 6.7), a pack expansion in a
// requires expression of a trailing requires clause could produce an error
// constant in the IL tree.  In configurations that do IL lowering, this later
// triggered an internal error in lower_type.
//
// Additionally, the front end now correctly substitutes requires expressions in
// the trailing requires clauses of friend function template definitions.  For
// example, with --c++20:
template<int I> constexpr int v = I;
template<int ... Is> struct C { };
template<int I> constexpr int i =
  []<int ... Is>(C<Is...>) requires requires { (1 + ... + v<Is>); } {
    return (1 + ... + v<Is>);
  } (C<1>());
static_assert(i<0> == 2);  // Previously triggered an internal error. 
                           // Now okay.

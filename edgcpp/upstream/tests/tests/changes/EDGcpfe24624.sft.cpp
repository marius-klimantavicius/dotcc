//type:fp
//options_all:--c++20
//remark:[6.4] Substitution of class-type nontype template parameters
// 10/7/22  [EDGcpfe/24624,EDGcpfe/25470]
//
// Substitution of class-type nontype template parameters
//
// The front end previously failed to substitute class-type nontype template
// parameters.
template<int> struct C { };
template<int I, C<I>> constexpr bool v = false;
template<C<1> C1> constexpr bool v<1, C1> = true;
static_assert(v<1, C<1>{}>, "Unexpected");  // Previously failed.  Now okay.

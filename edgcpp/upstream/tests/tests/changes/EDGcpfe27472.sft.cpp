//type:fp
//options_all:--c++20
//remark:[6.7] Expansion of enclosing pack in constraint in out-of-class template definition
// 7/24/24  [EDGcpfe/27472]
//
// Expansion of enclosing pack in constraint in out-of-class template definition
//
// Previously, when a constraint in an out-of-class definition of a member
// template referred to an enclosing parameter pack, that pack was not expanded
// during constraint checking.
template<int I, typename... Ts> concept X = sizeof...(Ts) == I;
template<typename ... Vs>
struct C {
  template<int J> struct D;
};
template<typename ... Us> template<int I>
struct C<Us ...>::D {
  int f() requires X<I, Us ...>;
};
int i = C<int, char>::D<2>().f();  // Previously a spurious error.  Now okay.

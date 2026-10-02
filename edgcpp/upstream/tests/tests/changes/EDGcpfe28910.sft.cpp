//type:fp
//options_all:--g++
//remark:GNU C++ compatibility: __integer_pack in an initializer-list
// 6/23/26  [EDGcpfe/28910]
//
// GNU C++ compatibility: __integer_pack in an initializer-list
//
// In GNU C++ modes, the __integer_pack(N) construct is now accepted as an
// element of an expression list (for example, a braced-init-list, a function
// argument list, or a parenthesized initializer), in addition to the already-
// supported template-argument context.
template<int N> struct S {
  int a[N];
  constexpr S() : a{__integer_pack(N)...} {}
};
S<3> s;

//type:fp
//options_all:--c++20
//remark:[6.7] Substitution of nested pack in requires-expression
// 5/15/24  [EDGcpfe/24907,EDGcpfe/26639]
//
// Substitution of nested pack in requires-expression
//
// A pack of a nested template would previously fail to be expanded in a
// requires-expression during substitution.
template<typename U> struct C { };
template<typename T> struct D {
  template<typename ... Us> requires requires { typename C<Us ...>; }
  struct N { };
};
D<char>::N<short> n;  // Previously a spurious error.  Now okay.

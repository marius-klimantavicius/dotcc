//type:fn
//options_all:--c++17
//remark:Missing diagnostic for redeclaration of deduction guide
// 6/10/26  [EDGcpfe/25301]
//
// Missing diagnostic for redeclaration of deduction guide
//
// Formerly, duplicate user-declared deduction guides were accepted by the front
// end.  An error is now diagnosed for such cases, except in g++ mode with
// gnu_version less than 110000 or in clang mode with clang_version less
// than 90000.
template<class T> struct S {
  template<class U> S(U &&u) {}
};
template<class U> S(U &&) -> S<U>;
template<class U> S(U &&) -> S<U>;

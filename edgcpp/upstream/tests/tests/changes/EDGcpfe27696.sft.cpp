//type:fp
//options_all:--c++20
//remark:[6.7] Nontype template arguments with a destructor
// 11/1/24  [EDGcpfe/27696]
//
// Nontype template arguments with a destructor
//
// Previously, the front end issued an error erroneously claiming that the nontype
// template argument "s" is not constant.  That is now fixed.
struct S {
  int i = 0;
  constexpr S(): i(0) {}
  constexpr S(S const &s): i(s.i) {}
  constexpr ~S() {}
};
template<S> struct X {};
constexpr S s;
X<s> xs;  // Previously an error.  Now okay.

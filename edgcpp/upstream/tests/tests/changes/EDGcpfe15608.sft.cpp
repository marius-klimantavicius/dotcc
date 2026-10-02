//type:fp
//options_all:--c++11
//remark:[4.10] Member access in constexpr mem-initializers
// 12/9/14  [EDGcpfe/15608]
//
// Member access in constexpr mem-initializers
//
// The front end previously failed to treat a constexpr constructor invocation
// as a constant expression if the constructor has a mem-initializer in which
// the expression refers to a previously-initialized member of the class.
// This is now fixed.
struct S {
  constexpr S() : m(42), n(m) { }
  int m, n;
};
static_assert(S().n == 42, "");  // Previously treated as non-constant

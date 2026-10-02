//type:fp
//options_all:--c++11
//remark:[4.13] Defaulted default constructor and constexpr
// 2/14/17  [EDGcpfe/18003]
//
// Defaulted default constructor and constexpr
//
// A defaulted default constructor was sometimes not treated as a constexpr
// constructor when it should have been.
//
// This is now fixed.
struct S {
  S() = default;  // Previously not implicitly "constexpr".
  virtual void f() {}
};
constexpr S s{};  // Previously an error.  Now okay.

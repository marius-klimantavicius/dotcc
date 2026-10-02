//type:fp
//remark:[4.12] Core issue 1684: constexpr member functions in non-literal types
// 6/2/16   [EDGcpfe/17022]
//
// Core issue 1684: constexpr member functions in non-literal types
//
// The original specification for constexpr member functions required constexpr
// member functions to have literal parent class types.  Core issue 1684 dropped
// that requirement.  The front end now now follows the more relaxed rule.
struct S {                                // Not a literal class type.
  S();
  constexpr int f() const { return 42; }  // Previously an error; now okay.
} s;
constexpr int r = s.f();

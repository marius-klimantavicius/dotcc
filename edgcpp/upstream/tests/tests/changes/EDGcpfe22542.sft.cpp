//type:fp
//options_all:--c++14
//remark:[6.1] Failure to fold assignment to rvalue
// 4/2/20   [EDGcpfe/22542]
//
// Failure to fold assignment to rvalue
//
// In some cases, the constexpr interpreter failed to fold the result of an
// assignment to an rvalue.
//
// That is now fixed.
struct S { int x = 1; };
constexpr S a = (S() = S());  // Previously an error.  Now okay.

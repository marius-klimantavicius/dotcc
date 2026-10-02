//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Assignment to non-active union members during constexpr evaluation
// 11/21/18 [EDGcpfe/20493]
//
// C++20: Assignment to non-active union members during constexpr evaluation
//
// In C++20 mode, constexpr evaluation now permits assignment to non-active
// union members.
//
// Previously, an error was issued because "g() == 42" was never a constant
// expression due to the assignment "u.i = 42;" which assigns a value to a
// non-active member of u.  Now, this is accepted in C++20 mode.  This implements
// the C++ standardization committee's change made through paper P1330R0.
union U { int i; double f; };
constexpr int g() {
  U u = { .f = 1.0 };
  u.i = 42;
  return u.i;
}
static_assert(g() == 42);

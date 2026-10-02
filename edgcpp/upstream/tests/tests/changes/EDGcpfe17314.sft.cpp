//type:fp
//options_all:--c++14
//remark:[4.12] C++14 constexpr conversion from unsigned to signed value incorrect
// 6/14/16  [EDGcpfe/17314]
//
// C++14 constexpr conversion from unsigned to signed value incorrect
//
// The C++14 interpreter did not correctly convert values of unsigned types to
// larger signed types in some contexts: It extended the most significant bit of
// the unsigned value as if it were signed.
//
// That is now fixed.
constexpr bool f() {
  unsigned short m = (unsigned short)65535U;
  return (unsigned short)65535U == m;
}
static_assert(f(), "Unexpected");
  // Previously failed because the promotion of m to int in function f
  // incorrectly produced a negative value.

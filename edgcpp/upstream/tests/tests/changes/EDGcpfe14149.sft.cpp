//type:fp
//options_all:--c++14
//remark:[4.11] C++14: Relaxed constexpr constraints
// 3/21/16  [EDGcpfe/14149]
//
// C++14: Relaxed constexpr constraints
//
// In C++14 mode, the front end now relaxes some of the C++11 constraints imposed
// on constexpr functions.  In particular, C++14 constexpr functions can contain
// multiple statements, including loops.
//
// These changes required the addition of an IL interpreter to the front end: It
// is mostly contained in the new source file "interpret.c".
//
// (See also the Changes entry of 2/22/16 for EDGcpfe/16892: It describes new
// options to control how much effort the front end should expend on evaluating
// constant-expressions.)
constexpr unsigned long fact(unsigned long p) {
  unsigned long r = 1;
  while (p > 1) r *= p--;
  return r;
}
static_assert(fact(4) == 24, "Unexpected");  // Accepted in C++14 mode.

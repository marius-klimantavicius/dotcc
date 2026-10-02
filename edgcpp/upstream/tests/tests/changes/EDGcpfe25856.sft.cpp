//type:fp
//options_all:--c++23 --clang
//remark:[6.8] C++23: Local non-automatic variables in constant evaluation
// 4/22/25  [EDGcpfe/25856]
//
// C++23: Local non-automatic variables in constant evaluation
//
// In C++23 mode, the front end now accepts local non-automatic variables to be
// initialized and evaluated as part of constant evaluation, if the variable can
// be used as a constant-expression on its own.
//
// See committee paper P2647R1.  In Clang modes with clang_version >= 160000 this
// feature is accepted with a warning in pre-C++23 modes.
constexpr char hex_digit(int n) {
  thread_local constexpr char digits[] = "0123456789ABCDEF";
  return digits[n];
}
static_assert(hex_digit(10) == 'A');  // Now accepted in C++23 mode.

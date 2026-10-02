//type:fp
//options_all:--c++20
//remark:[6.1] C++20: asm-declarations in constexpr functions
// 1/23/20  [EDGcpfe/21590]
//
// C++20: asm-declarations in constexpr functions
//
// In C++20 mode, the front end now accepts asm-declarations in constexpr
// functions.  Such declarations cannot be evaluated as part of constant
// expressions, however.
//
// This change was introduced in the working paper for C++20 by the C++
// standardization committee's paper P1668R1.  The predefined macro
// __cpp_constexpr now has the value 201907L in C++20 mode to reflect that this
// feature and the changes for EDGcpfe/21581 etc. are now implemented.
constexpr int f() {
  return 42;
  asm ("");
}
static_assert(f() == 42);  // Now okay in C++20 mode.

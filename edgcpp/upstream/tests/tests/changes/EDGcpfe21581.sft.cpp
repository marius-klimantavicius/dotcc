//type:fp
//options_all:--c++20 --clang
//remark:[6.1] C++20: Uninitialized local variables in constexpr functions
// 1/23/20  [EDGcpfe/21581,EDGcpfe/22234,EDGcpfe/22240]
//
// C++20: Uninitialized local variables in constexpr functions
//
// The changes for EDGcpfe/17158, which permitted uninitialized local variables of
// empty class types in constexpr function definitions, have now been enabled in
// all nonstrict C++14 modes and in all C++20 modes.
//
// Furthermore, in C++20 mode, this is now generalized for all types and the
// rules introduced by the standardization committee's P1331R2 are enabled.
struct S {};
constexpr void g() {
  S s;  // Now accepted in all nonstrict C++14 modes.
}       // Previously only accepted in GNU and Clang C++14 modes.

struct X { int i; };
constexpr X h() {
  X x, y = {42};
  x = y;
  return x;
}
static_assert(h().i == 42);  // Now accepted in C++20 mode.

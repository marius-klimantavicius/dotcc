//type:fp
//options_all:--gn 140200 --c++17
//remark:__builtin_add_overflow, etc.
// 12/19/25 [EDGcpfe/28586]
//
// __builtin_add_overflow, etc.
//
// The changes for EDGcpfe/18917 (etc.) added support for folding of arithmetic
// operation intrinsics (like __builtin_add_overflow, etc.) that record the
// occurrence of overflow.  Some cases combining signed source types and an
// unsigned destination type did not produce the correct result.
//
// That is now fixed.
constexpr bool f() {
  unsigned b{};
  return  __builtin_sub_overflow(0, -1, &b);
}
static_assert(!f(), "");  // Previously failed.  Now okay.

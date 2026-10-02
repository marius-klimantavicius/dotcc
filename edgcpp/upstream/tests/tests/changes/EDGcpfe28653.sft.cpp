//type:fp
//options_all:--gn 160001 --c++14
//remark:GNU/Clang compatibility: __builtin_is_virtual_base_of
// 1/21/26  [EDGcpfe/28653]
//
// GNU/Clang compatibility: __builtin_is_virtual_base_of
//
// The new type trait intrinsic __builtin_is_virtual_base_of is now supported in
// Clang 20+ and GCC 15+ modes.
struct B {};
struct D : virtual B {};
static_assert(__builtin_is_virtual_base_of(B, D), "");

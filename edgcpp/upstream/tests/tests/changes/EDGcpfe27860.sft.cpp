//type:fp
//options_all:--g++
//remark:[6.8] GNU compatibility: __builtin_index is now constexpr
// 2/7/25   [EDGcpfe/27860]
//
// GNU compatibility: __builtin_index is now constexpr
//
// The __builtin_index function is now folded in contexts where it can be
// constant-evaluated.
static_assert(__builtin_index("xyzzy", 'a') == nullptr);

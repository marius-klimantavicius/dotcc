//type:fp
//options_all:-w --clang_v 220100 --c++23
//remark:Clang compatibility: __builtin_dedup_pack
// 3/12/26  [EDGcpfe/28720]
//
// Clang compatibility: __builtin_dedup_pack
//
// In Clang C++ modes with clang_version >= 220000, the front end now implements
// support for an intrinsic __builtin_dedup_pack template.
template<typename... Ts> struct TypeList {};
template<typename... Ts>
using DedupedList = TypeList<__builtin_dedup_pack<Ts...>...>;
DedupedList<int, int, char> x;

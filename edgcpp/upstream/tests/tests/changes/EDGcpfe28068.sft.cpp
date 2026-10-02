//type:fp
//options_all:--clang_v 200100
//remark:GNU/Clang compatibility: __builtin_is_implicit_lifetime
// 1/26/26  [EDGcpfe/28068,EDGcpfe/28654]
//
// GNU/Clang compatibility: __builtin_is_implicit_lifetime
//
// The new type trait intrinsic __builtin_is_implicit_lifetime is now supported in
// Clang 20+ and GCC 16+ modes.
struct S { int x; };
bool b = __builtin_is_implicit_lifetime(S);

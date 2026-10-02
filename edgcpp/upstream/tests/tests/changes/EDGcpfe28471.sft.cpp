//type:fp
//options_all:--microsoft --microsoft_version=1951
//remark:Microsoft compatibility: __builtin_is_implicit_lifetime
// 2/23/26  [EDGcpfe/28471]
//
// Microsoft compatibility: __builtin_is_implicit_lifetime
//
// The new type trait intrinsic __builtin_is_implicit_lifetime is now also enabled
// in Microsoft mode with microsoft_version >= 1951.
struct S { int x; };
bool b = __builtin_is_implicit_lifetime(S);

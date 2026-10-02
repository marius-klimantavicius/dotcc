//type:fp
//options_all:--clang
//remark:[4.14] G++ and Clang compatibility: Enable __decltype to be used as a base specifier
// 4/10/17  [EDGcpfe/18181]
//
// G++ and Clang compatibility: Enable __decltype to be used as a base specifier
//
// GCC version 4.7.0 and later, as well as Clang, accept __decltype as a
// base specifier and now so does the front end.
struct B {};
struct A : __decltype(B()) {};

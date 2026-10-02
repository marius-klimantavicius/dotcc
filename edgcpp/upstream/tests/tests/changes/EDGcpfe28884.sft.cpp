//type:fp
//options_all:--clang_version=220100 --c++17
//remark:Clang __builtin_invoke and pointer-to-data-member
// 6/16/26  [EDGcpfe/28884]
//
// Clang __builtin_invoke and pointer-to-data-member
//
// The intrinsic handling of __builtin_invoke (accepted in some Clang C++ modes)
// failed to handle pointer-to-data-member cases.  That is now fixed.
struct X { int data; int func(); };
template<class T> T&& declval();
using Tf = decltype(__builtin_invoke(declval<int (X::*)()>(), declval<X&&>()));
using Td = decltype(__builtin_invoke(declval<int X::*>(), declval<X&&>()));

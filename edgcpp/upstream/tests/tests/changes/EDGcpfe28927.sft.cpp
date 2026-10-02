//type:fp
//options_all:--clang_v 220100 --c++03
//remark:Structured bindings in Clang C++03 mode
// 7/10/26  [EDGcpfe/28927]
//
// Structured bindings in Clang C++03 mode
//
// The changes for EDGcpfe/20107 etc. (introduced in version 6.6) enabled
// structured bindings in Clang C++11 mode when clang_version >= 40000.  It
// turns out that Clang 4 and later also accept structured bindings in C++03
// mode: The front end has been updated to match that behavior in corresponding
// Clang C++03 modes.
struct Pair { int a; int b; };
void f(Pair p) { auto [x, y] = p; (void)x; (void)y; }

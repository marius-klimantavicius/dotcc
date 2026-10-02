//type:fp
//options_all:--c++11 --clang
//remark:[4.14] Unbounded loop on prefix enable_if attribute
// 3/7/17   [EDGcpfe/18077]
//
// Unbounded loop on prefix enable_if attribute
//
// The changes for EDGcpfe/17738 (the Clang enable_if attribute) contained a bug
// causing the front end to enter a non-terminating loop when the enable_if
// attribute appeared as a prefix to a declaration along with other prefix
// attributes.
//
// This is now fixed.
__attribute((deprecated)) __attribute((enable_if(true, ""))) void f() {}
  // Triggered an unbounded loop in Clang C++11 mode.

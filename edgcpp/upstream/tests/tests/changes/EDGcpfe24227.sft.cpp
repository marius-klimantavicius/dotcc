//type:fn
//options_all:--c++17 --gnu_version=100200
//remark:[6.3] Equality folding in GNU and Clang C++ modes
// 5/21/21  [EDGcpfe/24227]
//
// Equality folding in GNU and Clang C++ modes
//
// The front end emulates a GCC behavior where expressions like "x == x" (with x a
// variable that is not constant-valued) are treated as constant-expressions.
// Previously, that emulation was accidentally enabled in Clang modes (because
// they are internally treated as variants of corresponding GNU modes): That is no
// longer the case.  Furthermore, that behavior is no longer emulated in GNU C++03
// modes or in GNU C++11 modes with gnu_version >= 60000 (this matches the
// observed behavior of various GCC versions).  The behavior is still enabled in
// all GNU C modes.
void g(int x) {
  enum { e = x == x ? 1 : -1 };  // Now an error in Clang modes, in GNU C++03
}                                // modes, and in GNU C++11 modes with
                                 // gnu_version >= 60000.

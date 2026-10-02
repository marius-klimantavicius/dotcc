//type:fp
//options_all:--clang_v 100100 -w --c
//remark:[6.5] Clang C compatibility: Promotion and overload resolution
// 4/4/23   [EDGcpfe/23475,EDGcpfe/25708]
//
// Clang C compatibility: Promotion and overload resolution
//
// In Clang C mode, the "overloadable" attribute enables overloading function
// names.  However, the front end previously did not distinguish integral
// promotion from other implicit integral conversions in C mode, thus rendering
// ambiguous certain cases that Clang accepts in its C mode.
//
// That is now fixed.
void __attribute((overloadable)) g(int);
void __attribute((overloadable)) g(unsigned);
void f(char c) {
  g(c);  // Previously ambiguous.  Now okay.
}

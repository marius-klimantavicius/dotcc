//type:fp
//options_all:--c11 --clang
//remark:[6.4] Clang C compatibility: _Generic selector operand
// 7/19/22  [EDGcpfe/19491,EDGcpfe/25473]
//
// Clang C compatibility: _Generic selector operand
//
// In Clang C mode, the first operand of _Generic constructs now undergoes the
// usual operand transformations (which, e.g., drops top-level "const" from the
// type or performs array-to-pointer decay).  This was already the case in other
// modes that accept the C _Generic construct.
void g(void) {
  int a[1];
  _Generic (a, int*: 0, int const*: 0);  // Previously an error in Clang
}                                        // C mode.  Now okay.

//type:fp
//options_all:--microsoft
//remark:[4.12] Microsoft compatibility: unbounded recursion in macro expansion
// 6/2/16   [EDGcpfe/17204,EDGcpfe/17239,EDGcpfe/17406]
//
// Microsoft compatibility: unbounded recursion in macro expansion
//
// The change for EDGcpfe/16200 (in version 4.11) introduced a regression that
// resulted in unbounded recursion when a macro definition directly invokes
// the macro from a position other than the first token.  This is now fixed.
//
// In addition, that change caused a regression in which the behavior of the
// front end differed from that of the Microsoft preprocessor for recursive
// macro invocations occurring within the expansion of an argument in the
// top-level invocation of a macro; recursion is no longer allowed in this
// context, which matches the behavior of the Microsoft preprocessor.  For
// example:
int M(int i) { return i; }
#define M(a) 1+M(a)
int j = M(M(2));    // Previously caused unbounded recursion

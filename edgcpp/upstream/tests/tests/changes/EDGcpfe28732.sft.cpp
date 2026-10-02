//type:fp
//options_all:--clang_v 210100 --c++11 -tused
//remark:Clang compatibility: Accept an optional fourth argument on diagnose_if
// 3/9/26   [EDGcpfe/28732]
//
// Clang compatibility: Accept an optional fourth argument on diagnose_if
// attributes
//
// Clang 20.1.0 allows an optional fourth string argument to diagnose_if
// attributes.  The front end now allows that as well.  Arguments to diagnose_if
// are parsed and recorded, but are otherwise ignored.
void f(int x) __attribute__((diagnose_if(x < 0, "neg", "warning", "extra")));

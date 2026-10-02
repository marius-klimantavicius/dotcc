//type:fp
//options_all:--clang_version=180000 --c++20
//remark:[6.7] Clang compatibility: Deduction from _Nullable types
// 1/13/25  [EDGcpfe/27831]
//
// Clang compatibility: Deduction from _Nullable types
//
// Previously, this failed template deduction because the front end expected the
// _Nullable qualifier in the parameter type of g(...) to match a qualifier in
// the parameter of pf (which doesn't actually include any qualifier).  That is
// now fixed.
int g(char *_Nullable *p);
template <typename T> int f(int (*pf)(T**)) {
  return pf(nullptr);
}
int x = f(&g);  // Previously failed deduction.  Now okay.

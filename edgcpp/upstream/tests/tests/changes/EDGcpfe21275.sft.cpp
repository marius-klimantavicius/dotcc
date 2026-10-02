//type:fp
//options_all:--c++17 -A --exceptions -tused -w
//remark:[6.1] Failure to deduce "auto" template parameter
// 3/25/20  [EDGcpfe/21275,EDGcpfe/22515]
//
// Failure to deduce "auto" template parameter
//
// In some cases, the front end failed to deduce a C++17-style "auto" template
// parameter when it should have been able to do so.
//
// That is now fixed.
template<typename T, auto = T()+42> int f(T) { return 42; };
int r = f('x');  // Previously a spurious error.  Now okay.

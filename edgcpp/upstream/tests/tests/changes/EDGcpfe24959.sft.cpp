//type:fp
//options_all:--c++11
//remark:[6.4] Spurious parsing error in __INTADDR__ construct
// 1/11/22  [EDGcpfe/24959]
//
// Spurious parsing error in __INTADDR__ construct
//
// In C++11 (and later) modes, the EDG-specific __INTADDR__ construct was not
// always parsed correctly.
//
// In this example, scanning the __INTADDR__ construct accidentally skipped past
// the closing angle bracket.  That is now fixed.
struct S { void *m; };
template<unsigned N> void f();
void g() {
  f<((unsigned)__INTADDR__(&(((S*)0)->m)))>();  // Previously sometimes
}                                               // issued a spurious error.

//type:fp
//options_all:--gn 70300
//remark:[6.1] Spurious error on attempt to call friend function with auto return type
// 4/30/20  [EDGcpfe/22635]
//
// Spurious error on attempt to call friend function with auto return type
//
// The changes for EDGcpfe/18786,EDGcpfe/21191 (see entry of 11/25/19) introduced
// a regression in version 6.0 causing the front end to erroneously discard
// certain friend function declarations with a deduced ("auto") return type during
// call resolution.
//
// That is now fixed.
template<typename> class S {
  friend auto f(S const&) { return 0; }
};
template<typename T, typename = decltype(f(T()))> int g(T const &) {
  return 0;
}
int r = g(S<int>());  // A spurious error in version 6.0.  Now okay.

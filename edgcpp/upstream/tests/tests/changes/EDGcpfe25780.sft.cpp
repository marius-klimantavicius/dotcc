//type:fp
//options_all:--c++14
//remark:[6.5] Abort elicited by checking for duplicate captures in a nested lambda
// 12/1/22  [EDGcpfe/25780]
//
// Abort elicited by checking for duplicate captures in a nested lambda
//
// The changes for EDGcpfe/25246 (in version 6.4) included a bug that caused the
// front end to sometimes fail an assertion check in diagnose_duplicate_capture
// after looking for duplicate captured in a nested lambda.
//
// That is now fixed.
template<typename Fn, typename Out, typename... In>
struct W {
  W (Fn fn): fn(fn) {}
  virtual void call (In... in) { fn(in...); }
  Fn fn;
};
template<typename> class F;
template <typename Out, typename... In>
struct F<Out(In...)> {
  template<typename Fn> F(Fn fn) {
    W<Fn, Out, In...> tmp(fn);
  }
};
void f(F<void(int)>); 
void g(int x, int y) {
  f( [x = x, y = y](auto) { [x, y]() {}; } );  // Previously triggered an
}                                              // internal error.  Now okay.

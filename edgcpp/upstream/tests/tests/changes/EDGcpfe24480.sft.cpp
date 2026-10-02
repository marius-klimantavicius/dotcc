//type:fp
//options_all:--microsoft_v 1928 --ms_c++20
//remark:[6.4] Assertion failure in deduce_class_template_args
// 6/1/22   [EDGcpfe/24480,EDGcpfe/24528,EDGcpfe/25359]
//
// Assertion failure in deduce_class_template_args
//
// The declaration of variable w relies on class template argument deduction, but
// the front end code intended to handle template-dependent initializations did
// not cover all cases (including this one), which then resulted in failing an
// assertion check in deduce_class_template_args (overload.c).  That caused an
// internal error for the example above.  That problem is now fixed.
template<typename T> struct W { W(T inner); };
struct S {
  int f();
  template<typename ... T> void vf(T ... args) {
    W w{ f() };  // Previously aborted.  Now okay.
  }
};

//type:fp
//options_all:--microsoft_v 1940 --ms_c++20
//remark:[6.7] Microsoft-mode abort on copy of closure object
// 7/8/24   [EDGcpfe/27439]
//
// Microsoft-mode abort on copy of closure object
//
// This previously triggered an internal error in conv_glvalue_expr_to_prvalue.
// The underlying reason for this is that in Microsoft mode, a lambda expression
// (which is a prvalue) can be converted to a corresponding glvalue, but the
// conversion back to a prvalue was (accidentally) not supported.  This particular
// example exercises the back-and-forth conversion, thus triggering an internal
// error.  This is now fixed.
class C {
  template<typename T> C(T);
  using Alias = C&&;
  void f() {
    Alias([]{});  // Previously triggered an internal error.  Now okay.
  }
};

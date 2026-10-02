//type:fp
//options_all:--g++
//remark:[6.7] Internal error in inactive_scope_lookup in GNU mode
// 10/17/24 [EDGcpfe/27634]
//
// Internal error in inactive_scope_lookup in GNU mode
//
// When the front end is not deferring function prototype instantiations in GNU
// mode, an internal error could occur in inactive_scope_lookup in some fairly
// complex cases involving a using-declaration for a base-class member where the
// base class has the same name as the class template.
// --no_defer_parse_function_templates:
template<class T> struct B;
template<> struct B<void> {
  void f();
};
template<class T> struct B : B<void>, T {
  using B<void>::f;
  void g() {
    B::f();  // Previously triggered an internal error.  Now okay.
  }
};

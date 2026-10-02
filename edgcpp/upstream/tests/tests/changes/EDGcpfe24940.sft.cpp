//type:fp
//options_all:--c++20 --clang
//remark:[6.5] Abort in push_template_instantiation_scope
// 5/22/23  [EDGcpfe/24940,EDGcpfe/25601,EDGcpfe/26354]
//
// Abort in push_template_instantiation_scope
//
// A number of situations could result in an assertion error when pushing an
// instantiation scope for rescanning purposes.
//
// That is now fixed.
template<typename T> decltype(T::f()) g(T);
template<typename T> struct S {
  static void f() requires __is_pointer(T) || requires(T x) { x.f; };
};
void h() { g(S<S<int*>>()); }  // Previously triggered an internal error
                               // while substituting the constraint for
                               // S<int*>::f().

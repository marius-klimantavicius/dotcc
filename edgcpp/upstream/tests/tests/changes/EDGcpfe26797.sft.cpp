//type:fp
//options_all:--c++20
//remark:[6.7] Enclosing pack expansion in abbreviated member function template declaration
// 1/11/24  [EDGcpfe/26797]
//
// Enclosing pack expansion in abbreviated member function template declaration
//
// Previously, an abbreviated member function template declaration naming an
// enclosing template parameter pack would abort with a failed assertion in
// find_placeholder_arg_for_pack (scope_stk.c).
template<typename ...> struct A {};
template<typename ... T> struct C {
  static int f(A<T ...>, auto);
};
int i = C<int>::f(A<int>(), 1);  // Previously triggered an internal error.
                                 // Now okay.

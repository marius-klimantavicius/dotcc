//type:fp
//options_all:--c++20
//remark:[6.8] Internal error on substitution of dependent template arguments
// 1/31/25  [EDGcpfe/26806,EDGcpfe/27822]
//
// Internal error on substitution of dependent template arguments
//
// Previously, the front end failed to substitute dependent template arguments
// into a nested template-id that is itself used as a nested name qualifier.  This
// could result in an abort with a failed assertion in equiv_template_arg_lists
// when ordering function overloads by constraints.
template<typename>
struct B {
  template<typename T>
  struct N { using A = T; };
};
template<typename T> concept C1 = sizeof(T) != 0;
template<typename T> concept C2 = C1<typename B<T>::template N<T>::A>;
template<typename T> bool f() requires C2<T>;
template<typename T> bool f() requires C2<T> && true;
bool b = f<int>();  // Previously triggered an internal error.  Now okay.

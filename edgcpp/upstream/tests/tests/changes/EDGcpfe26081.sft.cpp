//type:fp
//options_all:--c++14
//remark:[6.5] Abort in compare_expressions for "function call" to a variable
// 3/21/23  [EDGcpfe/26081]
//
// Abort in compare_expressions for "function call" to a variable
//
// A "function call" expression to a variable of class type with an overloaded
// function call operator appearing in a dependent decltype could trigger an abort
// in compare_expressions.  Additionally, template arguments of variable template
// specializations were not compared in these cases, which could result in
// spurious redefinition errors.
struct C {
  int operator () (int = 0);
};
template<typename T> C c;
template<typename T> using F = T;
template<typename T, F<decltype(c<T *>(1))>>
void f(T) { }
template<typename T, F<decltype(c<T &>(1))>>
void f(T) { }  // Previously a spurious redefinition error.  Now okay.
template<typename T, F<decltype(c<T>())>> void g(T) { }
template<typename T, F<decltype(c<T>())>> void h(T) { }

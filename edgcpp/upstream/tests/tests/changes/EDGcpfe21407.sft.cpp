//type:fp
//options_all:--c++11
//remark:[6.5] Incorrect expansion of function parameter pack in trailing return type
// 3/27/23  [EDGcpfe/21407]
//
// Incorrect expansion of function parameter pack in trailing return type
//
// A function parameter pack was not always expanded correctly when explicit
// template arguments were provided for the function, resulting in spurious
// substitution failures.
struct C {
  static int g();
};
template<typename T, typename ... U>
auto f(T t, U ... u) -> decltype(C::g(u ...));
int i = f<int>(1);  // Previously a spurious error.  Now okay.

//type:fp
//options_all:--c++20
//remark:[6.6] Substitution of requires expressions
// 10/18/23 [EDGcpfe/26228,EDGcpfe/26298]
//
// Substitution of requires expressions
//
// Previously, the front end always performed substitution into a requires
// expression in a dependent context, causing some constraints to be spuriously
// satisfied.  Additionally, the front end previously only substituted the
// innermost template arguments into nested requirements.  Both issues are now
// fixed.
template<typename T, bool B = true>
struct A {
  T t;
  int f(int) requires (!requires { t.i; });
  template<typename U> int g(U) requires requires { requires B; };
};
int i = A<int>().f(0);  // Previously a spurious error.  Now okay.
int j = A<int>().g(0);  // Previously a spurious error.  Now okay.

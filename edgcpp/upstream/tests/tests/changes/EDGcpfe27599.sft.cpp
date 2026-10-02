//type:fp
//options_all:--c++20
//remark:[6.7] Substitution of member template requires clauses in template heads
// 10/7/24  [EDGcpfe/27599]
//
// Substitution of member template requires clauses in template heads
//
// Previously, a requires clause in the template head of a member template was
// instantiated together with the definition of the enclosing class template
// specialization.  That in turn could result in spurious errors.  Now,
// substitution into the requires clause of a template head is only performed
// during constraint checking.
template<typename U>
struct A {
  template<typename T> requires (sizeof(U) > 1)  // Previously a spurious
  static int f(T t);                             // error.  Now okay.
  static int f(long);
};
int i = A<void>::f(0);

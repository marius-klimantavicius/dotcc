//type:fp
//options_all:--c++20
//remark:[6.7] Substitution of member function template trailing requires clauses
// 8/12/24  [EDGcpfe/22821,EDGcpfe/27314,EDGcpfe/27438]
//
// Substitution of member function template trailing requires clauses
//
// Previously, a trailing requires clause of a member function template was
// instantiated together with the definition of the enclosing class template
// specialization.  That in turn could result in spurious errors.  Now,
// substitution into the trailing requires clause is only performed during
// constraint checking.
template<typename U>
struct A {
  template<typename T>
  static int f(T t) requires (sizeof(U) > 1);  // Previously a spurious
                                               // error.  Now okay.
  static int f(long);
};
int i = A<void>::f(0);

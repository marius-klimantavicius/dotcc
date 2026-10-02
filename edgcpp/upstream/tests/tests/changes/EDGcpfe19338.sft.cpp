//type:fp
//options_all:--c++11
//remark:[6.5] Spurious error for functional notation cast with empty pack expansion
// 3/21/23  [EDGcpfe/19338,EDGcpfe/22788,EDGcpfe/25109]
//
// Spurious error for functional notation cast with empty pack expansion
//
// Previously, the front end issued a spurious error for a functional notation
// cast to an instantiated non-class type from an expression list consisting of a
// single expression and an additional empty pack expansion.
// --c++11:
template<typename T, typename ... U>
T f(T t, U ... u) {
  return T(t, u ...);  // Previously a spurious error.  Now okay.
}
template int f(int);

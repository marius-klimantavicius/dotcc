//type:fp
//remark:[4.12] Overloaded literal operator templates
// 7/29/16  [EDGcpfe/17056]
//
// Overloaded literal operator templates
//
// The front end previously did not correctly handle overloaded literal operator
// templates (reporting an ambiguity in all cases).
//
// This is now fixed.
template<bool, typename = void> struct enable_if {};
template<typename T> struct enable_if<true, T> { typedef T type; };
template <char... cs>
  typename enable_if<sizeof...(cs) == 1, int>::type operator""X();
template <char... cs>
  typename enable_if<sizeof...(cs) >= 2, int>::type operator""X();
int r = 42X;  // Previously triggered a spurious error.  Now okay.

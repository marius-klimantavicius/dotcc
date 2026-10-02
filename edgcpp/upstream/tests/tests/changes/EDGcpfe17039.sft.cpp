//type:fp
//remark:[4.12] Spurious deduction failure of with dependent nontype template parameter that
// 7/19/16  [EDGcpfe/17039]
//
// Spurious deduction failure of with dependent nontype template parameter that
// acquires a reference type
//
// In some cases, the front end triggered a deduction failure when attempting to
// bind a nontype template argument with a template-dependent type that becomes a
// reference type after substitution.  This could lead to spurious errors later
// on.
//
// This is now fixed.
extern constexpr int v = 0;
template<typename T> struct Id {
  typedef T Type;
};
template<typename T, typename Id<T>::Type V> struct X {};
template<typename T> void f(X<const T&, v>) {}
void g() {
  X<const int&, v> x;
  f(x);  // Previously an error because v was not successfully bound to
}        // nontype template parameter V in X<const T&, v>.  Now okay.

//type:fp
//options_all:--g++
//remark:[4.5] GNU and Microsoft compatibility: Instantiated pointer-to-member functions
// 1/18/12  [EDGcpfe/12214]
//
// GNU and Microsoft compatibility: Instantiated pointer-to-member functions
//
// In GNU and Microsoft mode, instantiating a template containing a dependent
// pointer-to-member-function type PMF may cause any const/volatile qualifiers in
// the substituted parent class of PMF to be applied to the qualification of the
// member function itself.  E.g., a substitution of "int T::f()" with T = const C
// results in a type "int C::f() const".
//
// In Microsoft mode, this is only done when instantiating pointer-to-member
// types from token caches.  In GNU mode, however, this is also done in type
// substitution (but not during deduction).
template<typename T, void (T::*P)()> struct X {};
struct S {
  void f() const {}
};
typedef X<S const,  &S::f> XST;  // Now accepted in GNU and Microsoft
                                 // C++ modes.

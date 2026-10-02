//type:fp
//remark:[4.2] Spurious error when member function fails to hide a using-declaration
// 11/20/09 [EDGcpfe/10159,EDGcpfe/10220]
//
// Spurious error when member function fails to hide a using-declaration
//
// When the declaration of a member function or member function template matches
// a declaration brought in by a using-declaration, the front end previously
// failed to hide the declaration brought in from the base class.  This could
// result in spurious ambiguity errors later on, most commonly when calling
// member function templates.
//
// This is now fixed.
struct B { template <class T> int f(T); };
struct D: B { 
  using B::f;
  template <class T> int f(T);
};
void g(D *p) {
  p->f(1);  // Previously triggered a spurious ambiguity error.
}

//type:fp
//options_all:--microsoft
//remark:[4.3] Spurious error in class template with explicit "override" (Microsoft mode)
// 11/2/10  [EDGcpfe/11103]
//
// Spurious error in class template with explicit "override" (Microsoft mode)
//
// Version 4.2 of the front end introduced a bug that triggered spurious errors
// on some virtual member functions of class templates marked with the "override"
// specifier.
//
// This is now fixed.  (The problem was introduced by the changes for
// EDGcpfe/10573; see entry of 4/22/10.)
template<class T> struct C {
  typedef int I;
  virtual void f(I);
};
template<class T> struct D: C<T> {
  virtual void f(I) override;
};

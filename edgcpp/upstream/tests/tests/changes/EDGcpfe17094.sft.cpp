//type:fp
//remark:[4.12] Redundant ::template and operator names
// 5/19/16  [EDGcpfe/17094]
//
// Redundant ::template and operator names
//
// In some modes, a "::template" prefix is accepted for a name that is not
// followed by a template argument list.  The changes for EDGcpfe/15656 dropped
// that relaxation for operator names.  Now, the previous behavior is restored
// (while still addressing the problem fixed by the change for EDGcpfe/15656).
template<typename T> class A {
  template<typename U> void operator<<(U const&);
};
template<typename T>
  template<typename U>
    void A<T>::template operator<<(U const&) {}
      // Extraneous "template" now accepted in GNU and Microsoft modes.

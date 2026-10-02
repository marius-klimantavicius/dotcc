//type:fp
//remark:[4.6] Spurious Microsoft-mode error on assignment operator from a dependent base
// 11/26/12 [EDGcpfe/13364]
//
// Spurious Microsoft-mode error on assignment operator from a dependent base
//
// In Microsoft C++ mode, version 4.5 of the front end introduced a bug causing
// it to sometimes issue a spurious redeclaration error when a class template
// derived from a dependent base class declaring a user-defined assignment
// operator.
//
// This is now fixed.  (This bug was introduced by the changes for EDGcpfe/12786
// etc. -- see the entry of 5/14/12.)
template<int N> struct B {
  typedef B<N> BN;
  BN& operator=(BN const&);
};
template<int N> struct D: B<N> {};
  // The "nonreal" instantiation of base class type B<N> previously
  // triggered a redeclaration error for B<N>::operator=.

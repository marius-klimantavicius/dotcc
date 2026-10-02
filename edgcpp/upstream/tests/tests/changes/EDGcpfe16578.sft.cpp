//type:fp
//remark:[4.11] Partial substitution of constant expressions in pre-C++11 SFINAE contexts
// 10/30/15 [EDGcpfe/16578]
//
// Partial substitution of constant expressions in pre-C++11 SFINAE contexts
//
// The changes implementing C++11 SFINAE (see also the entry of 4/26/10 for
// EDGcpfe/9169) accidentally changed the behavior of pre-C++11 SFINAE in certain
// situations where a partial substitution of a template parameter list (through
// an explicit but incomplete list of template arguments) results in a
// constant-expression with remaining template parameters.
//
// Here, the partial substitution for m<int&> (with T left unsubstituted)
// triggered a spurious SFINAE failure in the substitution of the constant
// expression "true || (sizeof(T)<8)", which in turn caused the front end to fail
// to resolve the call.  This is now fixed.
template <bool B, class T> struct E {
  typedef T type;
};
template <class Ret, class T>
  typename ::E<true || (sizeof(T)<8), T&>::type m(T&);
void g(int i) {
  m<int&>(i);  // Previously an error in C++03 mode; now okay.
}

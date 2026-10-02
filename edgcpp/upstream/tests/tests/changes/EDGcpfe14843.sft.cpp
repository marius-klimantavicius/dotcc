//type:fp
//remark:[4.9] Instantiations during copy/move assignment operator generation
// 2/6/14   [EDGcpfe/14843]
//
// Instantiations during copy/move assignment operator generation
//
// When the front end determines which special members to generate for a class
// type (like copy constructors and copy assignment operators), it may have to
// instantiate class types, which may lead to errors (which instantiations occur
// is not precisely defined by the language).  The front end now avoid some such
// instantiations while generating copy/move assignment operators.
//
// Previously, while considering the generation of a move assignment operator
// for X<S const> in this example, the front end attempted to instantiate
// X<S>::value_type (which doesn't exist, and therefor triggers an error) as
// part of testing the member assignment template of S for copying the member
// X<S const>::v.  Now the member assignment template is discarded without
// requiring the instantiation of X<T>::value_type.
template<typename> struct P {};
template <typename T> struct P<T const&> {
  typedef T value_type;
};
template < typename T> struct X {
  typedef typename P<T&>::value_type value_type;
  value_type v;
};
struct S {
  void operator=(S&);
  template<typename T> typename X<T>::value_type operator=(T&);
};
X<S const> e;

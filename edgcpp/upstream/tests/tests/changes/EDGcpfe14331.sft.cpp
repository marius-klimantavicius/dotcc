//type:fp
//remark:[4.10] Typeinfo structures and key/decider functions
// 11/18/14 [EDGcpfe/14331]
//
// Typeinfo structures and key/decider functions
//
// In classes with a key (or "decider") function, the front end now avoids
// emitting the associated typeinfo structures if that key/decider function is
// not defined.  Previously, those structures were forced in any translation
// units where exception handling or RTTI features required the existence of the
// structures.  That in turn could result in unneeded instantiations of virtual
// member functions, which in turn could trigger errors.
struct I;
struct B {
  virtual void k();  // Key/decider function.
};
template<typename T> struct C: B {
  virtual void f() { T i; };  // Would trigger an error if instantiated
};                            // with T == I.
struct D: C<I> {
  virtual void k();
};
D& g(B &b) {
  return dynamic_cast<D&>(b);  // Previously caused the instantiation of
}                              // C<I>::f(), and thus an error.  Now okay.

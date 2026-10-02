//type:fp
//remark:[4.3] Internal error in dtor_initializer on using-declaration for operator delete
// 9/14/10  [EDGcpfe/11010]
//
// Internal error in dtor_initializer on using-declaration for operator delete
//
// In some cases, the front end previously aborted with an internal error in
// dtor_initializer (decl_inits.c) when processing the definition of a destructor
// of a class containing a using-declaration for a set of "delete" operators in a
// base class.
//
// This is now fixed.
struct B {
  void  operator delete(void*);
  void  operator delete(void*, void*);
  virtual ~B();
};
struct D: B {
  using B::operator delete;
  ~D() {}  // The front end previously aborted when processing this
};         // destructor definition.

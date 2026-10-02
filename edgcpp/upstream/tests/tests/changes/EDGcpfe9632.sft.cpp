//type:fp
//options_all:--g++
//remark:[4.1] GNU C++ compatibility: Overriding a virtual function returning void*
// 3/23/09  [EDGcpfe/9632]
//
// GNU C++ compatibility: Overriding a virtual function returning void*
//
// In GNU C++ mode, the front end now accepts overriding a virtual function
// returning a void* pointer with a virtual function returning another pointer
// type.  A warning is issued in such cases.
//
// (Unlike covariant return types -- see Changes entry of 11/4/96 -- this
// does not involve pointer adjustments.)
struct B {
  virtual void* f();
};
struct D: B {
  B* f() { return this; }  // Now accepted in GNU C++ mode.
};

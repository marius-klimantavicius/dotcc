//type:fp
//remark:[4.10] Downgrade diagnostic on virtual function declared but not defined in a
// 8/22/14  [EDGcpfe/15379]
//
// Downgrade diagnostic on virtual function declared but not defined in a
// local class
//
// According to the C++ standard, a non-pure virtual function is "odr-used", and
// as such, an error had been issued when a virtual function in a local class
// had been declared but not defined (because a reference to the function
// will be in the vtable, causing an error at link time).  In cases where
// the local class is unused, the error might be considered spurious, so it
// has been downgraded to a warning (except in strict mode).
void g() {
  struct A {
    virtual void f();   // Now a warning except in strict mode.
  };
}

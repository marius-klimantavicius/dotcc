//type:fp
//options_all:--c++11
//remark:[4.12] Spurious access error on braced constructor initializer
// 5/26/16  [EDGcpfe/17037]
//
// Spurious access error on braced constructor initializer
//
// In C++11 mode, a braced constructor initializer for a base class, selecting a
// protected default base class constructor previously elicited a spurious access
// error.
//
// This is now fixed.
struct B {
protected:
  B();
};
struct D: B {
  D(): B{} {}  // Previously triggered a spurious error about B::B() not
};             // being accessible.  Now okay.

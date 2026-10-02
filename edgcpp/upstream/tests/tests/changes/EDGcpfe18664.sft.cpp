//type:fp
//options_all:--c++14 -tused
//remark:[4.14] Constexpr interpreter: Value-initialization of empty derived class
// 8/14/17  [EDGcpfe/18664]
//
// Constexpr interpreter: Value-initialization of empty derived class
//
// The C++ interpreter failed to correctly record the value-initialization of an
// object of an empty class type derived from another (empty) class type.  This
// could lead to aborts, particularly when attempting to perform a cast from the
// empty base class to the empty derived class.
//
// This is now fixed.
struct B {};
struct D: B {};
constexpr int f() {
  D d = D();
  D *p = (D*)(B*)&d;  // Previously aborted.  Now okay.
  return 42;
}
constexpr int r = f();

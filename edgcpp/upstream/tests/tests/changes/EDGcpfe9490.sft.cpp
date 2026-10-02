//type:fp
//remark:[4.1] Spurious error on member function call in presence of selective overriders
// 2/3/09   [EDGcpfe/9490]
//
// Spurious error on member function call in presence of selective overriders
//
// In Microsoft mode, a call to a member function could result in a spurious
// error if that member was part of an overload set containing multiple selective
// overriders (a Microsoft extension).
//
// This is now fixed.
__interface I1 { void f(); };
__interface I2 { void f(); };
struct D: I1, I2 {
  void I1::f() {}
  void I2::f() {}
  int f(int);
} d;
int x = d.f(3);  // Previously resulted in a spurious error.

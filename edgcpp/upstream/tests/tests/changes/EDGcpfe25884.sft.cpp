//type:fn
//options_all:--c++20 --gn 110100
//remark:[6.5] GNU C++ compatibility: Relaxed checking for abstract class types
// 1/6/23   [EDGcpfe/25884]
//
// GNU C++ compatibility: Relaxed checking for abstract class types
//
// The changes for EDGcpfe/25234 in version 6.4 introduced a regression causing
// the front end to fail to report certain invalid uses of prvalues of abstract
// class type.
//
// That is now fixed.
struct S { virtual void f() = 0; };
S g();
S h() {        // Previously failed to be diagnosed.  Now an error again.
  return g();  // Ditto.
}

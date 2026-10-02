//type:fp
//options_all:--c++14
//remark:[4.12] Spurious error on aggregate initialization with default member initializers
// 9/27/16  [EDGcpfe/17379]
//
// Spurious error on aggregate initialization with default member initializers
//
// In C++14 mode, some braced initializations of aggregate classes with default
// member initializers produced a spurious error.
//
// This is now fixed.
template<typename T> struct A { T m = 0; };
void g() {
  A<int> x{};  // Previously triggered a spurious error in C++14 mode.
}              // Now okay.

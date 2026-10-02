//type:fp
//options_all:--c++11
//remark:[4.12] Spurious error on final modifiers in class templates
// 6/13/16  [EDGcpfe/17305]
//
// Spurious error on final modifiers in class templates
//
// The front end previously issued spurious errors on some members of class
// template with final modifiers.
//
// This is now fixed.
struct B {
  virtual void f(int) = 0;
};
template<typename T> struct D: B {
  void f(T) final {}  // Previously triggered a spurious error.  Now okay.
};

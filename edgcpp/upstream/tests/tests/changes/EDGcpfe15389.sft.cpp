//type:fp
//options_all:--c++11
//remark:[4.10] Spurious diagnostic on override modifiers
// 9/2/14   [EDGcpfe/15389]
//
// Spurious diagnostic on override modifiers
//
// In modes supporting the "override" and/or "new" member function modifiers, the
// front end sometimes issued diagnostics when such a modifier appears in a class
// template.
//
// This is now fixed.
struct B {
  virtual void f(int);
  virtual void f(char);
};
template<typename T> struct D: B {
  void f(T) override;  // Previously triggered an error and a warning.
};                     // Now accepted.

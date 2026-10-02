//type:fp
//options_all:--microsoft
//remark:[4.9] Microsoft compatibility: explicit override of a cv-qualified member function
// 4/1/14   [EDGcpfe/14980]
//
// Microsoft compatibility: explicit override of a cv-qualified member function
//
// A spurious error had been reported when using the Microsoft explicit override
// mechanism to override a cv-qualified member function.  Now fixed.
// (with --microsoft):
struct A {
  virtual void f(void) const = 0;
};
struct B: A {
  virtual void A::f(void) const override {}
};

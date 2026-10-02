//type:fp
//options_all:--c++14
//remark:[5.0] Abstract classes and virtual bases
// 10/26/17 [EDGcpfe/18833]
//
// Abstract classes and virtual bases
//
// Core issue 1658 clarified that the generated constructor or destructor of an
// abstract class does not initialize its virtual bases.  This affects whether
// certain generated members are deleted or not.
//
// Previously, Y's generated default constructor was deleted because X's default
// constructor is inaccessible from Y.  That in turn caused Z's default
// constructor to be deleted, which triggered an error for the initialization of
// variable z.  Now, the case is accepted because the default constructor of Y
// does not attempt to initialize its virtual base subobject.
struct Z;
class X {
  X() {}
  friend struct Z;
};
struct Y: virtual X {
  virtual void f() = 0;  // Y is an abstract class with a virtual base.
};
struct Z: Y {
  virtual void f();
};
Z z; // Previously an error.

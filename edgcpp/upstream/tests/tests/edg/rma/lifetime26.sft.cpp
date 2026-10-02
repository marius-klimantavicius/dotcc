//options_all:-r -x -tused
//options: --strict;cp

// object lifetime around array construction in constructor
struct C {
  C(int);
  ~C();
  operator int();
};
struct A {
  A(int = C(1));
  ~A();
};
struct B {
  A a[10];
  B(const B&) {}
};



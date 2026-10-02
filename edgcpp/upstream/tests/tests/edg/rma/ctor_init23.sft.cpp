//options_all:-r -x -tused
//options: --strict;cn:;cn

// POD
struct A {
  const int i;
};

// non-POD with trivial default constructor
struct B {
private:
  const int i;
};

// non-POD with nontrivial implicitly-declared default constructor
struct C {
  const int i;
  virtual void f();
};

// non-POD with user-declared default constructor
struct D {
  const int i;
  D();
};

A a1;
B b1;
C c1;
D d1;

const A a2;
const B b2;
const C c2;
const D d2;







//options_all:-r -x -tused
//options: --strict;cp

// Destructor IL lowering
struct A {virtual int f(); ~A();};
struct B {int i; virtual int f(); ~B();};
struct C : public A, virtual public B {int f(); ~C();};
struct X {int xx; ~X();};
struct D : public C {X x; int f(); ~D();};
A::~A() {
  f();
}
B::~B() {
  f();
}
C::~C() {
  f();
}
D::~D() {
  A aa;
  if (1) {
    f();
    return;
  } else {
    f();
  }  /* if */
}
D ddd;


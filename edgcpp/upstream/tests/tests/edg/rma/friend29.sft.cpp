//options_all:-r -x -tused
//options: --strict;cn

class A {
  void f();
};
class B : public A {
  friend void B::f();
  friend void A::g();
};
void B::f() { }
void A::g() { }


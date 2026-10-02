//options_all:-r -x -tused
//options: --strict;cn:;cp

struct C {
  C(int);
  ~C();
  operator int();
};
struct A {
  A(int = C(1));
  A(const A&, int = C(1));
  ~A();
};
struct B {
  B();
  A a;
};
main () {
  B b;
  B bb = b;
}



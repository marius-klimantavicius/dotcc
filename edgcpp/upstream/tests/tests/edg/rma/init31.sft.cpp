//options_all:-r -x -tused
//options: --strict;cp

struct A {
  int i;
};
struct B {
  A a;
  int i;
};
struct C {
  void f();
  int i;
};
struct D {
  int A::* pmi;
};
struct E {
  D d;
};
struct F {
  int & ri;
};
struct G {
  F f;
};


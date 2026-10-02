//options_all:-r -x -tused
//options: --strict;cn

struct A {
  const int i;
};
A a;
struct B : public A {
  int x;
//  B();
};
B b;
struct AA {
  const int i;
  AA(int);
};
AA aa(0);
struct BB : public AA {
  int x;
  BB() { };
};
BB bb;


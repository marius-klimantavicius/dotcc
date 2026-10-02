//options_all:-r -x -tused
//options: --strict;cp

struct A {
  int i, ii;
};

struct B : private A {
public:
  A::i;
  struct i { int n; };
  struct ii { int n; };
  A::ii;
};

struct BB : public B {
  void f();
  void g();
};
void BB::f() { i = ii; }
void BB::g() { struct i x; struct ii y; x.n = y.n; }



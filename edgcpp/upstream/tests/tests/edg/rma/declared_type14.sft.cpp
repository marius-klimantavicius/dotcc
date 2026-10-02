//options_all:-r -x -tused
//options: --strict;cp

struct C {
  C(int);
  ~C();
  operator int();
};
struct A {
  A(int = C(1));
  ~A();
};


//options_all:-r -x -tused
//options: --strict;cn

struct A { } a1;
struct B {
  int i;
  A a;
};
B b1 = { 1, {} };
B b2 = { 1, 0 };
B b3 = { 1, a1 };
B b4[] = { 1, {}, 1, 0, 1, a1 };
B b5[] = { { 1, {} }, { 1, 0 }, {1, a1 } };


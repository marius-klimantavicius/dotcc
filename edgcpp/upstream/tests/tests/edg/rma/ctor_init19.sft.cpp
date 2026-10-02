//options_all:-r -x -tused
//options: --strict;cn

struct S { S(); S(int); int i; };
class A {
  int a[3];
  S s[3];
  A() : a(0,0,0), s(0,0,0) { }
};


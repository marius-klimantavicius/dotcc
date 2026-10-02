//options_all:-r -x -tused
//options: --strict;fn

struct X { };
struct Y { };
struct Z { };
struct A {
  typedef int X;
};
struct S : A {
  typedef int Y;
  int Z;
  class X *px1;
  ::X *px2;
  X *pi1;
  class Y *py1;
  ::Y *py2;
  Y *pi2;
  class Z *pz1;
  ::Z *pz2;
};


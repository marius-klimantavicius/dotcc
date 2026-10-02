//options_all:-r -x -tused
//options: --strict;cn

class A {
  static int f();
  int g();
};
class B {
  friend static int A::f();  // error, according to Cfront
  friend int A::g();
};
class C {
  friend int A::f();
  friend int A::g();
};
class D {
  friend static int A::g();  // error, according to Cfront
  friend static int A::f();  // error, according to Cfront
};
static int f();
extern int g();
class E {
  friend static int f();         // error, according to Cfront
  friend extern int g();         // error, according to Cfront
};



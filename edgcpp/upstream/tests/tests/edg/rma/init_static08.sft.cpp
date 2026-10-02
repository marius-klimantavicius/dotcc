//options_all:-r -x -tused
//options: --strict;cn

struct S {
  static int a;
  static int b;
  static void (*pf1)();
  static void (*pf2)();
  static void (S::* pmf1)();
  static void (S::* pmf2)();
};
int S::a(1);
int S::b(int);
void (*S::pf1)()(0);
void (*S::pf2)()(int);
void (S::*S::pmf1)()(0);
void (S::*S::pmf2)()(int);


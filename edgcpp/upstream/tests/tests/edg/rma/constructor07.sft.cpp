//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

class A {
  A(), ~A(), f(),
  A(int), A(char), i;
};
class B {
  ~B(), B();
};
class C {
  int C;
  C();
  ~C(), C(int);
};


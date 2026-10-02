//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

class A {
  void f(int);
  friend A::f() { }
}


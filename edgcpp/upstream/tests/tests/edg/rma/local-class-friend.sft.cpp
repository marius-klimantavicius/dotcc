//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() {
  class A {
    friend void g(A&) { }
  };
  A a;
  g(a);
}


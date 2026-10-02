//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

main() {
  class A {
    enum { e1, e2 };
    friend class B;
  };
  struct B {
    static int f() { return A::e1; }
  };
  int i = B::f();
}


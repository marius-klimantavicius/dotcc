//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

void s03() {
  struct A {
    int i;
  };
  static struct A a = {32};
  struct B {
    static int f() { return 1; }
  };
}


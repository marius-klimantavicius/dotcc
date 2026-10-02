//options_all:-r -x -tused
//options: --strict;cn: --diag_suppress=102;cp

namespace N {
  class A {
    void f(enum E1);
    class X *p;
    enum E *q;
  };
  void f(enum E2);
}


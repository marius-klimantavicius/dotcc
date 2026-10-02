//options_all:-r -x -tused
//options: --strict;cn:;cn

namespace N {
  void f(int);
  void g() {
    int fff;
    class A {
      friend void f();
      friend void ff();
      friend void fff();
    };
  }
  void ff();
}


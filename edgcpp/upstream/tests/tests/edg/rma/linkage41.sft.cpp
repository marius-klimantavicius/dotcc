//options_all:-r -x -tused
//options: --strict;cn

extern "C" {
  void callfoo() {
    extern void foo();
    foo();
  }
}

extern void foo() {}


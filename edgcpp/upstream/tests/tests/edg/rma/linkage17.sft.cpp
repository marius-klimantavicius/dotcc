//options_all:-r -x -tused
//options: --strict;cn

namespace N {
  extern "C" int x;
}
extern "C" {
  void f() {
    int x();
  }
}


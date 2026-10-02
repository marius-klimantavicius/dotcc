//options_all:-r -x -tused
//options: --strict;cn:;ln

namespace N {
  extern "C" {
    void f() {
      extern void g();
      g();
    }
  }
}
extern "C" void g();
main() {
  g();
}


//options_all:-r -x -tused
//options: --strict;cn:;rp

namespace N {
  extern "C" void f() { }
}
void f() {
  N::f();
}
main() {
  f();
}


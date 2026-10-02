//options_all:-r -x -tused
//options: --strict;cp

extern "C" {
  void f();
}
void g() {
  extern void f();
  f();
}


//options_all:-r -x -tused
//options: --strict;cn

typedef void T();

extern "C" {
  void f();
}
void g() {
  extern T f;         // Error
}

extern "C" {
  void ff();
  void gg() {
    extern T ff;     // Error
  }
}


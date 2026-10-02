//options_all:-r -x -tused
//options: --strict;cn

extern "C" {
  void f() {
    void g();
  }
}
extern "C++" {
  void g() { }
}


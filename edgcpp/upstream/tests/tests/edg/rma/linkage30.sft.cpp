//options_all:-r -x -tused
//options: --strict;cn

extern "C" {
  void f() {
    extern void g();
    extern void h();
  }
}
extern "C++" void g();
extern "C++" int h();


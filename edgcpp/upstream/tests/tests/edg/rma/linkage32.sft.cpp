//options_all:-r -x -tused
//options: --strict;cn

int f();
int g();
extern "C" {
  void f() {
    extern void g();
  }
  void ff() {
    extern void gg();
  }
}
int gg();
int ff();


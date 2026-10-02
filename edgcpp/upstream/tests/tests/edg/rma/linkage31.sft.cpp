//options_all:-r -x -tused
//options: --strict;cp

int f();
int g();
namespace N {
  extern "C" {
    void f() {
      extern void g();
    }
    void ff() {
      extern void gg();
    }
  }
}
int gg();
int ff();


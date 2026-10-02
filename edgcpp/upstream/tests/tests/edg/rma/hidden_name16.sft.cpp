//options_all:-r -x -tused
//options: --strict;cp:;cp

int f();
namespace N {
  int f();
  void g() {
    class C {
      friend int ::f();
    };
    if (f()) {
      (void)::f();
    }
  }
}



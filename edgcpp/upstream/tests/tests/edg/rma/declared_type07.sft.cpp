//options_all:-r -x -tused
//options: --strict;cp

namespace N {
  void f() { 
    extern void g(float);
  }
  void g(float x=0) { }
}


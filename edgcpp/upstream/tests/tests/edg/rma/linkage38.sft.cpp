//options_all:-r -x -tused
//options: --strict;cp:;cp

namespace {
  extern "C" {
    void g();
  }
  void g() { }
}


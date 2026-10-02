//options_all:-r -x -tused
//options: --strict;cp

namespace N {
  extern "C" void f();
  extern "C" void g();
}
using N::f;
extern "C" void N::g() { }


//options_all:-r -x -tused
//options: --strict;cn

extern "C" void f();
extern "C++" int f();    // error

namespace N {
  extern "C" void g();
}
extern "C++" int g();   // no error


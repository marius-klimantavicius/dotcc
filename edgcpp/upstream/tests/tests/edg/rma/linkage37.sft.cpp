//options_all:-r -x -tused
//options: --strict;cn:;cp

namespace {
  extern "C" void f();
};
struct S {
  friend void f();
};
namespace {
  extern "C" void f() { }
}


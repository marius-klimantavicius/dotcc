//options_all:-r -x -tused
//options: --strict;cn

namespace N {
  extern "C" int x = 0;
  extern "C" int y;
  extern "C" int z;
}
namespace M {
  extern "C" int x = 1;
  extern "C" int y = 0;
  extern "C" void z();
}
extern "C" int x = 2;


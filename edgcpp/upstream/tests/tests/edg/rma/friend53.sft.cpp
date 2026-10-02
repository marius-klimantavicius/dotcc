//options_all:-r -x -tused
//options: --strict;cp

struct A;
namespace N {
  class X {
    friend struct A;
  };
  struct A *p;
}


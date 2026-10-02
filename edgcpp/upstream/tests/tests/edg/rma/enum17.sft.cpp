//options_all:-r -x -tused
//options: --strict;cn: --diag_suppress=102;cp

namespace N {
  struct S {
    enum E *p;         // type is pointer to N::S::E
    struct X *q;       // type is pointer to N::X
  };
  enum E { x,y,z };
  struct X { };
}


//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

class C { } c;
namespace N {
  class X {
    friend class C g() { return c; }  // Incorrect lookup of "C"
  };
}


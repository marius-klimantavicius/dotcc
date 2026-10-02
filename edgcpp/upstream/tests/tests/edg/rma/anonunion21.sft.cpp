//options_all:-r -x -tused
//options: --strict;fn

// C++ generating back end bug...
static union {
  typedef int T;
};
T x;
namespace N {
  static union {
    typedef int T;
  };
  T x;
}
void f() {
  static union {
    typedef int T;
  };
  T x;
}


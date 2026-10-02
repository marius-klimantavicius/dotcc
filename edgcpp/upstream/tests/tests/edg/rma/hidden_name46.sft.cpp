//options_all:-r -x -tused
//options: --strict;cp

namespace N {
  template <class T> struct A { };
  class B {
    int A;
    N::A<int> x;
  };
}
class C {
  int A;
  N::A<int> x;
};



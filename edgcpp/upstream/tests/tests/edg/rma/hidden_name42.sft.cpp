//options_all:-r -x -tused
//options: --strict;cp

namespace N {
  template <class T> class A { };
  void f() {
    int A = 0;
    N::A<int> x;
    struct S : N::A<char> { };
  }
  struct S {
    int A;
    struct B : N::A<float> { };
  };
}


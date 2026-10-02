//options_all:-r -x -tused
//options: --strict;cn:;cp

// EDGqa01679
namespace N {
  struct Y {
    typedef int A;
    int a;
  };
}
using namespace N;
template <class T> struct X {
  void foo() {}
};
struct Y {
  typedef int B;
  int b;
};
void foo() {
  X<::Y::B> x;
  x.foo();
}


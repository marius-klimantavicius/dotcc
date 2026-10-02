//options_all:-r -x -tused
//options: --strict;cp

// EDGqa01365
namespace N {
  template <class T> struct A {
    A();
  };
  template <class X, class T = A<X> > struct B {
    B(const T& = T());
    int A;
  };
}
N::B<char> b;


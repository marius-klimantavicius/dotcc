//options_all:-r -x -tused
//options: --strict;cp

template <class T, int n> struct A {
  A(int=0);
};

class X {};
template <int n> struct B : public A<int,n>, public A<X,n> {
  B(int=0);
};
template <int n> B<n>::B(int y) : A<int,n>(y), A<X,n>() {}

B<10> b;


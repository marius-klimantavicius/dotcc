//options_all:-r -x -tused
//options: --strict;cp

template <class T> struct A {
  A(int,int=0);
  A(const A&,int=0);
};
extern int i, j;
A<int> x1(A<int>(i));              // function decl
A<int> x2(A<int>(i,j));            // variable init
A<int> x3(A<int>(int(i)));         // variable init
A<int> y(A<int>(i),0);             // variable init
A<int> z(A<int>(i),int(j));        // function decl

template <class T> struct B {
  struct N {
    N(int,int=0);
    N(const N&,int=0);
  };
};
extern int i, j;
B<int>::N xx1(B<int>::N(i));              // function decl
B<int>::N xx2(B<int>::N(i,j));            // variable init
B<int>::N xx3(B<int>::N(int(i)));         // variable init
B<int>::N yy(B<int>::N(i),0);             // variable init
B<int>::N zz(B<int>::N(i),int(j));        // function decl



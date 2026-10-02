//options_all:-r -x -tused
//options: --strict;cn

typedef void F(int);
typedef const F FF;
struct S {
  F f1;
  FF f2;
  F f3 { }
  FF f4 { }
};
F S::f1 { }
FF S::f2 { }
const F g1;
const F g1 { }
FF g2;
FF g2 { }


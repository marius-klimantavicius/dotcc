//type:fn
//options:--c++17 -tused

namespace minimal
{
  template<int &> struct C { };
  struct B {
    int i, j;
  } b;
  C<b.j> c;
}

namespace pointer_arith
{
  template<int &> void fr() { }
  template<int *> void fp() { }

  template<int &> int vr = 0;
  template<int *> int vp = 0;

  template<int &> struct AR { };
  template<int *> struct AP { };

  struct C
  {
    int i;
    int j;
  };

  C c;

  int x = (fr<c.i>(),
           fp<&c.i>(),
           fr<c.j>(),
           fp<&c.j>(),
           vr<c.i>,
           vp<&c.i>,
           vr<c.j>,
           vp<&c.j>,
           AR<c.i>(),
           AP<&c.i>(),
           AR<c.j>(),
           AP<&c.j>(),
           0);
}

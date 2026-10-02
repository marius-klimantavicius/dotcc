//type:fn
//options:--c++20

namespace pointer_arith
{
  template<int *> void fp() { }
  template<int *> int vp = 0;
  template<int *> struct AP { };

  struct C
  {
    int i;
    int j;
  };

  C c;

  int x = (fp<&c.i + 1>(),
           fp<&c.j - 1>(),     // error
           fp<&c.j + 1>(),
           vp<&c.i + 1>,
           vp<&c.j - 1>,       // error
           vp<&c.j + 1>,
           AP<&c.i + 1>(),
           AP<&c.j - 1>(),     // error
           AP<&c.j + 1>(),
           0);
}

namespace member_subobject
{
  struct D
  {
    int *p;
  };

  template<D> void f() { }
  template<D> int v = 0;
  template<D> struct A { };

  struct C
  {
    int i;
    int j;
  };

  C c;

  int x = (f<D{&c.i + 1}>(),
           f<D{&c.j - 1}>(),   // error
           f<D{&c.j + 1}>(),
           v<D{&c.i + 1}>,
           v<D{&c.j - 1}>,     // error
           v<D{&c.j + 1}>,
           A<D{&c.i + 1}>(),
           A<D{&c.j - 1}>(),   // error
           A<D{&c.j + 1}>(),
           0);
}

namespace member_string_literal
{
  struct D
  {
    const char *s;
  };

  template<D> void f() { }
  template<D> int v = 0;
  template<D> struct A { };

  int x = (f<D{""}>(),          // error
           v<D{""}>,            // error
           A<D{""}>(),          // error
           0);
}

namespace array
{
  template<int &> void fr() { }
  template<int *> void fp() { }
  template<int &> int vr = 0;
  template<int *> int vp = 0;
  template<int &> struct AR { };
  template<int *> struct AP { };

  struct C
  {
    int i[2];
    int j[2];
  };

  C c;

  int x = (fr<c.i[2]>(),       // error
           fp<&c.i[2]>(),
           fp<&c.i[2] + 0>(),
           fp<&c.i[1] + 1>(),
           fr<c.j[2]>(),       // error
           fp<&c.j[2]>(),
           fp<&c.j[2] + 0>(),
           fp<&c.j[1] + 1>(),
           vr<c.i[2]>,         // error
           vp<&c.i[2]>,
           vp<&c.i[2] + 0>,
           vp<&c.i[1] + 1>,
           vr<c.j[2]>,         // error
           vp<&c.j[2]>,
           vp<&c.j[2] + 0>,
           vp<&c.j[1] + 1>,
           AR<c.i[2]>(),       // error
           AP<&c.i[2]>(),
           AP<&c.i[2] + 0>(),
           AP<&c.i[1] + 1>(),
           AR<c.j[2]>(),       // error
           AP<&c.j[2]>(),
           AP<&c.j[2] + 0>(),
           AP<&c.j[1] + 1>(),
           0);
}

namespace unknown_bound_array
{
  template<int *> void fp() { }

  extern int arr[];

  int x = (fp<&arr[1]>(),
           1);
}

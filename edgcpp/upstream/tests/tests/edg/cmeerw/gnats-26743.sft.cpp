//type:rp
//options:--c++20 -tused

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
           fp<&c.i + 0>(),
           fp<&c.i + 1 - 1>(),
           fr<c.j>(),
           fp<&c.j>(),
           fp<&c.j + 0>(),
           fp<&c.j + 1 - 1>(),
           vr<c.i>,
           vp<&c.i>,
           vp<&c.i + 0>,
           vp<&c.i + 1 - 1>,
           vr<c.j>,
           vp<&c.j>,
           vp<&c.j + 0>,
           vp<&c.j + 1 - 1>,
           AR<c.i>(),
           AP<&c.i>(),
           AP<&c.i + 0>(),
           AP<&c.i + 1 - 1>(),
           AR<c.j>(),
           AP<&c.j>(),
           AP<&c.j + 0>(),
           AP<&c.j + 1 - 1>(),
           0);
}

namespace member_subobject
{
  struct D
  {
    int &r;
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

  int x = (f<D{c.i, &c.i}>(),
           f<D{c.i, &c.i + 0}>(),
           f<D{c.i, &c.i + 1 - 1}>(),
           f<D{c.j, &c.j}>(),
           f<D{c.j, &c.j + 0}>(),
           f<D{c.j, &c.j + 1 - 1}>(),
           v<D{c.i, &c.i}>,
           v<D{c.i, &c.i + 0}>,
           v<D{c.i, &c.i + 1 - 1}>,
           v<D{c.j, &c.j}>,
           v<D{c.j, &c.j + 0}>,
           v<D{c.j, &c.j + 1 - 1}>,
           A<D{c.i, &c.i}>(),
           A<D{c.i, &c.i + 0}>(),
           A<D{c.i, &c.i + 1 - 1}>(),
           A<D{c.j, &c.j}>(),
           A<D{c.j, &c.j + 0}>(),
           A<D{c.j, &c.j + 1 - 1}>(),
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

  int arr[2];
  int arr2[3][4];

  int x = (fr<arr[0]>(),
           fr<arr[1]>(),
           fp<&arr[0]>(),
           fp<&arr[1] + 0>(),
           fp<&arr[0] + 1>(),
           fr<arr2[2][3]>(),
           vr<arr[0]>,
           vr<arr[1]>,
           vp<&arr[0]>,
           vp<&arr[1] + 0>,
           vp<&arr[0] + 1>,
           vr<arr2[2][3]>,
           AR<arr[0]>(),
           AR<arr[1]>(),
           AP<&arr[0]>(),
           AP<&arr[1] + 0>(),
           AP<&arr[0] + 1>(),
           AR<arr2[2][3]>(),
           0);
}

namespace array_member
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
    int arr[3][4];
  };

  C c;

  int x = (fr<c.i[0]>(),
           fp<&c.i[0]>(),
           fp<&c.i[0] + 0>(),
           fp<&c.i[0] + 1 - 1>(),
           fr<c.i[1]>(),
           fp<&c.i[1]>(),
           fp<&c.i[1] + 0>(),
           fp<&c.i[1] + 1 - 1>(),
           fr<c.j[0]>(),
           fp<&c.j[0]>(),
           fp<&c.j[0] + 0>(),
           fp<&c.j[0] + 1 - 1>(),
           fr<c.j[1]>(),
           fp<&c.j[1]>(),
           fp<&c.j[1] + 0>(),
           fp<&c.j[1] + 1 - 1>(),
           fr<c.arr[2][3]>(),
           vr<c.i[0]>,
           vp<&c.i[0]>,
           vp<&c.i[0] + 0>,
           vp<&c.i[0] + 1 - 1>,
           vr<c.i[1]>,
           vp<&c.i[1]>,
           vp<&c.i[1] + 0>,
           vp<&c.i[1] + 1 - 1>,
           vr<c.j[0]>,
           vp<&c.j[0]>,
           vp<&c.j[0] + 0>,
           vp<&c.j[0] + 1 - 1>,
           vr<c.j[1]>,
           vp<&c.j[1]>,
           vp<&c.j[1] + 0>,
           vp<&c.j[1] + 1 - 1>,
           vr<c.arr[2][3]>,
           AR<c.i[0]>(),
           AP<&c.i[0]>(),
           AP<&c.i[0] + 0>(),
           AP<&c.i[0] + 1 - 1>(),
           AR<c.i[1]>(),
           AP<&c.i[1]>(),
           AP<&c.i[1] + 0>(),
           AP<&c.i[1] + 1 - 1>(),
           AR<c.j[0]>(),
           AP<&c.j[0]>(),
           AP<&c.j[0] + 0>(),
           AP<&c.j[0] + 1 - 1>(),
           AR<c.j[1]>(),
           AP<&c.j[1]>(),
           AP<&c.j[1] + 0>(),
           AP<&c.j[1] + 1 - 1>(),
           AR<c.arr[2][3]>(),
           0);
}

namespace nested_array_member
{
  struct B1
  {
    int j;
  };

  struct B2
  {
    int i;
    B1 b1[2];
  };

  struct D2 : B2
  {
    int k;
  };

  B2 b2;
  D2 d2;

  template<int &r>
  struct C { };

  C<b2.b1[1].j> b1j;
  C<d2.b1[1].j> d1j;
}

int main()
{ }

//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1950
//options_all:-w

namespace minimal
{
  template<typename, int> concept C = true;
  void f(auto t) {
    for (C<1> auto v : t) { }
  }
}

namespace constrained_auto
{
  template<typename T, int I>
  concept C = true;

  struct X
  {
    const int *begin();
    const int *end();
    operator bool() const;
  };

  void f(X x)
  {
    C<1> auto i = 1;

    if (C<1> auto v = x)
    { }

    if (C<int, 1>)
    { }

    if (C<int, 1>; C<2> auto v = x)
    { }

    if (C<1> auto c = 1; C<2> auto v = x)
    { }

    for (C<1> auto v = 1; false; )
    { }

    for (C<1> auto v : x)
    { }

    for (C<int, 1>; C<2> auto v : x)
    { }

    for (C<1> auto c = 1; C<2> auto v : x)
    { }
  }
}

namespace expansion_in_for_stmt
{
  template<typename T, int I>
  concept C = true;

  template<int I>
  concept B = true;

  template<int I>
  struct D
  {
    D(int);
  };

  struct X
  {
    const int *begin();
    const int *end();
    operator bool() const;
  };

  template<int ... I>
  void f(X x)
  {
    for ((C<int, I> && ...); false; )
    { }

    for (B<(I + ... )>; int i : x)
    { }

    for (D<(I + ... )> d : x)
    { }

    for (C<(I + ... )> auto j : x)
    { }

    for ((C<int, I> && ...); C<(I + ... )> auto j : x)
    { }
  }

  template void f<1, 2, 3>(X);
}

namespace refer_to_declared_name
{
  void f()
  {
    for (int i = 1, j = i + decltype(i)(); false; )
    { }

    if (int i = 1, j = i + decltype(i)(); false)
    { }
  }
}

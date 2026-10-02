//type:fn
//options:--c++26 -tused -w

namespace templ_definition
{
  template<int N, int I>
  void f()
  {
    I...[N];             // error: non variadic context
  }

  template<int N, int ... I>
  void g()
  {
    N...[N];             // error: must be a pack
    x...[N];             // error: must be a pack
    I...[I];             // error: pack not expanded
  }
}

namespace out_of_bounds
{
  template<int I, typename ... Ts>
  auto f1()
  {
    return Ts ... [I]{};        // error: exceeds number of pack elements
  }

  int i = f1<4, int>() + f1<1>();

  template<int I, int ... Vs>
  auto f2()
  {
    return Vs ... [I];          // error: exceeds number of pack elements
  }

  int j = f2<4, 1>() + f2<1>();

  template<typename... Ts>
  struct C
  {
    template<int I>
    static auto f() -> Ts ... [I]; // error: exceeds number of pack elements
  };

  int k = C<>::f<1>();
}

namespace out_of_bounds_recover
{
  template<typename...>
  struct C {};

  template<int...>
  struct S {};

  template <typename... Ts, int... Is>
  auto f(S<Is...>)
  {
    return C<Ts...[Is]...>();   // error: exceeds number of pack elements
  }

  auto v0 = f<>(S<2>());
  auto v1 = f<int>(S<2>());
  auto v2 = f<int, long>(S<2>());
}

namespace base_class
{
  template<typename ... Ts>
  struct D : Ts ... [0]         // error: not a class or struct name
  { };

  template struct D<int>;
}

namespace member_initializer
{
  template<typename ... Ts>
  struct D
  {
    D() : Ts ... [0]()          // error: invalid base class
    { };
  };

  template struct D<int>;
}

namespace non_constant_index
{
  template<typename ... Ts>
  void f1(int i)
  {
    Ts ... [i] t;               // error: expression not constant
  }

  template void f1<int>(int);

  template<int ... Is>
  void f2(int i)
  {
    Is ... [i];                 // error: expression not constant
  }

  template void f2<1>(int);
}

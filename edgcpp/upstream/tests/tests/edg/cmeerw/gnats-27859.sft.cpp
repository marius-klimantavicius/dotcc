//type:fp
//options:--c++17:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w -tused

namespace minimal
{
  template<int I1, int I2>
  struct C {
    template<int ... Js>
    friend constexpr int f(C<Js...>) {
      return (Js + ...);
    }
  };
  static_assert(f(C<1, 2>()) == 3);
}

namespace deduced_return_type
{
  template<int I1, int I2>
  struct C
  { };

  template<int I1, int I2>
  struct Obj
  {
    template<int... Js>
    friend auto f(Obj<Js...>)
    {
      return C<Js...>{};
    }

    template<int... Js>
    static auto s(Obj<Js...>)
    {
      return C<Js...>{};
    }
  };

  C<1, 2> v1 = Obj<1, 2>::s(Obj<1, 2>{});
  C<1, 2> v2 = f(Obj<1, 2>{});
}

namespace substitute_into_type
{
  template<int I>
  struct D
  { };

  template<>
  struct D<3>
  {
    using type = int;
  };

  template<int I1, int I2>
  struct C
  {
    template<int ... Js>
    friend constexpr auto f(C<Js...>) -> typename D<(Js + ...)>::type
    {
      return (Js + ...);
    }
  };

  static_assert(f(C<1, 2>()) == 3);
}

namespace mixed_substitution
{
  template<int I>
  struct D
  { };

  template<>
  struct D<6>
  {
    using type = int;
  };

  template<int ... Is>
  struct C
  {
    template<int ... Js>
    friend constexpr auto f(C<Js...>) -> typename D<((Js + Is) + ...)>::type
    {
      return ((Js + Is) + ...);
    }
  };

  static_assert(f(C<1, 2>()) == 6);
}

namespace friend_decl_only
{
  template<typename T> struct A
  {
    template<typename U>
    friend int f(A<U>);
  };

  template<typename T>
  int f(A<T>)
  {
    return 0;
  }

  int i = f(A<int>());
}

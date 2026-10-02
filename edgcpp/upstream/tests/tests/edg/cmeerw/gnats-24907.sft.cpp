//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;


namespace minimal
{
  template<typename U> struct C { };
  template<typename T> struct D {
    template<typename ... Us> requires requires { typename C<Us ...>; }
    struct N { };
  };
  D<char>::N<short> n;
}

namespace pack_only
{
  template<typename U>
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... Us> requires requires { typename C<Us...>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace outer_param_and_pack
{
  template<typename T, typename U>
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... Us> requires requires { typename C<T, Us...>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace pack_only_with_alias
{
  template<typename U>
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... Us>
    using A = C<Us ...>;

    template<typename ... Us> requires requires { typename A<Us...>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace pack_only_with_alias_default
{
  template<typename T, typename U = void>
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... Us>
    using A = C<Us ...>;

    template<typename ... Us> requires requires { typename A<Us...>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace pack_with_outer_param_and_alias
{
  template<typename T, typename U >
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... UAs>
    using A = C<UAs ...>;

    template<typename ... UNs> requires requires { typename A<UNs..., T>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace outer_param_with_pack_and_alias
{
  template<typename T, typename U>
  struct C
  { };

  template<typename T>
  struct D
  {
    template<typename ... Us>
    using A = C<Us ...>;

    template<typename ... Us> requires requires { typename A<T, Us...>; }
    struct N { };
  };

  D<char>::N<short> n;
}

namespace outer_and_inner_pack
{
  template<typename T1, typename T2, typename U1, typename U2>
  struct C
  {
    static_assert(is_same_v<T1, char[1]>);
    static_assert(is_same_v<T2, char[2]>);
    static_assert(is_same_v<U1, short[1]>);
    static_assert(is_same_v<U2, short[2]>);

    using type = int;
  };

  template<typename ... Ts>
  struct D
  {
    template<typename ... UAs>
    using A = C<UAs ...>;

    template<typename ... UNs> requires requires {
        typename A<Ts ..., UNs ...>::type;
      }
    struct N {
      A<Ts ..., UNs ...> a;
    };
  };

  D<char[1], char[2]>::N<short[1], short[2]> n;
}

namespace only_outer_pack
{
  template<typename T, typename U>
  struct C
  { };

  template<typename ... Ts>
  struct D
  {
    template<typename ... UAs>
    using A = C<UAs ...>;

    template<typename UN> requires requires { typename A<Ts (UN) ...>; }
    struct N { };
  };

  D<signed char, unsigned char>::N<int> n;
}

namespace mixed_pack_outer_and_inner
{
  template<typename T, typename U>
  struct C
  {
    static_assert(is_same_v<T, signed char(short)>);
    static_assert(is_same_v<U, unsigned char(int)>);

    using type = int;
  };

  template<typename ... Ts>
  struct D
  {
    template<typename ... UAs>
    using A = C<UAs ...>;

    template<typename ... UNs> requires requires { typename A<Ts (UNs) ...>; }
    struct N { };
  };

  D<signed char, unsigned char>::N<short, int> n;
}

namespace trailing_requires_clause
{
  template<typename U> struct C
  { };

  template<typename T> struct D
  {
    static void f() requires requires { typename C<typename T::type>; };

    struct F
    {
      static void f() requires requires { typename C<typename T::type>; };

      template<typename U>
      struct G
      {
        static void f() requires requires { typename C<typename T::type>; };
      };
    };
  };

  struct B {
    using type = int;
  };

  void f()
  {
    D<B>::f();
    D<B>::F::f();
    D<B>::F::G<int>::f();
  }
}

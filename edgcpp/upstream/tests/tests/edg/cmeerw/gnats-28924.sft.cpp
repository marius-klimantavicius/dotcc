//type:fp
//options:--c++20:--c++26:--ms_c++20 --microsoft_version 1950:--c++26 --gn 150200:--c++26 --clang_version 220100

namespace minimal
{
  template<typename, typename> constexpr bool is_same_v = false;
  template<typename T>         constexpr bool is_same_v<T, T> = true;
  template<typename T, int N>
  struct C {
    C(T*, auto ...);
  };
  template<typename T, typename... Us>
  C(T*, Us ...) -> C<T, sizeof ... (Us)>;
  template<typename T, int N>
  using A = C<T, N>;
  A a{"", 1, 2, 3};
  static_assert(is_same_v<decltype(a), A<const char, 3>>, "Unexpected");
}

namespace sizeof_pack
{
  template<int N>
  struct B
  {
    static constexpr int v = N;
  };

  template<typename T, typename U>
  struct C
  {
    C(T*, auto ...);
  };

  template<typename T, typename... Us>
  C(T*, Us...) -> C<T, B<sizeof...(Us)>>;

  template<typename T, int N>
  using A = C<T, B<N>>;

  template<typename H>
  struct X;

  template<typename T, int N>
  struct X<C<T, B<N>>> { static constexpr int v = N; };

  C c{"", 1, 2, 3};
  static_assert(X<decltype(c)>::v == 3, "Unexpected");

  A a{"", 1, 2, 3};
  static_assert(X<decltype(a)>::v == 3, "Unexpected");
}

namespace fold_expr
{
  template<int N> struct B { static constexpr int v = N; };
  template<int I> struct D {};

  template<typename T, typename U>
  struct C
  {
    C(T*, auto ...);
  };
  template<typename T, int ... Is>
  C(T*, D<Is> ...) -> C<T, B<(Is + ... + 0)>>;

  template<typename T, int v> using A = C<T, B<v>>;

  template<typename H> struct X;
  template<typename T, int N>
  struct X<C<T, B<N>>> { static constexpr int v = N; };

  C c{"", D<1>{}, D<2>{}, D<3>{}};
  static_assert(X<decltype(c)>::v == 6, "Unexpected");

  A a{"", D<1>{}, D<2>{}, D<3>{}};
  static_assert(X<decltype(a)>::v == 6, "Unexpected");

  A a1{"", D<5>{}};
  static_assert(X<decltype(a1)>::v == 5, "Unexpected");

  A a0{""};
  static_assert(X<decltype(a0)>::v == 0, "Unexpected");
}

#if defined(__cpp_pack_indexing)
namespace pack_indexing
{
  template<int N> struct B { static constexpr int v = N; };
  template<int I> struct D {};

  template<typename T, typename U>
  struct C {
    C(T*, auto ...);
  };
  template<typename T, int ... Is>
  C(T*, D<Is> ...) -> C<T, B<Is ...[1]>>;

  template<typename T, int N> using A = C<T, B<N>>;

  template<typename H> struct X;
  template<typename T, int N>
  struct X<C<T, B<N>>> { static constexpr int v = N; };

  C c{"", D<1>{}, D<2>{}, D<3>{}};
  static_assert(X<decltype(c)>::v == 2, "Unexpected");

  A a{"", D<1>{}, D<2>{}, D<3>{}};
  static_assert(X<decltype(a)>::v == 2, "Unexpected");
}
#endif

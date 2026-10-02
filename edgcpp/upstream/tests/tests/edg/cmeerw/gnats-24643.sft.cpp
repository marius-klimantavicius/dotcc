//type:fp
//options:--c++11 --g++:--c++20 --g++

namespace minimal
{
  template<typename T, T ...>
  struct S { };
  template<typename ... T>
  auto f(T ...) -> S<int, __integer_pack(sizeof ... (T))...>;
  auto v = f<int>(1, 2);
}

template<typename T1, typename T2>
struct is_same
{
  static constexpr bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static constexpr bool value = true;
};

namespace dependent_sizeof_pack_integer_pack
{
  template<typename T, T...>
  struct S
  { };

  template<typename ... T>
  auto f() -> S<int, __integer_pack(sizeof...(T))... >;

  auto v0 = f<>();
  static_assert(is_same<decltype(v0), S<int>>::value, "v0");

  auto v1 = f<int>();
  static_assert(is_same<decltype(v1), S<int, 0>>::value, "v1");

  auto v2 = f<int, char>();
  static_assert(is_same<decltype(v2), S<int, 0, 1>>::value, "v2");

  auto v3 = f<int, char, short>();
  static_assert(is_same<decltype(v3), S<int, 0, 1, 2>>::value, "v3");
}

namespace dependent_sizeof_pack_integer_pack_append
{
  template<typename T, T...>
  struct S
  { };

  template<typename ... T>
  auto f(T ... t) -> S<int, __integer_pack(sizeof...(T))... >;

  auto v0_0 = f<>();
  static_assert(is_same<decltype(v0_0), S<int>>::value, "v0_0");

  auto v0_1 = f<>(1);
  static_assert(is_same<decltype(v0_1), S<int, 0>>::value, "v0_1");

  auto v1_1 = f<int>(1);
  static_assert(is_same<decltype(v1_1), S<int, 0>>::value, "v1_1");

  auto v1_2 = f<int>(1, 2);
  static_assert(is_same<decltype(v1_2), S<int, 0, 1>>::value, "v1_2");

  auto v2_2 = f<int, char>(1, 2);
  static_assert(is_same<decltype(v2_2), S<int, 0, 1>>::value, "v2_2");

  auto v2_3 = f<int, char>(1, 2, 3);
  static_assert(is_same<decltype(v2_3), S<int, 0, 1, 2>>::value, "v2_3");

  auto v3_3 = f<int, char, short>(1, 2, 3);
  static_assert(is_same<decltype(v3_3), S<int, 0, 1, 2>>::value, "v3_3");

  auto v3_4 = f<int, char, short>(1, 2, 3, 4);
  static_assert(is_same<decltype(v3_4), S<int, 0, 1, 2, 3>>::value, "v3_4");
}

namespace dependent_integer_pack
{
  template<typename T, T...>
  struct S
  { };

  template<int N>
  struct C
  { };

  template<typename U, int N>
  auto g(C<N>) -> S<int, __integer_pack(N)... >;

  auto v0 = g<int>(C<0>());
  static_assert(is_same<decltype(v0), S<int>>::value, "v0");

  auto v1 = g<int>(C<1>());
  static_assert(is_same<decltype(v1), S<int, 0>>::value, "v1");

  auto v2 = g<int>(C<2>());
  static_assert(is_same<decltype(v2), S<int, 0, 1>>::value, "v2");

  auto v3 = g<int>(C<3>());
  static_assert(is_same<decltype(v3), S<int, 0, 1, 2>>::value, "v3");
}

//type:fp
//options:--c++20 --clang_version 220100
//options_all:-w

using INT = int;

#if !__has_builtin(__builtin_dedup_pack)
#error __has_builtin failed
#endif

namespace templ_args
{
  template<typename ... Ts>
  constexpr int f()
  {
    return sizeof ... (Ts);
  }

  template<typename ... Ts>
  constexpr int v = sizeof ... (Ts);

  // clang doesn't actually support this outside of templates
  static_assert(f<__builtin_dedup_pack<> ...>() == 0);
  static_assert(f<__builtin_dedup_pack<int[1]> ...>() == 1);
  static_assert(f<__builtin_dedup_pack<int[1], int[1]> ...>() == 1);
  static_assert(f<__builtin_dedup_pack<int[1], INT[1]> ...>() == 1);
  static_assert(f<__builtin_dedup_pack<int[1], const int[1]> ...>() == 2);
  static_assert(f<__builtin_dedup_pack<int[1], int[2]> ...>() == 2);
  static_assert(f<__builtin_dedup_pack<int[1], int[2], int[3]> ...>() == 3);

  static_assert(v<__builtin_dedup_pack<> ...> == 0);
  static_assert(v<__builtin_dedup_pack<int[1]> ...> == 1);
  static_assert(v<__builtin_dedup_pack<int[1], int[1]> ...> == 1);
  static_assert(v<__builtin_dedup_pack<int[1], INT[1]> ...> == 1);
  static_assert(v<__builtin_dedup_pack<int[1], const int[1]> ...> == 2);
  static_assert(v<__builtin_dedup_pack<int[1], int[2]> ...> == 2);
  static_assert(v<__builtin_dedup_pack<int[1], int[2], int[3]> ...> == 3);


  template<int I, typename ... Ts>
  struct C
  {
    static_assert(f<__builtin_dedup_pack<Ts ...> ...>() == I);
    static_assert(v<__builtin_dedup_pack<Ts ...> ...> == I);
  };

  void g()
  {
    { C<0> c; }
    { C<1, int[1]> c; }
    { C<1, int[1], int[1]> c; }
    { C<1, int[1], INT[1]> c; }
    { C<2, int[1], const int[1]> c; }
    { C<2, int[1], int[2]> c; }
    { C<3, int[1], int[2], int[3]> c; }
    { C<2, int[1], int[2], int[1]> c; }
  }
}

namespace base_class
{
  struct B1 { int b1; };
  struct B2 { int b2; };
  struct B3 { int b3; };

  using A1 = B1;
  using A2 = B2;
  using A3 = B3;

  template<typename ... Ts>
  struct D : __builtin_dedup_pack<Ts ...>...
  { };

  static_assert(__is_empty(D<>));
  static_assert(__is_base_of(B1, D<B1>));
  static_assert(__is_base_of(B1, D<B1, B1>));
  static_assert(__is_base_of(A1, D<B1>));
  static_assert(__is_base_of(A1, D<B1, B1>));
  static_assert(__is_base_of(B1, D<A1>));
  static_assert(__is_base_of(B1, D<A1, A1>));
  static_assert(__is_base_of(A1, D<A1, B1>));
  static_assert(__is_base_of(B1, D<A1, B1>));
  static_assert(D<B1>{ 1 }.b1 == 1);
  static_assert(D<B1, B1>{ 1 }.b1 == 1);

  static_assert(__is_base_of(B1, D<B1, B2>));
  static_assert(__is_base_of(B2, D<B1, B2>));
  static_assert(__is_base_of(B1, D<B1, B2, B1>));
  static_assert(__is_base_of(B2, D<B1, B2, B1>));
  static_assert(D<B1, B2>{ 1, 2 }.b1 == 1);
  static_assert(D<B1, B2>{ 1, 2 }.b2 == 2);
  static_assert(D<B1, B2, B1>{ 1, 2 }.b1 == 1);
  static_assert(D<B1, B2, B1>{ 1, 2 }.b2 == 2);
}

namespace virtual_base_class
{
  struct B1 { int b1; };
  struct B2 { int b2; };
  struct B3 { int b3; };

  template<typename ... Ts>
  struct D : virtual __builtin_dedup_pack<Ts ...>...
  { };

  static_assert(__is_empty(D<>));
  static_assert(__builtin_is_virtual_base_of(B1, D<B1>));
  static_assert(__builtin_is_virtual_base_of(B1, D<B1, B1>));

  static_assert(__builtin_is_virtual_base_of(B1, D<B1, B2>));
  static_assert(__builtin_is_virtual_base_of(B2, D<B1, B2>));
  static_assert(__builtin_is_virtual_base_of(B1, D<B1, B2, B1>));
  static_assert(__builtin_is_virtual_base_of(B2, D<B1, B2, B1>));
}

namespace substitution
{
  template<typename ... T>
  struct C
  { };

  template<typename ... T>
  auto f(T ...) -> C<__builtin_dedup_pack<T ...>...>;

  C<char, int> v = f('a', 1, 2, 'd');
}

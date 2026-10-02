//type:fp
//options:--c++20:--ms_c++20 --microsoft_version=1928

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

template<int I>
struct X { };

namespace trailing_pack
{
  template<typename ... T>
  struct D : T ...
  { };

  static_assert(is_same_v<decltype(D{}), D<>>);
  static_assert(is_same_v<decltype(D{X<1>{}}), D<X<1>>>);
  static_assert(is_same_v<decltype(D{X<1>{}, X<2>{}}), D<X<1>, X<2>>>);
}

namespace non_trailing_pack
{
  template<typename U, typename V, typename ... T>
  struct D : T ...
  {
    U u;
    V v;
  };

  static_assert(is_same_v<decltype(D{1, 2}), D<int, int>>);
  static_assert(is_same_v<decltype(D{X<1>{}, X<2>{}}), D<X<1>, X<2>>>);
}

namespace non_trailing_pack_deduced
{
  template<typename T>
  struct B
  { };

  struct X
  {
    template<typename T>
    operator B<T>() const;
  };

  template<typename ... T>
  struct C
  {
    operator X() const;
  };

  template<typename U, typename ... T>
  struct A : B<T> ... {
    U u;
    C<T ...> c;
  };

  static_assert(is_same_v<decltype(A{ X{}, C<int>{} }), A<X, int>>);
  static_assert(is_same_v<decltype(A{ X{} }), A<X>>);
}

namespace over_match_class_deduct_examples
{
  template <typename T>
  struct S {
    T x;
    T y;
  };

  template <typename T>
  struct C {
    S<T> s;
    T t;
  };

  template <typename T>
  struct D {
    S<int> s;
    T t;
  };

  C c3 = {{1u, 2u}, 3};           // OK, deduces C<int>
  static_assert(is_same_v<decltype(c3), C<int>>);

  D d2 = {1, 2, 3};               // OK, braces elided, deduces D<int>
  static_assert(is_same_v<decltype(d2), D<int>>);

  template <typename T>
  struct E {
    T t;
    decltype(t) t2;
  };

  E e1 = {1, 2};                  // OK, deduces E<int>
  static_assert(is_same_v<decltype(e1), E<int>>);

  template <typename... T>
  struct Types {};

  template <typename... T>
  struct F : Types<T...>, T... {};

  struct X {};
  struct Y {};
  struct Z {};
  struct W { operator Y(); };

  F f1 = {Types<X, Y, Z>{}, {}, {}};      // OK, F<X, Y, Z> deduced
  static_assert(is_same_v<decltype(f1), F<X, Y, Z>>);
  F f2 = {Types<X, Y, Z>{}, X{}, Y{}};    // OK, F<X, Y, Z> deduced
  static_assert(is_same_v<decltype(f2), F<X, Y, Z>>);
}

namespace array_member
{
  template<typename T, unsigned N>
  struct B
  {
    T arr[N];
  };

  static_assert(is_same_v<decltype(B{"Hello"}), B<char, 6>>);
}

namespace trailing_pack_elements_deduced
{
  template<typename... T>
  struct Types { };

  template<typename... T>
  struct F : Types<T...>, T...
  { };

  static_assert(is_same_v<decltype(F{}), F<>>);

  static_assert(is_same_v<decltype(F{Types<X<1>>{}}),
                F<X<1>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>>{}, {}}),
                F<X<1>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>>{}, X<1>{}}),
                F<X<1>>>);

  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, {}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, {}, {}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, X<1>{}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, X<1>{}, {}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, X<1>{}, X<2>{}}),
                F<X<1>, X<2>>>);
  static_assert(is_same_v<decltype(F{Types<X<1>, X<2>>{}, {}, X<2>{}}),
                F<X<1>, X<2>>>);
}

namespace trailing_and_non_trailing_pack
{
  template<typename T>
  struct Y
  {
    Y(T);
  };

  template<typename ... U>
  struct C : Y<U> ..., U ...
  { };

  static_assert(is_same_v<decltype(C{X<1>{}, X<2>{}}), C<X<1>, X<2>>>);
}

namespace deduce_non_trailing_pack_from_member_type
{
  template<typename ... T>
  struct Y
  {
    Y();
    operator X<1>() const;
  };

  template<typename ... U>
  struct C : U ...
  {
    Y<U ...> y;
  };

  static_assert(is_same_v<decltype(C{Y<X<1>, X<2>>{}}), C<X<1>, X<2>>>);
}

namespace brace_elision
{
  struct B
  {
    int i;
    int j;
  };

  template<typename T>
  struct C
  {
    B b;
    T t;
  };

  template<typename T>
  struct D : B
  {
    T t;
  };

  static_assert(is_same_v<decltype(C{ 1, 2, 3 }), C<int>>);
  static_assert(is_same_v<decltype(C{ 1, 2, 'a' }), C<char>>);

  static_assert(is_same_v<decltype(D{ 1, 2, 3 }), D<int>>);
  static_assert(is_same_v<decltype(D{ 1, 2, 'a' }), D<char>>);
}

namespace whole_aggr_class_init
{
  struct A
  {
    int i;
    int j;
  };

  struct B
  {
    operator A() const;
  };

  template<typename T>
  struct C
  {
    A a;
    T t;
  };

  static_assert(is_same_v<decltype(C{ B{}, 2 }), C<int>>);
  static_assert(is_same_v<decltype(C{ 1, 2, 3 }), C<int>>);

  static_assert(is_same_v<decltype(C{ B{}, 'b' }), C<char>>);
  static_assert(is_same_v<decltype(C{ 1, 2, 'c' }), C<char>>);
}

namespace union_member
{
  union U
  {
    int i;
    int j;
  };

  struct B
  {
    operator U() const;
  };

  template<typename T>
  struct C
  {
    U u;
    T t;
  };

  static_assert(is_same_v<decltype(C{ 1, 2 }), C<int>>);
  static_assert(is_same_v<decltype(C{ B{}, 2 }), C<int>>);
}

namespace indirect_base
{
  struct B1
  { int i; };

  template<typename T>
  struct B2 : B1
  { T j; };

  template<typename T, typename U>
  struct C : B2<U>
  {
    T t;
  };

  static_assert(is_same_v<decltype(C{ B2{ { 1 }, 2 }, 'a' }), C<char, int>>);
}

namespace deduce_from_string_literal
{
  template<typename T, unsigned N>
  struct C
  {
    T arr[N];
  };

  static_assert(is_same_v<decltype(C{ "Hello" }), C<char, 6>>);
}

namespace parenthesized_init
{
  template<typename T, typename U>
  struct C
  {
    T t;
    U u;
  };

  static_assert(is_same_v<decltype(C{ 1, 2 }), C<int, int>>);
  static_assert(is_same_v<decltype(C( 3, 4 )), C<int, int>>);
  static_assert(is_same_v<decltype(C( 'a', 5L )), C<char, long>>);
}

//type:fn
//options:--c++20:--ms_c++20 --microsoft_version=1927

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

  C c1 = {1, 2};                  // error: deduction fails
  C c2 = {1, 2, 3};               // error: deduction fails
  C c3 = {{1u, 2u}, 3};           // OK, deduces C<int>

  D d1 = {1, 2};                  // error: deduction fails
  D d2 = {1, 2, 3};               // OK, braces elided, deduces D<int>

  template <typename T>
  struct E {
    T t;
    decltype(t) t2;
  };

  E e1 = {1, 2};                  // OK, deduces E<int>

  template <typename... T>
  struct Types {};

  template <typename... T>
  struct F : Types<T...>, T... {};

  struct X {};
  struct Y {};
  struct Z {};
  struct W { operator Y(); };

  F f1 = {Types<X, Y, Z>{}, {}, {}};      // OK, F<X, Y, Z> deduced
  F f2 = {Types<X, Y, Z>{}, X{}, Y{}};    // OK, F<X, Y, Z> deduced
  F f3 = {Types<X, Y, Z>{}, X{}, W{}};    // error: conflicting types deduced; operator Y not considered
}

namespace explicit_guide_with_pack
{
  template <typename... T>
  struct Types { };

  template<typename U>
  struct NonAggr
  {
    NonAggr(...);
  };

  template<typename ... T>
  NonAggr(Types<T...>, T ...) -> NonAggr<void>;

  NonAggr v{Types<int, int>{}, 1}; // error: deduction fails
}

namespace implicit_guide_with_pack
{
  struct C { };
  struct D { };

  template <typename... T>
  struct Types { };

  template<typename ... T>
  struct NonAggr
  {
    NonAggr(Types<T...>, T...);
  };

  template<>
  struct NonAggr<C, D>
  { };

  NonAggr v{Types<C, D>{}, C{}}; // error: deduction fails
}

namespace empty_member
{
  struct E
  { };

  template<typename T>
  struct C
  {
    E a;
    T t;
  };

  C c{ 1 };                     // error: deduction fails
}

namespace non_trailing_pack_alias
{
  template<typename ... T>
  struct B
  { };

  template<typename U, typename ... T>
  struct A : B<T> ... {
    B<T ...> b;
    U u;
  };

  A a{ B<>{}, 1 };

  template<typename U>
  using AC = A<U, char>;

  AC ac{ B<char>{}, 1 };        // deduction succeeds, but initialization fails
  // transformed aggregate guide is A(B<char>, U) -> A<U, char>
}

namespace no_expr_init
{
  template<typename T>
  struct C
  {
    T t;
  };

  C c = 1;                      // deduction fails
}

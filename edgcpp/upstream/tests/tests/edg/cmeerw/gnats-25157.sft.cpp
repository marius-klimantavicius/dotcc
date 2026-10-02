//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1927:--c++20 --gn 120100

namespace minimal
{
  template<class T> struct A {
    template<typename U> requires requires { typename U::type; }
    friend void f(A, U) { }
  };
  int f(A<int>, long);
  int i = f(A<int>(), 1);
}

struct D1
{
  static constexpr bool value1 = true;
};

struct D2
{
  static constexpr bool value2 = true;
};

namespace outer_templ_param_in_requires_expr
{
  template<typename T>
  struct C
  {
    template<typename U> requires requires { T::value1; U::value2; }
    friend void f(C, U)
    { }
  };

  template<typename ... Args>
  constexpr bool is_f_callable_v = requires (Args ... args) { f(args ...); };

  static_assert(is_f_callable_v<C<D1>, D2>);

  static_assert(!is_f_callable_v<C<D2>, D2>);
  static_assert(!is_f_callable_v<C<D1>, D1>);
  static_assert(!is_f_callable_v<C<D2>, D1>);

  static_assert(!is_f_callable_v<C<int>, int>);
  static_assert(!is_f_callable_v<C<int>, D2>);
  static_assert(!is_f_callable_v<C<D1>, int>);
}

namespace ctad_template_decl_trailing_requires
{
  template<typename T>
  struct C
  {
    template<typename U>
    C(T, U) requires requires { T::value1; U::value2; };

    C(...);
  };

  template<typename ... Args>
  constexpr bool is_c_constructible_v = requires (Args ... args) { C(args ...); };

  static_assert(is_c_constructible_v<D1, D2>);

  static_assert(!is_c_constructible_v<D2, D2>);
  static_assert(!is_c_constructible_v<D1, D1>);
  static_assert(!is_c_constructible_v<D2, D1>);

  static_assert(!is_c_constructible_v<int, int>);
  static_assert(!is_c_constructible_v<int, D2>);
  static_assert(!is_c_constructible_v<D1, int>);


  template<typename T>
  using A = C<T>;

  template<typename ... Args>
  constexpr bool is_a_constructible_v = requires (Args ... args) { A(args ...); };

  static_assert(is_a_constructible_v<D1, D2>);

  static_assert(!is_a_constructible_v<D2, D2>);
  static_assert(!is_a_constructible_v<D1, D1>);
  static_assert(!is_a_constructible_v<D2, D1>);

  static_assert(!is_a_constructible_v<int, int>);
  static_assert(!is_a_constructible_v<int, D2>);
  static_assert(!is_a_constructible_v<D1, int>);
}

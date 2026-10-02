//type:fp
//options:--c++20:--ms_c++20

namespace concept_with_type_param
{
  template<typename> concept X = true;

  void f(int);

  template<typename ... T>
  int g(T ... t) requires (X<decltype(f(t ...))>);

  int i = g(1);
}

namespace concept_with_non_type_param
{
  template<bool> concept X = true;

  void f(int);

  template<typename ... T>
  int g(T ... t) requires (X<noexcept(f(t ...))>);

  int i = g(1);
}

namespace fn_params
{
  template<typename> concept X = true;

  void f0();
  void f1(int);
  void f2(int, int *);

  template<typename F, typename ... T>
  int g(F f, T ... t) requires (X<decltype(f(t ...))>);

  int i0 = g(f0);
  int i1 = g(f1, 1);
  int i2 = g(f2, 1, (int *) nullptr);
}

namespace negate_concept
{
  template<typename, typename>
  inline constexpr bool is_same_v = false;

  template<typename T>
  inline constexpr bool is_same_v<T, T> = true;

  template<typename T1, typename T2>
  concept same_as = is_same_v<T1, T2>;

  template<typename F, typename ... T>
  int g(F f, T ... t) requires (!same_as<decltype(f(t ...)), void>);

  int f0();
  int f1(int);
  int f2(int a, int *b);

  int i0 = g(&f0);
  int i1 = g(&f1, 1);
  int i2 = g(&f2, 1, (int *) nullptr);
}

namespace regression
{
  template<int>
  int f(int);

  int g(int);
  int g(int, int);
  int g(int, int, int);

  template<int... J, typename... T>
  auto h(T ... t) -> decltype(g(f<J>(t)...));

  int i1 = h<1>(2);
  int i2 = h<1, 2>(3, 4);
  int i3 = h<1, 2, 3>(4, 5, 6);
}

namespace regression_2
{
  template<int ...>
  struct C { };

  template<int> int f(int);

  int g(int);
  int g(int, int);
  int g(int, int, int);

  template<typename U, int... J, typename... T>
  auto h(C<J ...>, T ... t) -> decltype(g(f<J>(t)...));

  int i1   = h<int>(C<1>(), 2);
  int i1_1 = h<int, 1>(C<1>(), 2);

  int i2   = h<int>(C<1, 2>(), 2, 3);
  int i2_1 = h<int, 1>(C<1, 2>(), 2, 3);
  int i2_2 = h<int, 1, 2>(C<1, 2>(), 2, 3);

  int i3   = h<int>(C<1, 2, 3>(), 2, 3, 4);
  int i3_1 = h<int, 1>(C<1, 2, 3>(), 2, 3, 4);
  int i3_2 = h<int, 1, 2>(C<1, 2, 3>(), 2, 3, 4);
  int i3_3 = h<int, 1, 2, 3>(C<1, 2, 3>(), 2, 3, 4);
}

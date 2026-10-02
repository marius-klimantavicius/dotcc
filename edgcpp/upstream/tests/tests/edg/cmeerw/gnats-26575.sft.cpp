//type:fp
//options:--c++20:--c++20 --gn 120100

namespace minimal
{
  void f(int);
  template<typename ... T>
  concept C = requires(T ... t) { f(t ...); };
  template<typename ... T> requires C<T ...>
  int g(T ... t);
  int i = g(1);
}

namespace non_trailing_requires_clause
{
  void f(int);

  template<typename ... T>
  concept C = requires(T ... t) { f(t ...); };

  template<typename ... T> requires C<T ...>
  int g(T ... t);

  int i = g(1);
}

namespace trailing_requires_clause
{
  void f(int);

  template<typename ... T>
  concept C = requires(T ... t) { f(t ...); };

  template<typename ... T>
  int g(T ... t) requires C<T ...>;

  int i = g(1);
}

namespace pack_use_in_requires_expr
{
  template<typename ...>
  struct A
  { };

  void g(char, int);
  void h(int, int, int);

  template<typename ... T, typename ... U>
  int f(T ... t1, void (*p)(int, int, T ... t2), U ... t2) requires
    requires (T ... t) { p(1, 2, t1 ...); g(t2 ...); }
  ;

  int i = f<int>(1, &h, 'a', 1);
}

namespace pack_use_in_nested_function_type
{
  template<int>
  struct X
  { };

  template<>
  struct X<1>
  {
    static int val1;
  };

  template<>
  struct X<2>
  {
    static int val2;
  };

  X<1> g(int);

  template<typename ... T, typename ... U>
  int f(T ... t1, auto (*p)(T ... t2) -> X<sizeof...(t2)>, U ...) requires
    requires (U ... t2) { p(t1 ...).val1; X<sizeof...(t2)>::val2; }
  ;

  int i = f<int>(1, &g, 'a', 2);
}

//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace pack_expansion
{
  template<typename U> int v;
  template<typename U> struct C { };

  template<typename T> struct D {
    template<typename ... Us> requires requires { v<Us ...>; }
    struct M { };

    template<typename ... Us> requires requires { typename C<Us ...>; }
    struct N { };
  };

  D<char>::N<short> n;
  D<char>::M<short> m;
}

namespace minimal_regression
{
  template<int, typename W> struct C
  {
    template<typename T> requires requires(W w) { w; }
    friend int f(T)
    { return 0; }
  };

  int i = f(C<1, int>());
}

namespace friend_requirements
{
  struct B
  {
    using type = int;
  };

  template<int, typename W> struct C
  {
    using type = int;

    template<typename T> requires requires(W w) {
      typename W::type;
      typename T::type;
      w;
      T().UNDECLARED; }
    friend void f(T)
    { }

    template<typename T> requires requires(W w) {
      typename W::type;
      typename T::type;
      w;
      T(); }
    friend int f(T)
    { return 1; }
  };

  int i = f(C<1, B>());
}

namespace substitute_expr
{
  template<typename ...>
  class X;

  template<int, typename ... Ts>
  void g(const X<Ts...>&);

  template<typename T, typename... Rs>
  class X<T, Rs...> {
  public:
    template<int I>
    static constexpr bool v = requires(X x) { g<I>(x); };
  };

  static_assert(X<int>::v<0>);
}

namespace two_element_pack
{
  template<typename ...>
  class X;

  template<int I, typename T, typename U>
  void g(const X<T, U>&);

  template<typename T, typename... Rs>
  class X<T, Rs...> {
  public:
    template<int I>
    static constexpr bool v = requires(X x) { g<I>(x); };
  };

  static_assert(X<int, char>::v<0>);
}

namespace friend_order_of_substitution
{
  template<typename U> struct C {
    using type = int;
  };

  template<> struct C<int> { };

  template<typename T> struct D
  {
    template<typename U> requires requires { typename C<U>::type; }
    friend int f1(U);

    template<typename ... Us> requires requires { typename C<Us ...>::type; }
    friend int f2(Us ...);
  };

  auto i1 = f1(D<int>());
  auto i2 = f2(D<int>());
}

namespace alias_deduction_substitution
{
  template<typename U> struct C
  {
    using type = int;
  };

  template<> struct C<int> { };

  template<typename T> struct D {
    template<typename U>
    D(T, U) requires requires { typename C<U>::type; };
  };

  template<typename T> using A = D<T *>;

  D d1(1, 'a');
  D d2("", 'b');
  A a("", 'c');
}

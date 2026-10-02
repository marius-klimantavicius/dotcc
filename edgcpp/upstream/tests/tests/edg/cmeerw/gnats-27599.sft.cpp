//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T>
  struct C {
    template<typename U> requires T::value
    static int f(U);
    static int f(int);
  };
  int i = C<int>::f(0);
}

namespace more_decls
{
  template<typename T>
  struct C {
    template<typename U> requires T::value
    static int f(U);

    template<typename U> requires T::value
    static int s;

    template<typename U> requires T::value
    struct D
    { };
  };

  C<int> c;
}

namespace only_requires_clause
{
  template<bool B1>
  struct C
  {
    template<bool B2> requires B1 && B2
    static int f();

    template<bool B2> requires B1 && B2
    struct D
    { };
  };

  int i = C<true>::f<true>();
  C<true>::D<true> d;
}

namespace type_constraint_and_requires_clause
{
  template<typename>
  concept X = true;

  template<bool B1>
  struct C
  {
    template<X, bool B2> requires B1 && B2
    static int f();

    template<X, bool B2> requires B1 && B2
    struct D
    { };
  };

  int i = C<true>::f<int, true>();
  C<true>::D<int, true> d;
}

namespace more_constrained_friend
{
  template<bool B>
  concept C = B;

  template<bool>
  struct A
  { };

  template<bool B1>
  struct D
  {
    template<bool B3> requires C<B1>
    friend void f(D const &, A<B3>) requires C<B3>
    { }

    template<bool B3> requires C<B3>
    friend void f(D const &, A<B3>) requires C<B3> = delete;


    template<bool B3> requires C<B3>
    friend void g(D const &, A<B3>) requires C<B1>
    { }

    template<bool B3> requires C<B3>
    friend void g(D const &, A<B3>) requires C<B3> = delete;
  };

  void foo(D<true> d, A<true> a1)
  {
    f(d, a1);
    g(d, a1);
  }
}

namespace out_of_class_friend_matching
{
  template<bool B>
  concept C = B;

  template<bool>
  struct A
  { };

  template<bool B1>
  struct D;

  template<bool B3> requires C<B3>
  void f(D<true> const &, A<B3>) requires C<B3> = delete;

  template<bool B3> requires C<B3>
  void g(D<true> const &, A<B3>) requires C<B3> = delete;

  template<bool B1>
  struct D
  {
    template<bool B3> requires C<B3>
    friend void f(D const &, A<B3>) requires C<B3>;

    template<bool B3> requires C<B1>
    friend void f(D const &, A<B3>) requires C<B3>
    { }


    template<bool B3> requires C<B3>
    friend void g(D const &, A<B3>) requires C<B3>;

    template<bool B3> requires C<B3>
    friend void g(D const &, A<B3>) requires C<B1>
    { }
  };

  void foo(D<true> d, A<true> a1)
  {
    f(d, a1);
    g(d, a1);
  }
}

namespace delayed_requires_expr_expansion
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

    template<typename ... Us> requires requires { typename A<Us...>; }
    static int f();

    template<typename ... Us>
    static int g() requires requires { typename A<Us...>; };

    template<typename ... Us> requires requires { typename A<Us...>; }
    static inline int v = 0;
  };

  D<char>::N<short> n;
  int i = D<char>::f<short>() +
          D<char>::g<short>() +
          D<char>::v<short>;
}

namespace nested_friend
{
  template<int I, int J>
  struct Y;

  template<int I>
  struct Y<I, I>
  { };

  template<int I, typename W> struct C
  {
    template<int J, typename X> struct D
    {
      template<typename T> requires requires(W w, X x) {
        w; x; Y<I, 1>{}; Y<J, 2>{}; }
      friend int tf(T)
      { return 0; }

      // we probably don't check constraints on these yet
      friend int nf(D) requires requires(W w, X x) {
        w; x; Y<I, 1>{}; Y<J, 2>{}; }
      { return 0; }
    };
  };

  int i = tf(C<1, int>::D<2, char>());
  int j = nf(C<1, int>::D<2, char>());
}

namespace more_nested_friend
{
  template<typename, typename>
  struct X;

  template<typename T>
  struct X<T, T>
  {
    static constexpr bool value = true;
  };

  template<typename>
  struct B
  { };

  template<typename T1>
  struct C
  {
    template<typename T2>
    struct D {
      template<typename T3> requires X<T1, char[1]>::value && X<T2, char[2]>::value && X<T3, char[3]>::value
      friend int f(D, B<T3>) requires X<T1, char[1]>::value && X<T2, char[2]>::value && X<T3, char[3]>::value
      { return 0; };

      template<typename T3> requires requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      friend int g(D, B<T3>) requires requires { X<T1, char[1]>(), X<T2, char[2]>(), X<T3, char[3]>(); }
      { return 0; };
    };
  };

  int fv = f(C<char[1]>::D<char[2]>(), B<char[3]>());
  int gv = g(C<char[1]>::D<char[2]>(), B<char[3]>());
}

namespace dependent_var_substitution
{
  template<typename ... Ts>
  struct C;

  template<typename T1, typename... Ts>
  struct C<T1, Ts...>
  {
    template<typename U>
    static constexpr bool v = true;

    template<typename U> requires v<U>
    static int f(U);

    template<typename U>
    static int g(U) requires v<U>;
  };

  int i = C<char>::f(1) + C<char>::g(2);
}

namespace more_var_substitutions
{
  template<typename T1, typename T2>
  struct B
  {
    template<typename U1, typename U2>
    static constexpr bool v = true;
  };

  template<typename T>
  struct C
  {
    template<typename U>
    struct N
    {
      static int f() requires B<T, U>::template v<T, U>;
    };

    template<typename U> requires B<T, U>::template v<T, U>
    static int f(U);

    template<typename U>
    static int g(U) requires B<T, U>::template v<T, U>;
  };

  int i = C<char>::N<int>::f() + C<char>::f(1) + C<char>::g(2);
}

namespace subst_fn_call
{
  template<typename T>
  struct C
  {
    static constexpr bool f() { return true; }

    template<typename U> requires (C<T>::f())
    static int g1(U);

    template<typename U> requires (C<U>::f())
    static int g2(U);

    template<typename U>
    static int g3(U) requires (C<T>::f());

    template<typename U>
    static int g4(U) requires (C<U>::f());

    template<typename U>
    struct N
    {
      static int g5(U) requires (C<T>::f());

      static int g6(U) requires (C<U>::f());
    };
  };

  int i = C<char>::g1(1) + C<char>::g2(2) + C<char>::g3(1) + C<char>::g4(2) +
          C<char>::N<int>::g5(5) + C<char>::N<int>::g6(6);
}

namespace subst_fn_with_args
{
  template<typename ... Ts>
  struct C;

  template<typename T1, typename... Ts>
  struct C<T1, Ts...>
  {
    template<typename U>
    static constexpr bool f() { return true; }

    template<typename U> requires (f<U>())
    static int g1(U);

    template<typename U>
    static int g2(U) requires (f<U>());

    template<typename U> requires (f<T1>())
    static int g3(U);

    template<typename U>
    static int g4(U) requires (f<T1>());

    template<typename U>
    struct N
    {
      static int g5(U) requires (f<U>());

      static int g6(U) requires (f<T1>());
    };
  };

  int i = C<char>::g1(1) + C<char>::g2(2) + C<char>::g3(3) + C<char>::g4(4) +
          C<char>::N<int>::g5(5) + C<char>::N<int>::g6(6);
}

namespace sizeof_pack
{
  template<typename ... Ts>
  struct C
  { };

  template<typename T1, typename ... Ts>
  struct C<T1, Ts ...>
  {
    template<bool B, class U, unsigned int I = sizeof ... (Ts)>
    static constexpr bool v = false;

    template<typename U, unsigned int I>
    static constexpr bool v<false, U, I> = I == 2;

    template<typename U> requires v<false, U>
    void g1(U);

    template<typename U>
    void g2(U) requires v<false, U>;

    void g3(int) requires v<false, T1>;
  };

  void f(C<char, short, int> c)
  {
    c.g1(1);
    c.g2(2);
    c.g3(3);
  }
}

namespace splitter
{
  template<typename...>
  struct TypeList;

  template<typename ...N>
  struct Splitter
  {
    template<template<N> class ...L>
    struct Split
    {
      using Left = TypeList<L<0> ...>;
    };
  };
}

namespace subst_pack_into_default_arg
{
  template<typename>
  struct G
  {
    static const bool value = false;
  };

  template<typename T, bool = G<T>::value>
  struct H
  { };

  template<typename ...>
  struct X { };

  template<typename ... TT>
  X<H<TT> ...> f();
}

namespace use_template_param_in_default_arg_lambda_body
{
  template<class T, auto t = [] { return T(); }>
  inline constexpr auto b_v = t;

  static_assert(b_v<int>() == 0);
}

namespace in_default_type_arg_nested_arg
{
  template<typename, int>
  struct B
  {
    static constexpr bool value = true;
  };

  template<int ...>
  struct Y
  { };

  template<int>
  struct YY
  {
    using type = Y<>;
  };

  template<>
  struct YY<2>
  {
    using type = Y<1, 2>;
  };

  template<typename T1, typename ... Ts>
  struct C
  {
    template<bool B, typename U, class V = typename YY<sizeof ... (Ts)>::type>
    static constexpr bool v = false;

    template<typename U, int ... Is>
    static constexpr bool v<true, U, Y<Is ...>> = (B<Ts, Is>::value && ...);

    template<typename U> requires v<true, U>
    static int f1(U);

    template<typename U> requires v<true, U>
    static int f2(U);

    template<typename U>
    struct N
    {
      static int f() requires v<true, U>;
    };
  };

  int i1 = C<char, short, int>::f1(1);
  int i2 = C<char, short, int>::f2(1);
  int i = C<char, short, int>::N<int>::f();
}

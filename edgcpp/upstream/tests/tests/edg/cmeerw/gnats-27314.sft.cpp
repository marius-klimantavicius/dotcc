//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename U>
  struct A {
    template<typename T>
    static int f(T t) requires (sizeof(U) > 1);
    static int f(long);
  };
  int i = A<void>::f(0);
}

namespace incomplete_type_in_requires_clause
{
  template<typename U>
  struct A
  {
    template<typename T>
    static constexpr bool tmpl(T t) requires (sizeof(U) > 1)
    { return true; }

    template<typename T>
    static constexpr bool tmpl(T t) requires (sizeof(U) <= 1)
    { return false; }

    static constexpr bool nontmpl() requires (sizeof(U) > 1)
    { return true; }

    static constexpr bool nontmpl() requires (sizeof(U) <= 1)
    { return false; }
  };

  struct B;

  A<B> ab;
  A<char> ac;

  struct B
  { char arr[2]; };

  static_assert(A<B>::tmpl(1));
  static_assert(A<B>::nontmpl());

  static_assert(!A<char>::tmpl(1));
  static_assert(!A<char>::nontmpl());
}

namespace decl_matching
{
  template<typename T>
  concept C = true;

  template<typename T> struct A
  {
    template<typename U>
    U tmpl(U) requires C<T> && C<U>;

    int nontmpl(int) requires C<T>;
  };

#if !defined(__clang__) && !defined(_MSC_VER)
  template<>
  template<typename U>
  U A<int>::tmpl(U u) requires C<int> && C<U>
  { return u; }

  template<>
  int A<int>::nontmpl(int i)
  { return i; }
#endif
}

namespace decl_matching_2
{
  template<typename T>
  concept C = true;

  template<typename T>
  struct A
  {
    template<typename U>
    struct B
    {
      template<typename V>
      static void tmpl(V) requires false;

      template<typename V>
      static constexpr V tmpl(V) requires C<T> && C<U> && C<V>;
    };
  };

  template<typename T>
  template<typename U>
  template<typename V>
  constexpr V A<T>::B<U>::tmpl(V v) requires C<T> && C<U> && C<V>
  { return v + 1; }

#if !defined(__clang__) && !defined(_MSC_VER)
  template<>
  template<>
  template<typename V>
  constexpr V A<int>::B<char>::tmpl(V v) requires C<int> && C<char> && C<V>
  { return v + 2; }

  static_assert(A<int>::B<char>::tmpl(1) == 3);
#endif
}

namespace decl_matching_3
{
  template<typename>
  struct B
  {
    template<typename>
    void f() requires true;
  };

  template<>
  template<typename T>
  void B<int>::f() requires true
  { }
}

namespace pack_expansion
{
  template<typename ... Ts>
  struct C
  {
    template<typename ... Us>
      requires (sizeof ... (Us) >= 1) && (sizeof ... (Ts) >= 1)
    static int f(Us ... us);

    template<typename ... Us>
    static int g(Us ... us)
      requires (sizeof ... (Us) >= 1) && (sizeof ... (Ts) >= 1);
  };

  int i = C<int, char>::f(1, 2) + C<int, short>::g(3, 4);
}

namespace mixed_pack_expansion
{
  template<typename T1, typename T2>
  concept X = sizeof(T1) == sizeof(T2);

  template<typename ... T>
  struct C
  {
    template<typename ... U>
    static int f(U ... u) requires (X<T, U> || ...);
  };

  int i = C<int>::f(1) + C<int, char>::f(1, 'a');
}

namespace mixed_pack_expansion_constant
{
  template<typename T, typename U>
  struct X
  {
    static constexpr bool value = true;
  };

  template<typename T>
  struct C1
  {
    template<typename ... U>
    static int f() requires (((X<T, U>::value) || ...));
  };

  template<typename ... T>
  struct C2
  {
    template<typename U>
    static int f() requires (((X<T, U>::value) || ...));
  };

  template<typename ... T>
  struct C3
  {
    template<typename ... U>
    static int f() requires (((X<T, U>::value) || ...));
  };

  int i = C1<int>::f<int>() + C2<int>::f<int>() + C3<int>::f<int>();
}

namespace requires_expr_in_constraint
{
  template<typename T1, typename T2>
  void g();

  template<typename T>
  struct C
  {
    template<typename U>
    static int f1(U u) requires requires { g<T, U>(); };

    template<typename U>
    static int f2(U u) requires (!requires { g<T, U>(1); });
  };

  int i = C<int>::f1(1) + C<int>::f2(1);
}

namespace shortcut_substitution_into_constraints_tmpl
{
  template<typename T>
  struct X
  {
#if !defined(_MSC_VER)
    static_assert(sizeof(T) == 1);
#endif
  };

  template<typename T>
  struct C
  {
    template<typename U>
    static void f(U u) requires (sizeof(U) == 1) && X<T>::value;
    static int f(long);

    template<typename U>
    static void g(U u) requires (sizeof(T) == 1) && X<U>::value;
    static int g(long);
  };

  int i = C<int>::f(1) + C<int>::g(2);
}

namespace friend_substitution
{
  template<int I, typename ... TT>
  concept X = sizeof...(TT) == I;

  template<typename ... TT>
  struct C
  {
    template<int I>
    struct D
    {
      friend void operator +(D, int) requires X<I, TT ...>
      { }
    };
  };

  void f()
  {
    C<int>::D<1>() + 1;
    C<int, char>::D<2>() + 1;
  }
}

namespace friend_substitution_fold
{
  template<int I, int J>
  concept X = I == J;

  template<typename ... TT>
  struct C1
  {
    template<int I>
    struct D
    {
      friend void operator +(D, int) requires X<I, (TT{1} + ...)>
      { }

      friend void g(D, long) requires false
      { }
    };
  };

  void f(C1<int, int>::D<2> d12)
  {
    d12 + 1;
  }
}

namespace possible_regression
{
  template<int I, typename... Ts> concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C1 {
    template<int J> struct D;
  };

  template<int J>
  struct C2 {
    template<typename ... Vs> struct D;
  };

  template<typename ... Us> template<int I>
  struct C1<Us ...>::D {
    int f() requires X<I, Us ...>;
  };

  template<int I> template<typename ... Us>
  struct C2<I>::D {
    int f() requires X<I, Us ...>;
  };

  int i = C1<int, char>::D<2>().f();
  int j = C2<2>::D<int, char>().f();
}

namespace inconsistent_template_args
{
  template<int>
  struct B
  { };

  template<typename T1>
  struct C
  {
    template<typename T2>
    struct D
    {
      template<int I> requires requires { B<I>(); }
      friend int g(D, C<T2>, B<I>)
      { return 0; };
    };
  };

  int gv = g(C<char[1]>::D<char[2]>(), C<char[2]>(), B<1>());
}

namespace identical_requirements
{
  template<typename T>
  struct C
  {
    static int f1() requires (sizeof(T) > 0);
    static int *f1();

    static int f2() requires (sizeof(T) > 0);

    template<typename = void>
    static int t1() requires (sizeof(T) > 0);
    template<typename = void>
    static int *t1();

    template<typename = void>
    static int t2() requires (sizeof(T) > 0);
  };

  struct A;

  int *pf = C<A>::f1();
  int *pg = C<A>::t1();

  struct A { };

  int jf = C<A>::f2();
  int jg = C<A>::t2();
}

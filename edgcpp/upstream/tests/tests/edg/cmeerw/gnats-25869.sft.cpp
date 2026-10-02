//type:fp
//options:--c++17:--c++17 --gnu_version 100100:--c++17 --gnu_version 110100:--c++20

namespace minimal
{
  template<typename ... T>
  struct B {
    static_assert(sizeof ... (T) != 0, "Unexpected");
    static constexpr int v = 0;
  };
  template<typename ... T>
  struct C {
    template<int = B<T ...>::v, typename = C>
    C(T ...);
  };
  C c{ 1, 2 };
}

namespace substitution_failure_for_gcc10
{
#if defined(__GNUC__) && __GNUC__ < 11
  constexpr bool use_explicit_guide = true;
#else
  constexpr bool use_explicit_guide = false;
#endif

  template<typename ... T>
  struct D {
    template<int = 0>
    static constexpr int f() { return sizeof ... (T); }
    template<int = D::f()> constexpr D(int) { }
  };
  D(int *) -> D<int *>;
  static_assert(D(0).f() == use_explicit_guide);
}

namespace other_class_qualified_name
{
  template<typename ... T>
  struct B
  {
    template<typename ... U>
    static constexpr int f()
    { return 0; }
  };

  template<typename ... T>
  struct C
  {
    template<typename U, int = B<T ...>::template f<T ...>()>
    C(U);
  };

  C c(1);
}

namespace in_function_arg
{
  template<typename ... U>
  static constexpr int f(U ...)
  { return 0; }

  template<typename ... T>
  struct C
  {
    static constexpr int v = 1;

    template<typename U, int = f<T ...>(C<T ...>::v)>
    C(U);
  };

  C c(1);
}

namespace default_arg_with_constexpr_func
{
  template<typename... TT>
  constexpr auto f()
  {
    static_assert(sizeof...(TT) == 2);
    return 1;
  }

  template<typename... TT>
  struct C
  {
    template<int I = f<TT ...>()>
    C(TT... );
  };

  C c{ 1, 2L };

#if __cplusplus >= 202002L
  template<typename ... T>
  using CA = C<T ...>;

  CA ca{ 1, 2 };
#endif
}

namespace default_arg_with_constexpr_var
{
  template<typename ... TT>
  struct B
  {
    static_assert(sizeof...(TT) == 2);
    static constexpr int value = 1;
  };

  template<typename... TT>
  constexpr int v = B<TT ...>::value;

  template<typename ...TT>
  struct C
  {
    template<int I = v<TT...>>
    C(TT ...);
  };

  C c{ 1, 2L };

#if __cplusplus >= 202002L
  template<typename ... T>
  using CA = C<T ...>;

  CA ca{ 1, 2 };
#endif
}

namespace static_data_member_in_default_arg
{
  template<typename... T>
  struct C
  {
    template<typename ... U>
    static constexpr bool g = false;

    template<typename ... U, int V1 = g<U ...>> C(U ...);
  };

  C c{1, 2};
}

namespace empty_pack
{
  template<typename... T>
  struct C
  {
    template<typename ... U>
    static constexpr bool g() { return false; }

    template<typename ... U, int V1 = g<U ...>()>
    static int f(U ...);
  };

  int i = C<>::f(1, 2);
}

namespace default_arg_with_static_member_function
{
  template<int I, typename... TT>
  struct B {
    static constexpr auto f() {
      return 1;
    }
  };

  template<typename ...TT>
  struct C {
    template<int I> using A = B<I, TT...>;

    template<int = A<1>::f()>
    C(TT ...);
  };

  C c{ 1, 2 };

#if __cplusplus >= 202002L
  template<typename ... T>
  using CA = C<T ...>;

  CA ca{ 1, 2 };
#endif
}

namespace pack_expansion_in_template_parameter_type
{
  template<bool, typename> struct D;
  template<typename T> struct D<true, T>{ using type = T; };

  template<typename... T>
  struct G {
    static constexpr bool f() {
      return true;
    }
  };
  template<typename... T>
  struct C {
    template<typename D<G<T...>::f(), bool>::type = true>
    C(T... );
  };

  C x{ 1, 2 };

#if __cplusplus >= 202002L
  template<typename ... T>
  using CA = C<T ...>;

  CA ca{ 1, 2 };
#endif
}

namespace pack_expansion_in_alias_for_parameter_type
{
  template<bool, typename > struct D;
  template<typename T> struct D<true, T>{ using type = T; };

  template<bool B> using E = typename D<B, bool>::type;

  template<typename... T>
  struct G {
    static constexpr bool f() {
      return true;
    }
  };

  template<typename... T>
  struct C1
  {
    template<E<G<T...>::f()> = true>
    C1(T... );
  };

  C1 c1{ 1, 2 };

  template<typename... T>
  struct C2
  {
    template<bool B> using R = E<G<T...>::f()>;

    template<R<false> = true>
    C2(T... );
  };

  C2 c2{ 1, 2 };

#if __cplusplus >= 202002L
  template<typename ... T>
  using C1A = C1<T ...>;

  template<typename ... T>
  using C2A = C2<T ...>;

  C1A c1a{ 1, 2 };
  C2A c2a{ 1, 2 };
#endif
}

namespace no_spurious_empty_pack_instantiation
{
  template<bool> struct D;
  template<> struct D<true>{ using type = bool; };

  template<typename... T>
  struct G {
    static constexpr bool f() {
      static_assert(sizeof ... (T) != 0);
      return sizeof ... (T) != 0;
    }
  };

  template<typename... T>
  static constexpr bool f() {
    static_assert(sizeof ... (T) != 0);
    return sizeof ... (T) != 0;
  }

  template<typename... T>
  struct C1 {
    template<typename D<G<T...>::f()>::type = true>
    C1(T... );
  };

  template<typename... T>
  struct C2 {
    template<typename D<f<T...>()>::type = true>
    C2(T... );
  };

  C1 c1{ 1, 2 };
  C2 c2{ 1, 2 };

#if __cplusplus >= 202002L
  template<typename ... T>
  using C1A = C1<T ...>;

  template<typename ... T>
  using C2A = C2<T ...>;

  C1A c1a{ 1, 2 };
  C2A c2a{ 1, 2 };
#endif
}

namespace copy_deduction_candidate
{
  template<typename ... T>
  struct C;

  template<typename ... T>
  struct C
  { };

  template<int ... I>
  struct D;

  template<int ... I>
  struct D
  { };

  void f()
  {
    C<> c0;
    C cc0(c0);
    c0 = cc0;

    C<int> c1;
    C cc1(c1);
    c1 = cc1;

    C<int, long> c2;
    C cc2(c2);
    c2 = cc2;

    D<> d0;
    D dd0(d0);
    d0 = dd0;

    D<1> d1;
    D dd1(d1);
    d1 = dd1;

    D<1, 2> d2;
    D dd2(d2);
    d2 = dd2;
  }

#if __cplusplus >= 202002L
  template<typename ... T>
  using CA = C<T ...>;

  template<int ... I>
  using DA = D<I ...>;

  void g()
  {
    CA<> c0;
    CA cc0(c0);
    c0 = cc0;

    CA<int> c1;
    CA cc1(c1);
    c1 = cc1;

    CA<int, long> c2;
    CA cc2(c2);
    c2 = cc2;

    DA<> d0;
    DA dd0(d0);
    d0 = dd0;

    DA<1> d1;
    DA dd1(d1);
    d1 = dd1;

    DA<1, 2> d2;
    DA dd2(d2);
    d2 = dd2;
  }
#endif
}

namespace out_of_class_defn
{
  template<typename T, typename U>
  struct X
  { };

  template<typename ... T>
  struct C;

  template<typename ... T>
  struct C
  {
    template<typename ... U>
    C(X<T, U> ...);
  };

  template<typename ... T>
  template<typename ... U>
  C<T ...>::C(X<T, U> ...)
  { }

  C c{ X<int, long>() };

#if __cplusplus >= 202002L
  template<typename ... T>
  using A = C<T ...>;

  A a{ X<long, int>() };
#endif
}

#if __cplusplus >= 202002L
namespace code_coverage
{
  template<typename T>
  struct O
  {
    template<bool B>
    struct I
    {
      static constexpr bool v = true;

      template<bool>
      static constexpr bool w = true;

      friend constexpr bool operator==(const I &, const I &) requires v
      {
        return true;
      }

      friend constexpr bool operator!=(const I &, const I &) requires w<B>
      {
        return true;
      }
    };
  };

  bool f(O<int>::I<true> i)
  {
    return i == i && i != i;
  }
}
#endif

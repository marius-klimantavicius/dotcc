//type:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<typename ...> struct A { };
  template<typename ... T> struct C {
    static int f(A<T ...>, auto);
  };
  int i = C<int>::f(A<int>(), 1);
}

namespace abbr_template
{
  namespace fn_param_pack_with_auto
  {
    template<typename ... T>
    struct C
    {
      static int f1(T ..., auto ... p);
      static int f2(T ..., auto p);
    };

    auto v1 = C<int>::f1(1, 2, 3);
    auto v2 = C<int>::f2(1, 2);
  }

  namespace pack_param_with_auto
  {
    template<typename ...> struct A
    { };

    template<typename ... T> struct C
    {
      static int f1(A<T ...> a, auto p);
      static int f2(A<T ...> a, auto ... p);
    };

    auto v1 = C<int>::f1(A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), 1, 2);
  }

  namespace multiple_pack_params_with_auto
  {
    template<typename...> struct A
    { };

    template<typename... T> struct C
    {
      static int f1(A<T ...> a1, A<T ...> a2, auto x);
      static int f2(A<T ...> a1, A<T ...> a2, auto ... x);
    };

    auto v1 = C<int>::f1(A<int>(), A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), A<int>(), 1, 2);
  }

  namespace multiple_non_type_pack_params_with_auto
  {
    template<int ...> struct A
    { };

    template<int ... I> struct C
    {
      static int f1(A<I ...> a1, A<I ...> a2, auto x);
      static int f2(A<I ...> a1, A<I ...> a2, auto ... x);
    };

    auto v11 = C<1>::f1(A<1>(), A<1>(), 1);
    auto v12 = C<2, 3>::f1(A<2, 3>(), A<2, 3>(), 1);
    auto v21 = C<1>::f2(A<1>(), A<1>(), 1, 2);
    auto v22 = C<2, 3>::f2(A<2, 3>(), A<2, 3>(), 1, 2);
  }

  namespace nested_pack_expansions
  {
    template<int...> struct A
    { };

    template<int ... J> struct B
    {
      template<int ... I> struct C
      {
        static int f(A<I ...> a1, auto x, A<J ...> a2, auto ... y);
      };
    };

    auto v1 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4);
    auto v2 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4, 5);
  }
}

namespace template_decl
{
  namespace fn_param_pack_with_auto
  {
    template<typename ... T>
    struct C
    {
      template<typename = void>
      static int f1(T ..., auto ... p);
      template<typename = void>
      static int f2(T ..., auto p);
    };

    auto v1 = C<int>::f1(1, 2, 3);
    auto v2 = C<int>::f2(1, 2);
  }

  namespace pack_param_with_auto
  {
    template<typename ...> struct A
    { };

    template<typename ... T> struct C
    {
      template<typename = void>
      static int f1(A<T ...> a, auto p);
      template<typename = void>
      static int f2(A<T ...> a, auto ... p);
    };

    auto v1 = C<int>::f1(A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), 1, 2);
  }

  namespace multiple_pack_params_with_auto
  {
    template<typename...> struct A
    { };

    template<typename... T> struct C
    {
      template<typename = void>
      static int f1(A<T ...> a1, A<T ...> a2, auto x);
      template<typename = void>
      static int f2(A<T ...> a1, A<T ...> a2, auto ... x);
    };

    auto v1 = C<int>::f1(A<int>(), A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), A<int>(), 1, 2);
  }

  namespace multiple_non_type_pack_params_with_auto
  {
    template<int ...> struct A
    { };

    template<int ... I> struct C
    {
      template<typename = void>
      static int f1(A<I ...> a1, A<I ...> a2, auto x);
      template<typename = void>
      static int f2(A<I ...> a1, A<I ...> a2, auto ... x);
    };

    auto v11 = C<1>::f1(A<1>(), A<1>(), 1);
    auto v12 = C<2, 3>::f1(A<2, 3>(), A<2, 3>(), 1);
    auto v21 = C<1>::f2(A<1>(), A<1>(), 1, 2);
    auto v22 = C<2, 3>::f2(A<2, 3>(), A<2, 3>(), 1, 2);
  }

  namespace nested_pack_expansio4ns
  {
    template<int...> struct A
    { };

    template<int ... J> struct B
    {
      template<int ... I> struct C
      {
        template<typename = void>
        static int f(A<I ...> a1, auto x, A<J ...> a2, auto ... y);
      };
    };

    auto v1 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4);
    auto v2 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4, 5);
  }
}

namespace generic_lambda
{
  namespace fn_param_pack_with_auto
  {
    template<typename ... T>
    struct C
    {
      static inline auto f1 = [](T ..., auto ... p) { return 0; };
      static inline auto f2 = [](T ..., auto p) { return 0; };
    };

    auto v1 = C<int>::f1(1, 2, 3);
    auto v2 = C<int>::f2(1, 2);
  }

  namespace pack_param_with_auto
  {
    template<typename ...> struct A
    { };

    template<typename ... T> struct C
    {
      static inline auto f1 = [](A<T ...> a, auto p) { return 0; };
      static inline auto f2 = [](A<T ...> a, auto ... p) { return 0; };
    };

    auto v1 = C<int>::f1(A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), 1, 2);
  }

  namespace multiple_pack_params_with_auto
  {
    template<typename...> struct A
    { };

    template<typename... T> struct C
    {
      static inline auto f1 = [](A<T ...> a1, A<T ...> a2, auto x) { return 0; };
      static inline auto f2 = [](A<T ...> a1, A<T ...> a2, auto ... x) { return 0; };
    };

    auto v1 = C<int>::f1(A<int>(), A<int>(), 1);
    auto v2 = C<int>::f2(A<int>(), A<int>(), 1, 2);
  }

  namespace multiple_non_type_pack_params_with_auto
  {
    template<int ...> struct A
    { };

    template<int ... I> struct C
    {
      static inline auto f1 = [](A<I ...> a1, A<I ...> a2, auto x) { return 0; };
      static inline auto f2 = [](A<I ...> a1, A<I ...> a2, auto ... x) { return 0; };
    };

    auto v11 = C<1>::f1(A<1>(), A<1>(), 1);
    auto v12 = C<2, 3>::f1(A<2, 3>(), A<2, 3>(), 1);
    auto v21 = C<1>::f2(A<1>(), A<1>(), 1, 2);
    auto v22 = C<2, 3>::f2(A<2, 3>(), A<2, 3>(), 1, 2);
  }

  namespace nested_pack_expansions
  {
    template<int...> struct A
    { };

    template<int ... J> struct B
    {
      template<int ... I> struct C
      {
        static inline auto f = [](A<I ...> a1, auto x, A<J ...> a2, auto ... y) { return 0; };
      };
    };

    auto v1 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4);
    auto v2 = B<1>::C<2>::f(A<2>(), 2, A<1>(), 4, 5);
  }
}

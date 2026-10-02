//type:fp
//options:--c++26:--c++26 --gn 160100:--c++26 --clang_version 220100
//options_all:-w -tused

namespace minimal {
  template<int I, typename ... Ts>
  constexpr auto f(Ts ... vs) -> Ts ... [I] {
    return vs ... [I];
  }
  static_assert(f<0>(1, 2L) == 1 && f<1>(1, 2L) == 2L);
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;


namespace constant_parameter
{
  template<int N, int ... Is>
  constexpr int f()
  {
    return Is...[N];
  }

  static_assert(f<0, 1, 2, 3>() == 1);
  static_assert(f<1, 1, 2, 3>() == 2);
  static_assert(f<2, 1, 2, 3>() == 3);
}

namespace function_param_pack
{
  template<int N>
  constexpr int f(auto ... vs)
  {
    return vs...[N];
  }

  static_assert(f<0>(1, 2, 3) == 1);
  static_assert(f<1>(1, 2, 3) == 2);
  static_assert(f<2>(1, 2, 3) == 3);
}

namespace captured_pack
{
  template<int N>
  constexpr int f(auto ... vs)
  {
    return [=] () { return vs...[N]; }();
  }

  static_assert(f<0>(1, 2, 3) == 1);
  static_assert(f<1>(1, 2, 3) == 2);
  static_assert(f<2>(1, 2, 3) == 3);
}

namespace init_captured_pack
{
  template<int N>
  constexpr int f(auto ... vs)
  {
    return [... cs = vs] () { return cs...[N]; }();
  }

  static_assert(f<0>(1, 2, 3) == 1);
  static_assert(f<1>(1, 2, 3) == 2);
  static_assert(f<2>(1, 2, 3) == 3);
}

namespace binding_pack
{
  struct C
  {
    int i1, i2, i3;
  };

  template<int N>
  constexpr int f(auto t)
  {
    auto [ ... b ] = t;
    return b...[N];;
  }

  static_assert(f<0>(C{1, 2, 3}) == 1);
  static_assert(f<1>(C{1, 2, 3}) == 2);
  static_assert(f<2>(C{1, 2, 3}) == 3);
}

namespace nested_expansion
{
  template<int ... Ns>
  constexpr int f(auto ... vs)
  {
    return (vs...[Ns] + ...);
  }

  static_assert(f<0>(1, 2, 3) == 1);
  static_assert(f<1>(1, 2, 3) == 2);
  static_assert(f<2>(1, 2, 3) == 3);

  static_assert(f<0, 1>(1, 2, 3) == 3);
  static_assert(f<1, 2>(1, 2, 3) == 5);
  static_assert(f<2, 2>(1, 2, 3) == 6);
}

namespace parenthesized_nested_expansion
{
  template<int ... Ns>
  constexpr int f(auto ... vs)
  {
    return ((vs...[Ns]) + ...);
  }

  static_assert(f<0>(1, 2, 3) == 1);
  static_assert(f<1>(1, 2, 3) == 2);
  static_assert(f<2>(1, 2, 3) == 3);

  static_assert(f<0, 1>(1, 2, 3) == 3);
  static_assert(f<1, 2>(1, 2, 3) == 5);
  static_assert(f<2, 2>(1, 2, 3) == 6);
}

namespace substitution
{
  template<int I, typename ... Ts>
  constexpr auto g(Ts ... vs) -> decltype(vs...[I])
  {
    return vs...[I];
  }

  static_assert(is_same_v<decltype(g<0>(1, 'b', 3L)), int>);
  static_assert(is_same_v<decltype(g<1>(1, 'b', 3L)), char>);
  static_assert(is_same_v<decltype(g<2>(1, 'b', 3L)), long>);
}

namespace type_specifier_cast
{
  template<int I, typename ... Ts>
  constexpr auto g(Ts ... vs)
  {
    return Ts ... [I]{};
  }

  static_assert(is_same_v<decltype(g<0>(1, 'b', 3L)), int>);
  static_assert(is_same_v<decltype(g<1>(1, 'b', 3L)), char>);
  static_assert(is_same_v<decltype(g<2>(1, 'b', 3L)), long>);
}

namespace type_specifier_decl
{
  template<int I, typename ... Ts>
  constexpr auto g(Ts ... vs)
  {
    Ts ... [I] v{};
    return v;
  }

  static_assert(is_same_v<decltype(g<0>(1, 'b', 3L)), int>);
  static_assert(is_same_v<decltype(g<1>(1, 'b', 3L)), char>);
  static_assert(is_same_v<decltype(g<2>(1, 'b', 3L)), long>);
}

namespace type_specifier_substitution
{
  template<int I, typename ... Ts>
  constexpr auto g(Ts ... vs) -> Ts ... [I]
  {
    return { };
  }

  static_assert(is_same_v<decltype(g<0>(1, 'b', 3L)), int>);
  static_assert(is_same_v<decltype(g<1>(1, 'b', 3L)), char>);
  static_assert(is_same_v<decltype(g<2>(1, 'b', 3L)), long>);
}

namespace base_class
{
  template<int I, typename ... Ts>
  struct C : Ts ... [I]
  { };

  struct B1 { };
  struct B2 { };

  void f()
  {
    C<0, B1, B2> c0;
    B1 &b1 = c0;

    C<1, B1, B2> c1;
    B2 &b2 = c1;
  }
}

namespace init_list
{
  template<int I, typename ... Ts>
  constexpr auto f(Ts ... ts)
  {
    int i(ts ... [I]);
    int j{ts ... [I]};
    return i + j;
  }

  static_assert(f<0>(0) == 0);
  static_assert(f<0>(0, 1) == 0);
  static_assert(f<1>(0, 1) == 2);
}

namespace nested_name_specifier
{
  template<int I, typename ... Ts>
  constexpr int f(Ts ... ts)
  {
    typename Ts ... [I]::type t(I);
    return Ts ... [I]::value + t;
  };

  template<int I>
  struct C
  {
    using type = int;
    static constexpr int value = I;
  };

  static_assert(f<0>(C<1>(), C<2>(), C<3>()) == 1);
  static_assert(f<1>(C<1>(), C<2>(), C<3>()) == 3);
  static_assert(f<2>(C<1>(), C<2>(), C<3>()) == 5);
}

namespace outer_pack_for_lambda
{
  template<int I, auto... Vs>
  constexpr int v = []<int N>() { return Vs...[N]; }.template operator()<I>();

  static_assert(v<0, 1, 2, 3> == 1);
  static_assert(v<1, 1, 2, 3> == 2);
  static_assert(v<2, 1, 2, 3> == 3);
}

namespace outer_pack_inner_index
{
  template<typename ... T> struct C
  {
    template<unsigned I>
    static T...[I] f();
  };

  char *c = C<char *, short *, int *>::f<0>();
  short *s = C<char *, short *, int *>::f<1>();
  int *i = C<char *, short *, int *>::f<2>();
}

namespace destructor_name
{
  struct C
  { };

  template<typename ... T>
  void f()
  {
    T ... [0] *p = 0;

    p->~T ... [0]();
    (*p).~T ... [0]();
  }

  template void f<C>();
  template void f<int>();
}

namespace disambiguation
{
  template<int I>
  void f();

  template<int I, typename ... Ts>
  void g()
  {
    f<Ts ... [I]{}>();
    f<Ts ... [I](0)>();
  }

  template void g<0, int>();
}

namespace parameter_types
{
  template<int I, typename ... Ts>
  auto f(Ts ... [I] t)
  {
    return t;
  }

  static_assert(is_same_v<decltype(f<0, int, char>(1)), int>);
  static_assert(is_same_v<decltype(f<1, int, char>(1)), char>);

  template<typename ...T> struct A
  {
    template<unsigned I> static auto f(T...[I] t)
    {
      return t;
    }

    template<unsigned I> static T...[I] g();
  };

  static_assert(is_same_v<decltype(A<char, int>::f<0>(1)), char>);
  static_assert(is_same_v<decltype(A<char, int>::f<1>(1)), int>);
  static_assert(is_same_v<decltype(A<char, int>::g<0>()), char>);
  static_assert(is_same_v<decltype(A<char, int>::g<1>()), int>);
}

namespace alias_instantiation
{
  template<int... Is>
  struct X { };

  template<typename... Ts>
  struct Y { };

  template<typename, typename>
  struct C;

  template<typename... Ts, int... Is>
  struct C<Y<Ts...>, X<Is...>>
  {
    using type = Y<Ts...[Is]...>;
  };

  typename C<Y<char, short>, X<1, 1>>::type v = Y<short, short>();
}

namespace lambda_template_parameter_list
{
  template<typename ... Ts>
  int f()
  {
    auto l = []<Ts...[0]>() { return 0; };
    return l.template operator()<0>();
  }

  template<int I, typename ... Ts>
  int g()
  {
    auto l = []<Ts...[I]>() { return 0; };
    return l.template operator()<0>();
  }

  int i = f<int, int>();
  int j = g<1, int, int>();
}

namespace base_class_expansion
{
  template<typename ... Ts>
  struct O
  {
    template<int ... Is>
    struct D : Ts ... [Is] ...
    { };
  };

  template<int I>
  struct C
  { };

  O<C<0>, C<1>, C<2>>::D<2, 1> d;
  C<1> &c1 = d;
  C<2> &c2 = d;
}

namespace friend_nested_name_specifier
{
  template<typename... Ts>
  struct A
  {
    friend void Ts...[0]::f();

    template<typename U>
    friend void Ts...[0]::g();

    friend struct Ts...[0]::B;

    template<typename U>
    friend struct Ts...[0]::C;
  };

  struct X
  {
    void f();

    template<typename T>
    void g();

    struct B { };

    template<typename T>
    struct C { };
  };

  // Note: instantiation doesn't work yet, but seems like it's an unrelated
  // issue
  // template struct A<X>;
}

namespace template_argument_pack_index
{
  template<typename> void g();
  template<typename> struct C { };

  template<typename ... Ts>
  int f()
  {
    C<Ts...[0]> c;
    g<Ts...[0]>();
    return 0;
  }

  int i = f<int, int>();
}

namespace rescan_abort
{
  template<int ... Is>
  auto f(int) -> decltype(Is ...[5]) = delete;

  template<int ... Is>
  auto f(long) -> int;

  int x = f<1, 2>(1);
}

namespace nested_expansion
{
  template<int I, typename ... Ts> using A = Ts...[I];
  template<typename> struct C { };
  template<typename, int ... Is> struct X;

  template<typename>
  struct B;

  template<int ... Is>
  struct B<X<int, Is ...> > {
    template<typename ... Ts>
    using type = C<A<Is, Ts...>...>;
  };

  B<X<int, 0>>::type<char, short, int> *v0 = (C<char>*)0;
  B<X<int, 1>>::type<char, short, int> *v1 = (C<short>*)0;
  B<X<int, 2>>::type<char, short, int> *v2 = (C<int>*)0;
}

namespace return_type_substitution
{
  template<unsigned I>
  static constexpr auto g()
  { return I; }

  template<typename ... Ts>
  struct B
  {
    template<unsigned I>
    static auto f() -> Ts ... [g<I>()] *
    { return 0; }
  };

  template<auto ... Vs>
  struct C
  {
    template<unsigned I>
    static auto f() -> decltype(Vs ... [g<I>()]) *
    { return 0; }
  };

  int *p1 = B<int, long>::f<0>();
  long *p2 = B<int, long>::f<1>();

  int *q1 = C<0, 1L>::f<0>();
  long *q2 = C<0, 1L>::f<1>();
}

#if defined(__clang__) || !defined(__GNUC__)
// GCC doesn't support that syntax
namespace array_pack_param
{
  template<typename ... T>
  int f(T ... []);

  int i = f("Hello");
}
#endif

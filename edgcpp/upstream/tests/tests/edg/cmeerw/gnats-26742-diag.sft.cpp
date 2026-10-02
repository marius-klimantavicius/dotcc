//type:fn
//options:--c++11 -A:--c++20 -A

namespace param_after_pack
{
  template<int ... I, int J, int ... K> // diag for "J" and "K
  int f();

  template<typename ... T>
  struct C
  {
    template<T ... I, int J, int ... K> // OK
    static int f();

    template<T ... I, int ... J> // OK
    static int g();

    template<T ... I, int J>    // OK
    static int h();
  };

  auto f0 = C<char, short, int>::f<'a', 2, 3, 4>();
  auto f1 = C<char, short, int>::f<'a', 2, 3, 4, 5>();
  auto f2 = C<char, short, int>::f<'a', 2, 3, 4, 5, 6>();

  auto g0 = C<char, short, int>::g<'a', 2, 3>();
  auto g1 = C<char, short, int>::g<'a', 2, 3, 4>();
  auto g2 = C<char, short, int>::g<'a', 2, 3, 4, 5>();

  auto h0 = C<char, short, int>::h<'a', 2, 3, 4>();
}

#ifdef __cpp_concepts
namespace expand_type_constraint
{
  template<typename T, typename U>
  concept C2 = sizeof(T) == sizeof(U);

  template<typename>
  struct D
  { D(int); };

  template<typename ... Ts>
  struct B
  {
    template<C2<Ts> ... XT>
    static int g(D<XT> ...);
  };

  auto v0 = B<>::g<>();                          // OK
  auto v0a = B<>::g<>(1);                        // error
  auto v1 = B<int>::g<char>(1);                  // error
  auto v2 = B<int, char>::g<int, short>(1, 'a'); // error
}
#endif

namespace non_type_too_many_args
{
  template<int>
  struct X { };

  template<typename ...Ts> struct S {
    template<Ts ... Us> static void foo(X<Us> ... u);
  };

  void f() {
    S<int, int>::foo(X<1>(), X<2>());         // OK
    S<int, int>::foo(X<1>(), X<2>(), X<3>()); // error
  }
}

#ifdef __cpp_concepts
namespace type_too_many_args
{
  template<typename, typename>
  concept C = true;

  template<typename ...Ts> struct S {
    template<C<Ts> ... Us> static void foo(Us ... u);
  };

  void f() {
    S<int, int>::foo(1, 2);                   // OK
    S<int, int>::foo(1, 2, 3);                // error
  }
}
#endif

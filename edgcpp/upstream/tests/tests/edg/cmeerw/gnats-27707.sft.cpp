//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942:--ms_c++20 --microsoft_version 1942 --ms_permissive -DMS_PERMISSIVE
//options_all:-w

namespace minimal
{
  template<int ... Is>
  constexpr int f() {
    return []<int ... Js>(auto p) {
      return ((Is + Js) + ...);
    }.template operator()<1, 2>(0);
  };
  static_assert(f<10, 20>() == 33);  // Previously failed.  Now okay.
}

namespace non_lambda
{
  constexpr int g(int i, int j) { return i + j; }

  template<int ... Is>
  struct C
  {
    struct N
    {
      template<int ... Js>
      static constexpr auto f( auto ... ps )
      {
        return g( ( Is + Js + ps ) ...);
      }
    };

    static inline constexpr auto v = N::template f<1, 2>( 10, 20 );
  };

  static_assert(C<100, 200>::v == 333);
}

namespace lamba
{
  constexpr int g(int i, int j) { return i + j; }

  template<int ... Is>
  struct C
  {
    static inline constexpr auto v = []<int... Js>( auto ... ps ) {
      return g( ( Is + Js + ps ) ...);
    }.template operator()<1, 2>( 10, 20 );
  };

  static_assert(C<100, 200>::v == 333);
}

namespace name_lookup
{
  template<class T>
  void f()
  {
    constexpr int dim = 1;

    [] (auto) {
      int n[dim];
    } (0);
  }

  template void f<int>();
}

namespace capture
{
  void f()
  {
    [] (auto a) {
      [&] (auto b ) {
        return +a;
      } (1);
    } (1);
  }
}

namespace regression_constexpr_if
{
  template<int I = [ ] { if constexpr ( true ) return 29 ; } ()>
  int f()
  {
    return I;
  }
}

namespace dependent_calls
{
  struct B { };
  struct D : B { };

  constexpr int g(B) { return 1; }
  constexpr int g(char) { return 1; }

  template<typename T>
  constexpr int f(T t)
  {
    return [=] (auto p) -> int {
      return g(t) +
             10*g(p);
    } (t) + 100*g(t);
  }

  constexpr int g(D) { return 2; }
  constexpr int g(int) { return 2; }

#if !defined(MS_PERMISSIVE)
  static_assert((f(1) / 100) == 1);
#if !defined(__GNUC__)
  static_assert(f(1) == 111);   // still fails in GCC emulation mode, but
                                // that's likely a different issue
#endif
#else
  static_assert(f(1) == 222);   // MS in permissive mode gets the wrong result,
                                // and we emulate that
#endif
  static_assert(f(D()) == 222);
}

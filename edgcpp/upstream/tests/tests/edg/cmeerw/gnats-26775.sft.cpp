//type:fp
//options:--c++14:--c++20 -w -tused:--ms_c++20 -w -tused --microsoft_version 1936

namespace minimal
{
  template<typename ... T>
  auto f() {
    return [](auto, T ...) { };
  }
  auto l = f<short, int>();
}

#if __cpp_concepts
namespace pr_example
{
  template<typename ...T>
  void func() {
    []<class>(T...) {};
  }

  void test() {
    func<int, int>();
  }
}
#endif

namespace outer_expansion_and_inner_pack
{
  template<typename ... T>
  int f(T ... t)
  {
    return [] (T ... t, auto ... v) {
      return 0;
    } (t ..., 'c', 4);
  }

  auto v = f(1, 2L);
}

namespace use_outer_expansion
{
  template<typename ... T>
  int bar(T ...);

  void f()
  {
    auto l = [] (auto ... As) {
      return [](decltype(As) ... as, auto ... Bs) {
        bar(as ...);
        bar(Bs ...);
      };
    };

    l() (1, 'b');
    l(1) (1);
    l(1) (1, 'b');
    l(1, 2L) (1, 2L, 'c', 4, 5L);
    l(1, 2L) (1, 2L);
  }

  template<typename ... T>
  auto g(T ... t)
  {
    [t ...] (auto ... Bs) {
      return bar(t ...) + bar(Bs ...);
    };

#if __cpp_concepts
    [... u = t] (auto ... Bs) {
      return bar(u ...) + bar(Bs ...);
    };
#endif

    return [] (T ... as, auto ... Bs) {
      return bar(as ...) + bar(Bs ...);
    };
  }

  auto v = (g(1)(1),
            g(1)(1, 'b'),
            g(1, 2L)(1, 2L),
            g(1, 2L)(1, 2L, 'c', 4, 5L),
            0);
}

namespace multiple_levels
{
  template<typename ... T>
  int bar(T ...);

  void f()
  {
    auto l = [] (auto ... As) {
      return [] (decltype(As) ... as, auto ... Bs) {
        bar(as ...);
        return [Bs ...] (auto ... Cs) {
          bar(Bs ...);
          bar(Cs ...);
        };
      };
    };

    l(1) (1) ();
    l(1) (1, 'b') ();
    l(1, 2L) (1, 2L, 'c', 4, 5L) ();
    l(1, 2L) (1, 2L) ();

    l(1) (1) (10);
    l(1) (1, 'b') (10);
    l(1, 2L) (1, 2L, 'c', 4, 5L) (10);
    l(1, 2L) (1, 2L) (10);

    l(1) (1) (10, 11L);
    l(1) (1, 'b') (10, 11L);
    l(1, 2L) (1, 2L, 'c', 4, 5L) (10, 1LL);
    l(1, 2L) (1, 2L) (10, 1LL);
  }
}

//type:fn
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w

namespace regression_specialization
{
  class _A
  {
    class _B {};
  };

  template<int I,
      int J = [] { if constexpr (true) return [] { return I; }(); }()>
  struct A {
    template<>
    struct A<_A::_B>
    { };

    int f()
    {
      return I + J
    }
  };

  template <template<int I> class T> int f()
  {
    return T<29>().f();
  }

  int i = f<A>();
}

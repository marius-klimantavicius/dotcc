//type:fp
//options:--c++11:--c++20:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<int> using A = int;
  struct C {
    static constexpr int f(int i) { return i; }
    template<int ... Is> using type = A<f({Is ...})>;
  };
  template<typename T, typename = typename T::template type<1>>
  int g();
  int i = g<C>();  // Previously failed deduction, now okay.
}

namespace type_pack
{
  template<int> using A = int;
  struct C {
    static constexpr int f(int i) { return i; }
    template<typename ... Ts> using type = A<f({Ts{} ...})>;
  };
  template<typename T, typename = typename T::template type<int> >
  int g();
  int i = g<C>();
}

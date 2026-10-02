//type:fn
//options:--c++11 --gn 130200:--c++11 --clang_version 180200

template<bool>
struct C
{
  using type = int;
  static int v;
};

struct Non_Dependent
{
  template<bool B, typename C<B>::type = 0>
  void f();
};


template<int I>
struct Dependent
{
  template<bool B, typename C<I == 1 && B>::type = 0>
  void f2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  void f2();                    // accepted by GCC/Clang

  template<bool B, typename C<I == 1 && B>::type = 0>
  void s2();

  template<bool B, typename C<I == 2 && B>::type = 0>
  static void s2();             // accepted by GCC/Clang

};

Dependent<0> d0;

auto l1 = [] () {
  d0.f2<false>();               // ambiguous
  d0.s2<false>();               // ambiguous
};


template<int I>
struct FnType
{
  template<bool B>
  C<I == 1 && B> f2();

  template<bool B>
  C<I == 2 && B> f2();          // accepted by GCC/Clang

  template<bool B>
  void f4(typename C<I == 1 && B>::type);

  template<bool B>
  void f4(typename C<I == 2 && B>::type); // accepted by GCC/Clang

  template<bool B>
  void f5(decltype(C<I == 1 && B>::v));

  template<bool B>
  void f5(decltype(C<I == 2 && B>::v)); // accepted by GCC/Clang
};

FnType<0> ft0;

auto l2 = [] () {
  ft0.f2<false>();              // ambiguous
  ft0.f4<false>(0);             // ambiguous
  ft0.f5<false>(0);             // ambiguous
};

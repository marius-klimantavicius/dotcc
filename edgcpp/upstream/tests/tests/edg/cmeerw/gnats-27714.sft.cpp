//type:fp
//options:--c++20

namespace minimal
{
  template<typename>
  constexpr bool f() { return true; }
  template<typename T>
  struct B {
    template<int> static int g() requires (f<T>());
  };
  int i = B<int>::g<0>() + B<int>::g<1>();
}

namespace cache_concept
{
  constexpr bool g(int i)
  {
    for (unsigned int j = 0; j != 100000; ++j);
    return true;
  }
  template<int I> concept X = g(I);
  template<int I>
  struct B {
    template<int J> static int f() requires X<I>;
  };
  template<int N, int ... I> int i = (B<0>::f<N + I>() + ...);
  template<int N> int j = i<16*N, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10>;
  int k = j<0> + j<1> + j<2>  + j<3>  + j<4>  + j<5>  + j<6>  + j<7> +
          j<8> + j<9> + j<10> + j<11> + j<12> + j<13> + j<14> + j<15>;
}

namespace cache_evaluation
{
  template<typename T>
  constexpr bool g()
  {
    for (unsigned int j = 0; j != 100000; ++j);
    return true;
  }
  template<typename T>
  struct B {
    template<int J> static int f() requires (g<T>());
  };
  template<int N, int ... I> int i = (B<int>::f<N + I>() + ...);
  template<int N> int j = i<16*N, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10>;
  int k = j<0> + j<1> + j<2>  + j<3>  + j<4>  + j<5>  + j<6>  + j<7> +
          j<8> + j<9> + j<10> + j<11> + j<12> + j<13> + j<14> + j<15>;
}

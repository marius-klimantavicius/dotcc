//type:fp
//options:--c++11:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<bool B>
  struct C {
    static_assert(B, "Unexpected");
    using type = int;
  };
  template<typename... Us>
  using A = C<sizeof...(Us) != 0>;
  template<typename... Ts>
  typename A<int, Ts...>::type f();
  int i = f<>();
}

namespace alias_with_non_pack_and_pack
{
  template<int I, int S>
  struct C
  {
    static_assert(I == S, "Unexpected");
    using type = int;
  };

  template<int I, typename ... Us>
  using A = C<I, sizeof ... (Us)>;

  template<int I, typename ... Ts>
  typename A<I + 0, Ts  ...>::type f0();

  template<int I, typename ... Ts>
  typename A<I + 1, char, Ts  ...>::type f1();

  template<int I, typename ... Ts>
  typename A<I + 2, char, short, Ts  ...>::type f2();

  int i = f0<0>() + f0<1, char>() + f0<2, char, short>() +
          f1<0>() + f1<1, char>() + f1<2, char, short>() +
          f2<0>() + f2<1, char>() + f2<2, char, short>();
}

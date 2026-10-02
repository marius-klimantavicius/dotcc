//type:fp
//options:--c++11:--c++20:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<int> struct B;
  template<> struct B<2> {
    template<typename T, typename> using A = T;
  };
  template<typename ... Ts>
  using A1 = typename B<sizeof ... (Ts)>::template A<Ts ...>;
  template<typename ... Ts> using A2 = A1<char, Ts...>;
  template<typename ... Ts> struct C;
  template<typename ... Ts> struct C<A2<Ts ...>, Ts ...> { };
  C<char, int> c;
}

/*
Two distinct alias-template scopes (`A2` and the body of `C`'s partial
specialization) both build the dependent class instance `B<sizeof...(D,
Ts...)>` through `A1`.  Both packs sit at template-parameter coordinates (1,1),
so the cache key for the dependent expression is identical and the second scan
reuses the first instance --- with token bindings that refer to the wrong pack.
Substituting `int` then evaluates `B<1>` instead of `B<2>` and the partial
specialization fails to match.
*/

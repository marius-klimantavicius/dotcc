//type:fp
//options:--c++11:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1938

namespace minimal
{
  template<typename ...> struct C {};
  template<typename, typename> struct E {};
  template<typename T, typename ... Us> using A = C<E<T, Us> ...>;
  template<typename ... Ts, typename ... Us, typename = C<A<Ts, Us ...> ...>>
  int f(C<Ts ...>, Us ...);
  int i = f(C<int>{}, 0);
}

namespace non_type_params
{
  template<int> struct S { };
  template<int, typename> struct E { };
  template<typename ...> struct L { };
  template<typename ...> struct R { };
  template<int I, typename... Ts> using A = R<E<I, Ts> ...>;
  template<int ... Is, typename ... Ts, typename = L<A<Is, Ts ...> ...>>
  int f(S<Is ...>, Ts ...);
  auto v = f(S<1>{}, 1);
}

namespace type_params
{
  template<typename> struct S { };
  template<typename, typename> struct E { };
  template<typename ...> struct L { };
  template<typename ...> struct R { };
  template<typename I, typename... Ts> using A = R<E<I, Ts> ...>;
  template<typename ... Is, typename ... Ts, typename = L<A<Is, Ts ...> ...>>
  int f(S<Is ...>, Ts ...);
  auto v = f(S<int>{}, 1);
}

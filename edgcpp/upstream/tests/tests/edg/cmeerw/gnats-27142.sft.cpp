//type:fp
//options:--c++11 -A:--c++20 -A:--c++11 --gn 130200:--c++11 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<int> struct D {
    template<typename U> using X = U;
  };
  template<int I, typename ... T> struct C {
    template<typename>
    using A = typename D<I>::template X<int, T ...>;
  };
  C<0> c;
}

namespace instantiated_list_for_alias
{
  template<bool> struct D
  {
    template<typename T> using X = T;
  };

  template<bool B, typename ... T> struct C
  {
    typename D<B>::template X<int, T ...> m;

    template<typename>
    using A = typename D<B>::template X<int, T ...>;

    template<typename>
    static int f(typename D<B>::template X<int, T ...>);
  };

  C<true> c;
  C<true>::A<void> a;

  auto i = C<true>::f<void>(0);
}

namespace instantiated_list_for_class
{
  template<bool> struct D
  {
    template<typename>
    struct X
    { };
  };

  template<bool B, typename ... T> struct C
  {
    typename D<B>::template X<int, T ...> m;

    template<typename>
    using A = typename D<B>::template X<int, T ...>;

    template<typename>
    static int f(typename D<B>::template X<int, T ...>);
  };

  C<true> c;
  C<true>::A<void> a;

  auto i = C<true>::f<void>({});
}

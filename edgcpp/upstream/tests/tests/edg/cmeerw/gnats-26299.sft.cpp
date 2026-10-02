//type:fp
//options:--c++20:--ms_c++20

namespace minimal {
  template<typename...> struct B {
    using type = int;
  };
  template<typename... TT> struct C;
  template<typename... TT> struct C {
    template<typename> using A = typename B<TT ...>::type;
    template<A<int> = 0> C(TT ...);
  };
  C c{1};
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace declared
{
  template<bool>
  struct A
  {
    using type = bool;
  };

  template<typename...>
  struct B
  {
    static constexpr bool v = true;
  };

  template<typename...>
  struct C;

  template<typename... TT>
  struct C
  {
    template<bool>
    using D = typename A<B<TT ...>::v>::type;

    template<D<false> = true> C(TT ...);
  };

  C c1{ 1 };
  static_assert(is_same_v<decltype(c1), C<int>>);

  C c2{ 1, 2L };
  static_assert(is_same_v<decltype(c2), C<int, long>>);
}

namespace defined
{
  template<bool>
  struct A
  {
    using type = bool;
  };

  template<typename...>
  struct B
  {
    static constexpr bool v = true;
  };

  template<typename... TT>
  struct C
  {
    template<bool>
    using D = typename A<B<TT ...>::v>::type;

    template<D<false> = true> C(TT ...);
  };

  C c1{ 1 };
  static_assert(is_same_v<decltype(c1), C<int>>);

  C c2{ 1, 2L };
  static_assert(is_same_v<decltype(c2), C<int, long>>);
}

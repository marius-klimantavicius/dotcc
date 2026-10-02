//type:fp
//options:--c++20;fn:--c++20 --gn 140100

namespace minimal
{
  template<int, int> struct D;
  template<typename> struct C {};
  C() -> C<D<1, 2>>;
  template<int I> using A = C<D<I, +I>>;
  A a{};
}


template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace is_deducible_with_non_deduced_args
{
  template<int, int> struct D;

  template<typename T>
  struct C
  {
    C(int);
  };

  template<int I>
  using A = C<D<I, +I>>;

  C(long) -> C<D<1, 1>>;        // #1
  C(int) -> C<D<1, 2>>;         // #2

  C c(1);
  static_assert(is_same_v<decltype(c), C<D<1, 2>>>);

  A a(1);                       // gcc considers both #1 and #2
#if defined(__GNUC__)
  using deduced_type = C<D<1, 2>>;
#else
  using deduced_type = C<D<1, 1>>;
#endif
  static_assert(is_same_v<decltype(a), deduced_type>);
}

#if defined(__GNUC__) && !defined(__clang__)
namespace pr_example
{
  template<int...> struct IS;

  template<class> struct TX;

  template<int... Is>
  struct TX<IS<Is...>>
  {
    int mem;
  };

  template<class... T>
  using AX = TX<IS<__integer_pack(sizeof...(T))...>>;

  TX(int) -> AX<int>;

  AX t(1);
}
#endif

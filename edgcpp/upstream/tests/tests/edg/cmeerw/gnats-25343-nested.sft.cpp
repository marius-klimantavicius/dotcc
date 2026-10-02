//type:fp
//options:--c++20:--ms_c++20 --microsoft_version=1927

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;


namespace simple
{
  struct C
  {
    struct A
    {
      int i;
      int j;
    };
  };

  template<typename U>
  struct Outer
  {
    template<typename T>
    struct NonDpdtMbr
    {
      C::A u;
      T t;
    };

    template<typename T>
    struct DpdtMbr
    {
      typename U::A u;

      T t;
    };
  };

  static_assert(is_same_v<decltype(Outer<C>::NonDpdtMbr{ { 1, 2 }, 3 }),
                Outer<C>::NonDpdtMbr<int>>);
  static_assert(is_same_v<decltype(Outer<C>::NonDpdtMbr{ 1, 2, 3 }),
                Outer<C>::NonDpdtMbr<int>>);
  static_assert(is_same_v<decltype(Outer<C>::NonDpdtMbr{ { 1, 2 }, 'a' }),
                Outer<C>::NonDpdtMbr<char>>);
  static_assert(is_same_v<decltype(Outer<C>::NonDpdtMbr{ 1, 2, 'a' }),
                Outer<C>::NonDpdtMbr<char>>);

  static_assert(is_same_v<decltype(Outer<C>::DpdtMbr{ { 1, 2 }, 3 }),
                Outer<C>::DpdtMbr<int>>);
  static_assert(is_same_v<decltype(Outer<C>::DpdtMbr{ { 1, 2 }, 'a' }),
                Outer<C>::DpdtMbr<char>>);
}

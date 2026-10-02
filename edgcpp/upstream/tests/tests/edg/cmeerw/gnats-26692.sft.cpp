//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  enum E { E0, E1 };
  const E &e0 = E0;
  E e = true ? e0 : E1;
}

namespace minimal_decltype
{
  enum E { E1 };
  const E &e = E1;
  const decltype(true ? e : E1) *p = &e;
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace INT
{
  const int ci1 = 1;
  const int ci2 = 2;

  extern const int eci1;
  extern const int eci2;

  const int &r1 = 1;
  const int &r2 = 2;

  extern const int &er1;
  extern const int &er2;

  static_assert(is_same_v<decltype(true ? ci1 : ci2), const int &>);
  static_assert(is_same_v<decltype(true ? eci1 : eci2), const int &>);
  static_assert(is_same_v<decltype(true ? r1 : r2), const int &>);
  static_assert(is_same_v<decltype(true ? er1 : er2), const int &>);
}

namespace ENUM
{
  enum E
  {
    E1
  };

  const E ce1 = E1;
  extern const E ece1;
  const E &re1 = E1;
  extern const E &ere1;

  static_assert(is_same_v<decltype(true ? ce1 : E1), E>);
  static_assert(is_same_v<decltype(true ? ece1 : E1), E>);
  static_assert(is_same_v<decltype(true ? re1 : E1), E>);
  static_assert(is_same_v<decltype(true ? ere1 : E1), E>);
}

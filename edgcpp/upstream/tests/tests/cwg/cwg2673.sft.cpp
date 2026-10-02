//options_all:--c++20 -tused -A
  #include <compare>

  enum class E : int {
    Lo = 0,
    Hi = 1
  };

  constexpr auto operator<=>(E lhs, E rhs) -> std::strong_ordering {
    return (int)rhs <=> (int)lhs;
  }

  // everybody agrees this is true
  static_assert((E::Lo <=> E::Hi) == std::strong_ordering::greater);

  // gcc rejects this, msvc and clang accept
  static_assert(E::Lo > E::Hi);  // #1

//cwg: 2673
//title: User-declared spaceship vs. built-in operators
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26071
//fixed_in: 6.8

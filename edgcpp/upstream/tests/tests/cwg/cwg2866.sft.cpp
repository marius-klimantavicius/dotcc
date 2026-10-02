//options_all:--c++23 -A
  struct C1
  {
    C1() = default;
    ~C1() noexcept (false) = default;
  };

  struct C2
  {
    C2() = default;
    ~C2() noexcept (false) { }
  };

  static_assert(!noexcept(C1()));
  static_assert(!noexcept(C2()));


//type:fn
//options_all:--c++23 
  struct B {
  protected:
    constexpr operator int() const { return 0; }
  };
  struct D : B {
    constexpr bool operator==(const D&) const = default;
  };
  template<typename T> constexpr auto comparable(T t) -> decltype(t == t) { return t == t; }
  constexpr bool comparable(...) { return false; }
  static_assert(comparable(D{}));

//remark:abbreviate friend template type constraints
//options:--c++20;fp

  template<typename T, typename U, int i> concept C = true;
  template<typename T, int I> struct X {
    friend constexpr auto operator+(C<T, I> auto) {
      return true;
    }
  };
  static_assert(+X<int, 42>{});

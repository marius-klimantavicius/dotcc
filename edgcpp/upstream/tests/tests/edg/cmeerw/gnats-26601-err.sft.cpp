//type:fn
//options:--c++20:--ms_c++20
//options_all:-w -tused

namespace expect_instantiation
{
  template<typename T>
  struct C
  {
    constexpr C() = default;
    constexpr C(const C &o) : i(o.i) { }
    constexpr C(C &&o) : i(o.i) { T t; } // error expected
    int i{1};
  };

  template<typename T>
  struct X
  {
    C<T> c{};
  };

  constexpr auto f(auto ... x)
  {
    auto l = [... cx = x]() { return (cx.c.i + ...); };
    return l;
  };

  int i = f(X<void>(), X<int>())();
}

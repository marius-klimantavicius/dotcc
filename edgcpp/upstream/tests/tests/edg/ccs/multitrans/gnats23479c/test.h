template<bool> struct X;

struct Y {
  static constexpr bool val = true;
};

template<typename> struct Z
{
  using MY = Y;
  using MX = X<MY::val>;

  int mem_var = 0;

  Z() {}
};

struct Foo {
  Z<int> var{};
};

//type:fp
//options:--c++17:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

template<typename T>
struct C
{
  static int f(int);
};

namespace ns
{
  constexpr int i = 1;
}

template<>
constexpr int C<int[1]>::f(int i)
{
  auto l = [] (auto v) { return v + ns::i; };
  return [=] () {
    return l(i);
  }();
}

static_assert(C<int[1]>::f(2) == 3, "");

template<>
constexpr int C<int[2]>::f(int i)
{
  auto l = [] (int v) { return v + ns::i; };
  return [=] () {
    return l(i);
  }();
}

static_assert(C<int[2]>::f(2) == 3, "");

template<>
constexpr int C<int[3]>::f(int i)
{
  auto l = [] (auto v) { return v + ns::i; };
  return [=] (auto) {
    return l(i);
  }(0);
}

static_assert(C<int[3]>::f(2) == 3, "");

template<>
constexpr int C<int[4]>::f(int i)
{
  auto l = [] (int v) { return v + ns::i; };
  return [=] (auto) {
    return l(i);
  }(0);
}

static_assert(C<int[4]>::f(2) == 3, "");

template<>
constexpr int C<int[5]>::f(int i)
{
  return [=] (auto) {
    auto l = [] (auto v) { return v + ns::i; };
    return l(i);
  }(0);
}

static_assert(C<int[5]>::f(2) == 3, "");

template<>
constexpr int C<int[6]>::f(int i)
{
  return [=] (auto) {
    auto l = [] (int v) { return v + ns::i; };
    return l(i);
  }(0);
}

static_assert(C<int[6]>::f(2) == 3, "");

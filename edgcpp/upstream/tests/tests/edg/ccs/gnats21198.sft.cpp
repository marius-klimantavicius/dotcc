//type:cp
//options_all:--c++20

constexpr auto x = [](){ return 0; } = {};

void f(bool b) {
  (b ? static_cast<decltype(x)>(x) : x)();
}

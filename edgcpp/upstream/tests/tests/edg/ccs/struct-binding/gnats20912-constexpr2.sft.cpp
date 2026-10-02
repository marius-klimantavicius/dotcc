//type:fn
//options_all:--c++20

void f() {
  const int x = 5;
  const int& xr = x;

  static_assert(xr == 5);
}

//type:cp
//options_all:--c++20 -tused

template<auto lam = [] {
  struct A {int x = 29;};
  new A;
}>
auto f() { return lam; }

void g() {
  f<0>();
}

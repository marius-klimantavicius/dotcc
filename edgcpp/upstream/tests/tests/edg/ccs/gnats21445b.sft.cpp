//type:cp
//options_all:--c++17

template<int I = [] {
  struct A {
    int&& x = 29;
  };
  decltype(new A) x;
  return 0;
}()>
int f();

void g() {
  f<>();
  f<1>();
}

//type:cp
//options:--c++11:--c++17
//options_all:-tused

struct X {
  ~X();
};
template<typename T> struct A {
  A(int, X = {});
};
struct B : A<int> {
  using A::A;
};
void func() {
  B(0);
}

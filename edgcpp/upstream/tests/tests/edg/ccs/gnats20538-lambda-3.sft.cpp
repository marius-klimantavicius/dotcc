//type:rp
//options:--c++14:--c++17

struct A{ int z; };
struct B {
  A c{3}, d{4};
  int foo(void) {
    int x = 10, y = 20;
    constexpr A a{1}, b{2};
    auto lam1 = [=]() -> int {
      auto lam2 = [a,&x] { return a.z + x; };
      return lam2() * 20 + (b.z + y) - (c.z - d.z);
    };
    return lam1();
  }
};

int main() {
  B b;
  if (b.foo() != 243) return 1;
}

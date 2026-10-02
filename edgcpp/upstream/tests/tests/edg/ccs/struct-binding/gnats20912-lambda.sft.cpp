//type:rp
//options_all:--c++20

int foo[3] = {1, 2, 3};

int main() {
  auto [a, b, c] = foo;

  auto l1 = [a, b](int i) { return a + b + i; }; // Explicit capture
  auto l2 = [=] { return l1(c); }; // Implicit capture

  if (l2() != 6) return 1;
}

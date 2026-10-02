//type:rp
//options:--c++14:--c++17

int foo(int a, int b) {
  return a - b;
}

int bar(int x) {
  int y = 10;
  auto lam1 = [&x, w = 5] (int z) { return z + x + w; };
  return foo(lam1(x), [=] { return lam1(y) * x; }());
}

int main() {
  if (bar(11) != -259) return 1;
}


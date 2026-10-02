//type:rp
//options:--c++14:--c++17

struct S1_t { int x; };
int foo(void) {
  auto lam1 = [x=1] { return x * 20; };
  return lam1();
}

int main() {
  if (foo() != 20) return 1;
}


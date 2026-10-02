//type:rp
//options:--c++11:--c++17

struct S1_t { int x; };
int foo(void) {
  S1_t temp1{10};

  auto lam1 = [=] { return temp1.x * 20; };
  return lam1();
}

int main() {
  if (foo() != 200) return 1;
}


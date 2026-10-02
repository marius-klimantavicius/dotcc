//type:rp
//options_all:--c++11

int f(void){ return 10; }
auto (*g)(void) -> int;

int main() {
  auto (*h)(void) -> int;
  g = f;
  h = g;
  if (g() != 10 || h() != 10) return 1;
}


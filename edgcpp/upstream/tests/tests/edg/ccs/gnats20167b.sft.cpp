//type:rp
//options::--g++:--clang
//options_all:--c++17 --no_exceptions

struct S { S() {}; S(const S&) = delete; };

int main() {
  int i = 0;
  S qqq = (1.0f, i = 42, S{});

  if (i != 42) return 1;
  return 0;
}

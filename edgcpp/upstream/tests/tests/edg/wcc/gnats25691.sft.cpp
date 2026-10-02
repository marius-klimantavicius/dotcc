//type:cp
//options:--no_strict:--g++:--microsoft:--clang
//options_all:--c++20

int main() {
  static_assert(__func__[0] == 'm');
  return 0;
}

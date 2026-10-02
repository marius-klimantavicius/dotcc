//type:cp
//options::-DNEG;fn
//options_all:--c++20 -W

void f() {
  int arr[3] = {1, 2, 3};
#ifndef NEG
  [[maybe_unused]]
#endif /* NEG */
  auto [a, b, c] = arr;
}

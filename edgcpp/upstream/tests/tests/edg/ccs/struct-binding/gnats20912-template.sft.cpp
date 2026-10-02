//type:fn
//options_all:--c++20

template<auto Var> auto [X, Y] = Var;

void f() {
  const int arr[3] = {1, 2, 3};

  int x = X<arr>;
  int y = Y<arr>;
}

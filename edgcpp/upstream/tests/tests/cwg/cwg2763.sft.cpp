//options_all:--c++23 -A
  [[noreturn]] constexpr void f() {}
  constexpr int x = (f(), 0);

//cwg: 2763
//title: Ignorability of [[noreturn]] during constant evaluation
//meeting: Kona 11/23
//edg_status: Passes

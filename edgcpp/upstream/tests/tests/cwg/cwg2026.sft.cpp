//type:fp
//options_all:--c++17 -tused -A
//

struct t {
  int x, y;
  constexpr t(int n = 0) : x(n), y(x) {}
};
constexpr t t1;

//cwg: 2026
//title: Zero-initialization and constexpr
//meeting: Kona 10/15
//edg_status: Passes

//type:fn
//options_all:--c++17 -tused -A

struct A2 {
  int i;
  A2(int i) : i(i) {}
};

static_assert(__is_trivial(A2));

//type:fn
//options_all:--c++17 -tused -A

struct A1 {
  int i;
  A1(int i) : i(i) {}
  A1() = delete;
};

static_assert(__is_trivial(A1));

//cwg: 1496
//title: Triviality with deleted and missing default constructors
//meeting: Jacksonville 2/16
//edg_status: Passes

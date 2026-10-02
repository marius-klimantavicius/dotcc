//type:fn
//options_all:--c++14 -tused -A
  void f(int n) {
    static constexpr int *&&r = &n;
  }

//cwg: 1694
//title: Restriction on reference to temporary as a constant expression
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

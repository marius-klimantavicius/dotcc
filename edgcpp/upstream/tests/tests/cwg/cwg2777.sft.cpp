//options_all:--c++23 -tused -A
  struct A {};

  template<auto a, auto x>  // also consider A a and const auto x
  int f() {
    decltype(a) b;          // also consider decltype((a))
    A& rb = b;
    decltype(x) y;
    int& ry = y;
  }

  int x = f<A{}, 42>();

//cwg: 2777
//title: Type of id-expression denoting a template parameter object
//meeting: Tokyo 3/24
//edg_status: Passes

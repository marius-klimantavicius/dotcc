//options_all:--c++23 -A
  void g() {
    constexpr int x = 1;
    auto lambda = [] <typename T, int = ((T)x, 0)> {};  // OK
    lambda.operator()<int, 1>();         // OK, does not consider x at all
    lambda.operator()<int>();            // OK, does not odr-use x
  }

  void h() {
    constexpr int x = 1;
    auto lambda = [] <typename T> { (T)x; };  // OK
    lambda.operator()<int>();            // OK, does not odr-use x
    lambda.operator()<void>();           // OK, does not odr-use x
  }

//cwg: 2745
//title: Dependent odr-use in generic lambdas
//meeting: Tokyo 3/24
//edg_status: EDGcpfe/27123

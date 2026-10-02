//type:fn
//options_all:--c++23 -A
  void g() {
    constexpr int x = 1;
    auto lambda = [] <typename T, int = ((T)x, 0)> {};  // OK
    lambda.operator()<const int&>();  // error: odr-uses x from a context where x is not odr-usable
  }

  void h() {
    constexpr int x = 1;
    auto lambda = [] <typename T> {(T)x; };  // OK
    lambda.operator()<const int&>();  // error: odr-uses x from a context where x is not odr-usable
  }

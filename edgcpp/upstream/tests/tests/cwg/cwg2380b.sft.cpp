//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  void f(int n) {
    struct A {
      void f() { n = 2; } // error, n is not odr-usable due to intervening function definition scope
    };
  }

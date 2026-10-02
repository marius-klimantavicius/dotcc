//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  void f(int n) {
    [] { n = 1; };        // error, n is not odr-usable due to intervening lambda-expression
  }

//cwg: 2380
//title: capture-default makes too many references odr-usable
//meeting: Kona 02/19
//edg_status: Passes

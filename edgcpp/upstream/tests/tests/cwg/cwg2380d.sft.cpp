//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  void f(int n) {
    [=](int k = n) {};    // error, n is not odr-usable due to being outside the block scope of the lambda-expression
  }

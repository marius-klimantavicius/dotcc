//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  void f(int n) {
    [&] { [n]{ return n; }; }; // OK
  }

//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  struct A { 
    int a; 
  private: 
    int b; 
    constexpr auto g() { return &a <=> &b; } // returns unspecified value 
  }; 

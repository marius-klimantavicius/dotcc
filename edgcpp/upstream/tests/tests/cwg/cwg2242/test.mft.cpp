//type:fn
//options_all:--c++20 -tused --multi_trans_unit tu2.cpp
//source_files:tu2.cpp
  // tu1.cpp
  extern const int a = 1;
  inline auto f() {
    static const int b = a;
    struct A { auto operator()() { return &b; } } a;
    return a;
  }

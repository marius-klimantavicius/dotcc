//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
#include <compare>
  struct B { int n; }; 
  struct C : B { int m; } c; 
  constexpr auto x = &c.n < &c.m;   // not constant 
  constexpr auto y = &c.n <=> &c.m; // returns unspecified value 

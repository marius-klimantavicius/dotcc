//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
#include <compare>

struct B { int n; }; 
struct C : B { int m; } c; 
constexpr auto y = &c.n <=> &c.m; // returns unspecified value 

//cwg: 2368
//title: Differences in relational and three-way constant comparisons
//meeting: Kona 02/19
//edg_status: Passes

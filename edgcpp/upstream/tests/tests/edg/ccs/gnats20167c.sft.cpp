//type:cp
//options::--g++:--clang
//options_all:--c++17

struct S { S() = default; S(const S&) = delete; };
 
S s = (42, S{});

//type:fn
//options::--gnu_version 70400:--clang_version 80000:--microsoft;fp
//options_all:--c++11

struct S { S(){} };

auto s = S::S();
auto s2 = ::S();
auto s3{S::S()};
auto s4{::S()};

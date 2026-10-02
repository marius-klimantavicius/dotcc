//remark:Parenthesized aggr init
//options:--c++20;fp

struct A {
       int x;
       int y;
} a = A({ 42 });

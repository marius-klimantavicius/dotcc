//remark:Parenthesized aggr init
//options:--c++20;fp

struct B {};
struct D: B {
  int x;
};

D d = D({.x = 42 });


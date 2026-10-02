//remark:Parenthesized aggr init
//options:--c++20;fp

struct B { int x; };
struct D: B {
};

D d = D({.x = 42 });


//remark:Parenthesized aggr init
//options:--c++20;fp

struct X { int i; char *pc = nullptr; };

#if 1
X x1(1, nullptr);
X x2(1);
X x3 = X(1, nullptr);
X x4 = X(1);
#endif
X x5 = (X)(1);
X x6 = static_cast<X>(1);

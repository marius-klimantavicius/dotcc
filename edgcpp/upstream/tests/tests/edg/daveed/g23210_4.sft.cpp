//remark:Parenthesized aggr init
//options:--c++20;fp

using A = int[3];
auto const &r1 = static_cast<A>(42);
auto const &r2 = (A)(42);
A x(1);


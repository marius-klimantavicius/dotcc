//remark:Move assignment and closure types
//options:--c++11;fn:--c++11 -A;fn

auto x = []{};

typedef decltype(x) T;

struct A : T {};

typedef A& (A::*pmf)(A&&);

pmf p = &A::operator=;

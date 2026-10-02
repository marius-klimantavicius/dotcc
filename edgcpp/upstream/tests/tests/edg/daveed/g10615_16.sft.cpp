//remark:Generated move operator=
//options:--c++11;fp

struct A {};

typedef A& (A::*pmfA1)(const A&);
typedef A& (A::*pmfA2)(A&&);

pmfA1 p1 = &A::operator=;
pmfA2 p2 = &A::operator=;


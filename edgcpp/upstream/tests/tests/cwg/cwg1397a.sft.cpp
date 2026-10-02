//type:fn
//options_all:--c++14 -tused -A
struct A
{
 A(int i =f());
 static int f(int i = 42) { return i; }
};

A a;

//cwg: 1397
//title: Class completeness in non-static data member initializers
//meeting: Urbana-Champaign 11/14
//edg_status: Passes

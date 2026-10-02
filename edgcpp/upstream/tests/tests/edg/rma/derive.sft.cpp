//options_all:-r -x -tused
//options: --strict;cp

struct A { int a; };
struct B : public A { int a; };
struct C : public B { int a; int f(); };
int C::f() { return a + B::a + A::a; }


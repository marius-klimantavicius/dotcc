//options_all:-r -x -tused
//options: --strict;cp

struct A { int a[10000]; };
struct B { int i; };
struct C : public A, public B { };
struct D : public B, public A { };


//options_all:-r -x -tused
//options: --strict;cp

struct A { int i; };
struct B : public A { int i; };
struct C : public B {};


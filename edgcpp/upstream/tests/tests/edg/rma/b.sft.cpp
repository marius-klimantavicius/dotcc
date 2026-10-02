//options_all:-r -x -tused
//options: --strict;cp

struct A {int a,b,c;};
struct B : A {int a;};
struct C : A {int b;};
struct D : B, C {};


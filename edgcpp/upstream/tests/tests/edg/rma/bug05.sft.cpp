//options_all:-r -x -tused
//options: --strict;cp

struct V { int v; };
struct A : virtual V { int a; };
struct B { int b; };
struct C : virtual A { int c; };
struct X : virtual C { int x; };
struct D : virtual A, virtual B { int d; };
struct Y : C, virtual D { int y; };
struct Z : virtual C, D { int z; };


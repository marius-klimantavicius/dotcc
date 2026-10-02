//options_all:-r -x -tused
//options: --strict;cp

struct A { int a; };
struct B : virtual public A { int b; };
struct C : virtual public B { int c; };
struct CX : virtual public B, virtual public A { int cx; };
struct D : virtual public C { int d; };
struct Dx : virtual public C, virtual public A { int dx; };
struct Dy : virtual public C, virtual public B { int dy; };
struct Dz : virtual public C, virtual public A, virtual public B { int dz; };



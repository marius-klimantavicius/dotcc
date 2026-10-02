//options_all:-r -x -tused
//options: --strict;cp

struct A { int a,b,c; A(); };
struct B { B(); };
struct C { int i,j,k; };    /* No constructor */
struct D : public B, public C { int a, b[20]; A x, y[2]; D(); };
D d1;
D d2 = d1;


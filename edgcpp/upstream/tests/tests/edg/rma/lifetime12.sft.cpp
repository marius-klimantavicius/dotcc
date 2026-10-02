//options_all:-r -x -tused
//options: --strict;cp

struct A { A(); ~A(); };
struct B { A x; B(A); ~B(); };
struct C { C(); ~C(); };
struct D : public A, public B, public C { D(); ~D(); };
D::D() : C(), B(A()) { }


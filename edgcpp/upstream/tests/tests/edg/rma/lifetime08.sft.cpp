//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A { A(); ~A(); };
struct B { B(A); ~B(); };
struct C { C(); ~C(); };
struct X : public A, public B, public C { X(); ~X(); };
X::X() : B(A()), C() { };
X::~X() { };


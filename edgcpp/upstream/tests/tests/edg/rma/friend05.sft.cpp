//options_all:-r -x -tused
//options: --strict;cn

class A { void f(); };
class B { friend void A::f(); void g(); };
void A::f() { B b; b.g(); }  // B::g() is accessible because of friend decl
void f() { B b; b.g(); }     // B::g() is inaccessible


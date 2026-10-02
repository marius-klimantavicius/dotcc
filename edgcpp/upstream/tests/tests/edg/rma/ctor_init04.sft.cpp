//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a,b,c; A(const A&, int); };
class B : public A { B(const B&); };
class C : public A { C(const C&); };
B::B(const B&) {}
A::A(const A&, int i = 0) {}
C::C(const C&) {}                   // Error


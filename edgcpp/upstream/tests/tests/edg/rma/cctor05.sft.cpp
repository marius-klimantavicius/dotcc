//options_all:-r -x -tused
//options: --strict;cp

class A { A(A&); };
class B { B(const B&); };
class C : public A {};
class D : public B {};
class E { A x; B y; };


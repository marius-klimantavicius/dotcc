//options_all:-r -x -tused
//options: --strict;cn

class A { A(); };
class B { B(int); };
class C : public A, public B { C(); C(int); };
C::C() { }
C::C(int) { }


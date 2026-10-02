//options_all:-r -x -tused
//options: --strict;cn

class A { A(); A(int); };
class B : public A { B() : A(1) {}; };
class C : public A { C(); };
C::C() : A(1) {}


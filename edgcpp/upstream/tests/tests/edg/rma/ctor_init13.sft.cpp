//options_all:-r -x -tused
//options: --strict;cn

class A { };

A a1 = A();
A *pa1 = new A();
class B : public A { B() : A() { } };

A a2 = A(0);
A *pa2 = new A(0);
class C : public A { C() : A(0) { } };

struct X { operator A(); } x;
A a3 = A(x);
A *pa3 = new A(x);
class D : public A { D() : A(x) { } };

A a4 = A(0,0);
A *pa4 = new A(0,0);
class E : public A { E() : A(0,0) { } };



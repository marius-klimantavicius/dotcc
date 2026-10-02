//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a,b,c; virtual void f() = 0; };
class B : public A { };
class C : public B { virtual void g() = 0; };
class D : public A { virtual void f() = 0; }
A a;  // error: object of abstract class
B b;  // should also be an error -- abstract by inheritance
C c;
D d;


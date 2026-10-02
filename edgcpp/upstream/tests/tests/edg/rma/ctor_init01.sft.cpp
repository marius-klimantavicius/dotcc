//options_all:-r -x -tused
//options: --strict;cn

class A { int a,b; A(int,int); };
class B { int x; B(int); };
class C { int i; C(); };
class D { int i; D(); };
class X : public A, public B, public C { D d; X(); };
X::X() : B(0), A(-1,-1) {}


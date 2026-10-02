//options_all:-r -x -tused
//options: --strict;cn

class A { A(); ~A(); };
class B : public A {};
class C { A x; };
class D : public C, public B { D(); };
D d;


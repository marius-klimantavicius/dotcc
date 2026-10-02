//options_all:-r -x -tused
//options: --strict;cn

class A { A(); };
class B { B(int); };
class D : public A, public B { D(); };
D::D() : A(), B() { }


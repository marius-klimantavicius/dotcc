//options_all:-r -x -tused
//options: --strict;cn

class A { A(); };
class B { B(int); };
class E : public A, public B { };
E e;


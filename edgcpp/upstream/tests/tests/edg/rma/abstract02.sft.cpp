//options_all:-r -x -tused
//options: --strict;cn

class A { virtual int f() = 0; };
typedef A Atype;
A f(A a, A *b, Atype c);
Atype g(A, A*, Atype, Atype*);
A x, *y;


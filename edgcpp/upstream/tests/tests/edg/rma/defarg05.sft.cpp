//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a,b,c; A(); A(int); };
A x;
A::A(int i = 0) { a = b = c = i; }
A y;


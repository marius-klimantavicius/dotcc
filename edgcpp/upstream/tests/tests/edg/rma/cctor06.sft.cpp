//options_all:-r -x -tused
//options: --strict;cp

class A { public: int a,b,c; A(); A(A&); A(const A&, int=0); A(int); };
A x = 1;
class B : public A { public: B(const B&); B(); };
const B y, z = y;


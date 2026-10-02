//options_all:-r -x -tused
//options: --strict;cp

class A { public: A(); };
class B { public: static A x, y[], z[3]; };
A B::x;
A B::y[2];
A B::z[3];


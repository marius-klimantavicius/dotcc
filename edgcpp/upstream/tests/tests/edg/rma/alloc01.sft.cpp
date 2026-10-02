//options_all:-r -x -tused
//options: --strict;cp

class A { int i, j, k; };
class B { int a, b; };
class C : public A, public B { int x, y, z; };


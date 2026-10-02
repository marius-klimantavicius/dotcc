//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

class A { public: int i, j; };
class B : virtual public A { public: int i; };
class C : virtual public A { public: int j; };
class D : public B, public C { int f() { return i+j; } };


//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

class A { public: int i; };
class B : private A { public: A::i; void f() { i = 0; } };
class C : private B { public: B::i; void f() { i = 1; } };
class D : private C { public: C::i; void f() { i = 2; } };


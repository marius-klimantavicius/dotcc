//options_all:-r -x -tused
//options: --strict;cp

class A { public: int a; };
class B : public A {};
class C : public A {};
class D : private B, private C { public: A::a; };


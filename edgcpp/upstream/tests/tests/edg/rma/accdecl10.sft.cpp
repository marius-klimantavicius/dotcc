//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a,b; };
class B : virtual private A { public: A::a; };
class C : virtual private A { public: A::b; };
class D : private B, private C { public: A::a; A::b; };


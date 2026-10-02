//options_all:-r -x -tused
//options: --strict;cp

class A { public: int a,b,c,d; };
class B : protected A {};
class C : private B { protected: A::a; B::b; public: A::c; B::d; };


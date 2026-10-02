//options_all:-r -x -tused
//options: --strict;cn

class A { public: A(int); };
class B : public A { public: B() : (0) {} };
class C : public A, public B { public: C() : (0) {} };


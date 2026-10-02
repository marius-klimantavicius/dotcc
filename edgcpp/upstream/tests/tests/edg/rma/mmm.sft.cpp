//options_all:-r -x -tused
//options: --strict;cp

class A { int a,b,c; };
class B : public A { int a,b; };
class C : public B { int a; };


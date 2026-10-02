//options_all:-r -x -tused
//options: --microsoft -n;cp

class A;
class B;
class C { int y;};
class A { int z;};
class B : public C, public A { public: char *r; };
char * B::* temp = &B::r;


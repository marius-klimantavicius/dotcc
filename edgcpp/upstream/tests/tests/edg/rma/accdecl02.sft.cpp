//options_all:-r -x -tused
//options: --strict;cn:;cn

class A {
  public:
    int a,b,c;
};
class B : private A {
  public:
    int x;
    A::a;
    A::b;
};
class C : private B {
  public:
    A::a;
    B::a;
    B::b; 
    A::b; 
    B::c; 
    A::c; 
    C::x;
};


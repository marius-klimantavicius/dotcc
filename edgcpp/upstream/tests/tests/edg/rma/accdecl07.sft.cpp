//options_all:-r -x -tused
//options: --strict;cn

class A { 
  public: int a,b;
};
class B : private A {
  public:
    A::a;      // a is public in B
  };
class C : private B {
  public:
    B::a;      // a is public in C ( = A::a)
    A::b;      // error
    void f(); 
};
void C::f() {
  a = 0;
  A::a = 0;
  B::a = 0;
  C::a = 0;
  b = 0;
  A::b = 0;
  B::b = 0;
  C::b = 0;
}




//options_all:-r -x -tused
//options: --strict;cn

class A {
  void f();
  void g(int);
  void g(int,int);
};
class B : public A {
  friend void A::f();
  friend void A::ff();
  friend void A::g(int);
  friend void A::g(int,int);
  friend void A::g(int,int,int);
};
class C : public A {
  friend void C::f();
  friend void C::ff();
  friend void C::g(int);
  friend void C::g(int,int);
  friend void C::g(int,int,int);
};


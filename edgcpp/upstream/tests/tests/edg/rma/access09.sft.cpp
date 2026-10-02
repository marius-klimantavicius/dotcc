//options_all:-r -x -tused
//options: --strict;cn

// Based on ARM 11.7
class X {
public:
  void xg();
protected:
  void xgg();
private:
  void xggg();
};
class W : public X {
public:
  void f();
};
class A : private virtual W {};
class B : public virtual W {};
class C : public A, public B {
  void f();
};
void C::f() {
  W::f();      // accessible via ==>B==>W
  xg();        // accessible via ==>B==>W==>X
  xgg();       // accessible via ==>B==>W==>X
  xggg();      // inaccessible member (private)
}
class D : public A {
  void f();
};
void D::f() {
  W::f();      // inaccessible member (==>A==>W)
  xg();        // inaccessible member (==>A==>W==>X)
  xgg();       // inaccessible member (==>A==>W==>X)
  xggg();      // inaccessible member (==>A==>W==>X)
}


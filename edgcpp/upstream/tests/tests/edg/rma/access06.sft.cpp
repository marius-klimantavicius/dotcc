//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a; protected: int b; private: int c; };
class B : public A { public: int x; protected: int y; private: int z; };
class C : virtual public B { void cf(); };
class D : virtual private B { void df(); };
class E : public C, public D { void ef(); };
class F : private C, private D { void ff(); };
class G : public C, private D { void gf(); };
class H : private C, public D { void hf(); };
class I : public C, virtual private B { void If(); };
class J : private C, virtual private B { void jf(); };
class K : virtual private B, public D { void kf(); };
class L : virtual private B, private D { void lf(); };
void E::ef() {
  a = 0;
  b = 0;
  c = 0;     // inaccessible member
  x = 0;
  y = 0;
  z = 0;     // inaccessible member
}
void I::If() {
  a = 0;
  b = 0;
//c = 0;     // inaccessible member
  x = 0;
  y = 0;
//z = 0;     // inaccessible member
}
void J::jf() {
  a = 0;
  b = 0;
//c = 0;     // inaccessible member
  x = 0;
  y = 0;
//z = 0;     // inaccessible member
}
void F::ff() {
  a = 0;
  b = 0;
//c = 0;     // inaccessible member
  x = 0;
  y = 0;
//z = 0;     // inaccessible member
}
void G::gf() {
  a = 0;
  b = 0;
//c = 0;     // inaccessible member
  x = 0;
  y = 0;
//z = 0;     // inaccessible member
}
void H::hf() {
  a = 0;
  b = 0;
//c = 0;     // inaccessible member
  x = 0;
  y = 0;
//z = 0;     // inaccessible member
}


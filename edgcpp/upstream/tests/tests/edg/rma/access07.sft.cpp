//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a; protected: int b; private: int c; };
class B : public A { public: int x; protected: int y; private: int z; };
class C : virtual public B {};
class D : virtual private B {};
class E : public C, virtual private B { void ef(); };
class F : virtual public B, public D { void ff(); };
// Greater access to B via the indirect derivation
void E::ef() {
  a = 0;
  b = 0;
  c = 0;     // inaccessible member
  x = 0;
  y = 0;
  z = 0;     // inaccessible member
}
// Greater access to B via the direct derivation
void F::ff() {
  a = 0;     // illegal cast to B, according to Cfront
  b = 0;     // illegal cast to B, according to Cfront
  c = 0;     // inaccessible member
  x = 0;     // illegal cast to B, according to Cfront
  y = 0;     // illegal cast to B, according to Cfront
  z = 0;     // inaccessible member
}


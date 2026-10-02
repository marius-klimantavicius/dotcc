//options_all:-r -x -tused
//options: --cfront_3.0;cn

class A { public: int a; protected: int b; private: int c; };
class B : public A { public: int x; protected: int y; private: int z; };
class C : virtual public B {};
class D : virtual private B {};
class E : virtual private B, public C { void ef(); };
class EE : private E { void eef(); };
class F : virtual public B, public D { void ff(); };
class FF : private F { void fff(); };
void E::ef() {
  a = 0;
  b = 0;
  c = 0;     // inaccessible
  x = 0;
  y = 0;
  z = 0;     // inaccessible
}
void EE::eef() {
  a = 0;
  b = 0;
  x = 0;
  y = 0;
}
void F::ff() {
  a = 0;     // cfront: illegal cast -- but ==>B==>A looks okay
  b = 0;     // cfront: illegal cast -- but ==>B==>A looks okay
  c = 0;     // inaccessible
  x = 0;     // cfront: illegal cast -- but ==>B looks okay
  y = 0;     // cfront: illegal cast -- but ==>B looks okay
  z = 0;     // inaccessible
}
void FF::fff() {
  a = 0;     // cfront: illegal cast -- but ==>F==>B==>A looks okay
  b = 0;     // cfront: illegal cast -- but ==>F==>B==>A looks okay
  x = 0;     // cfront: illegal cast -- but ==>F==>B looks okay
  y = 0;     // cfront: illegal cast -- but ==>F==>B looks okay
}


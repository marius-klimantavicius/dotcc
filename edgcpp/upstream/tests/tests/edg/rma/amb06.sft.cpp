//options_all:-r -x -tused
//options: --strict;cn

class A { public: int i; };
class B : public A {};
class C : public A {};
class D : public B, public C { void f(); };
void D::f() {
  i = 0;      // ambiguous
  A::i = 0;   // ambiguous
  B::i = 0;
  C::i = 0;
  D::i = 0;   // ambiguous
}


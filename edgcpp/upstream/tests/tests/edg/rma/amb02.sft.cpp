//options_all:-r -x -tused
//options: --strict;cp

struct A { int i; };
struct B : public virtual A {};
struct C : public virtual A {};
struct D : public B, public C { void f(); };
void D::f() {
  A::i = 0;
  B::i = 0;
  C::i = 0;
  D::i = 0;
}
struct E : public D { void f(); };
void E::f() {
  A::i = 0;
  B::i = 0;
  C::i = 0;
  D::i = 0;
  E::i = 0;
}


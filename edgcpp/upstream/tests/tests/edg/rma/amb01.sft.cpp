//options_all:-r -x -tused
//options: --strict;cn

struct A { int i; };
struct B { int i; };
struct C : public A, public B {};
struct D : public C  { void f(); };
void D::f() {
  C::i = 0;   // A::i or B::I ???
  D::i = 0;   // A::i or B::I ???
  i = 0;      // A::i or B::I ???
}
  


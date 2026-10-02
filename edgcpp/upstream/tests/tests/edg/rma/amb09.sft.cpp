//options_all:-r -x -tused
//options: --strict;cn

// Ambiguity checking
class A {public: int i;};
class B : public A {public: int j;};
class X : public B {};
class Y : public B {};
class C : public X, public Y {};
class D : public C {void f();};
void D::f() {
  A::i = 0;
  B::i = 0;
  X::i = 0;
  Y::i = 0;
  C::i = 0;
  D::i = 0;
  i = 0;
}


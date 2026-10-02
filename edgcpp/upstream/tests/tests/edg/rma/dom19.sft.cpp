//options_all:-r -x -tused
//options: --strict;cn

// Ambiguity, dominance
//   B   A   C
//    \ / \ /
//     X   Y
//      \ /
//       D
class A { public: int i, j, k; };
class B { public: int i, j; };
class C { public: int j, k; };
class X : virtual public A, public B { public: int i; };
class Y : public B, virtual public A { public: int j; };
class D : public X, public Y { void f(); };
void D::f() {
  A::i = 0;
  A::j = 0;
  A::k = 0;
  B::i = 0;  // error
  B::j = 0;  // error
  C::j = 0;  // error
  C::k = 0;  // error
  X::i = 0;
  X::j = 0;  // error
  X::k = 0;
  Y::i = 0;  // error
  Y::j = 0;
  Y::k = 0;
  D::i = 0;   // error
  D::j = 0;   // error
  D::k = 0;
  i = 0;  // error
  j = 0;  // error
  k = 0;
}


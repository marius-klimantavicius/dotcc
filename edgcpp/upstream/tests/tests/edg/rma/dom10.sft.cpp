//options_all:-r -x -tused
//options: --strict;cn

//   B   A   C
//    \ / \ /
//     X   Y
//      \ /
//       D
class A { public: int i, j, k; };
class B { public: int i, j; };
class C { public: int j, k; };
class X : virtual public A, public B { public: int i; };
class Y : public C, virtual public A { public: int k; };
class D : public X, public Y { void f(); };
void D::f() {
  A::i = 0;   // okay
  A::j = 0;   // okay
  A::k = 0;   // okay
  B::i = 0;   // okay
  B::j = 0;   // okay
  C::j = 0;   // okay
  C::k = 0;   // okay
  X::i = 0;   // okay
  X::j = 0;   // ambiguous
  X::k = 0;   // okay
  Y::i = 0;   // okay
  Y::j = 0;   // ambiguous
  Y::k = 0;   // okay
  D::i = 0;   // should be okay: X::i dominates A::i and Y::i and hides B::i
  D::j = 0;   // ambiguous
  D::k = 0;   // should be okay for similar reasons
  i = 0;      // okay
  j = 0;      // ambiguous
  k = 0;      // okay
}


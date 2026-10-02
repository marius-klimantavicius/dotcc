//options_all:-r -x -tused
//options: --strict;cn

//   B   A   B 
//    \ / \ /
//     X   Y
//      \ /
//       C
class A { public: int i, j, k; };
class B { public: int i, j; };
class X : virtual public A, public B { public: int i; };
class Y : public B, virtual public A { public: int j; };
class C : public X, public Y { void f(); };

int f() {
  C c;
  return c.j;
}

void C::f() {
  A::i = 0;   // okay
  A::j = 0;   // okay
  A::k = 0;   // okay
  B::i = 0;   // ambiguous
  B::j = 0;   // ambiguous
  X::i = 0;   // okay
  X::j = 0;   // ambiguous
  X::k = 0;   // okay
  Y::i = 0;   // ambiguous
  Y::j = 0;   // okay
  Y::k = 0;   // okay
  C::i = 0;   // ambiguous
  C::j = 0;   // ambiguous
  C::k = 0;   // okay
  i = 0;      // ambiguous
  j = 0;      // ambiguous
  k = 0;      // okay
}



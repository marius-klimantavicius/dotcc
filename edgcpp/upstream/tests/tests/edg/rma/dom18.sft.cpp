//options_all:-r -x -tused
//options: --strict;cn

// Ambiguity
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
void C::f() {
  A::i = 0;
  A::j = 0;
  A::k = 0;
  B::i = 0; // error
  B::j = 0; // error
  X::i = 0;
  X::j = 0; // error
  X::k = 0;
  Y::i = 0; // error
  Y::j = 0;
  Y::k = 0;
  C::i = 0; // error
  C::j = 0; // error
  C::k = 0;
  i = 0; // error
  j = 0; // error
  k = 0;
}


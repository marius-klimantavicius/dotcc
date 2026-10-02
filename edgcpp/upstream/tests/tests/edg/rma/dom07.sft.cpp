//options_all:-r -x -tused
//options: --strict;cn

// This test checks for ambiguity in the presence of dominance
//    A   B   C   D
//     \ / \ / \ /
//      X   Y   Z
//       \  |  /
//        \ | /
//         \|/
//          S 
struct A1 { int i; };
struct B1 { int i; };
struct C1 { int i; };
struct D1 { int i; };
struct X1 : public virtual A1, public virtual B1 { int i; };
struct Y1 : public virtual B1, public virtual C1 {};
struct Z1 : public virtual C1, public virtual D1 {};
struct S1 : public X1, public Y1, public Z1 { void f(); };
void S1::f() {
  A1::i = 0;   // okay
  B1::i = 0;   // okay
  C1::i = 0;   // okay
  D1::i = 0;   // okay
  X1::i = 0;   // okay
  Y1::i = 0;   // ambiguous (B::i or C::i ??)
  Z1::i = 0;   // ambiguous (C::i or D::i ??)
  S1::i = 0;   // ambiguous (X::i or C::i or D::i ??)
  i = 0;       // ambiguous (ditto)
}


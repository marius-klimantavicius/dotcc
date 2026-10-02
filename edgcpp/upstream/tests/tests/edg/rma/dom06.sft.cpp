//options_all:-r -x -tused
//options: --strict;cp

// This test checks for inappropriate order sensitivity in processing
// projection symbols.  The first and second versions of the test are
// identical but for the order in which the base classes for X are seen.
//   B   A
//    \ / \
//     X   Y
//      \ /
//       C
class A1 { public: int i, j; };
class B1 { public: int i; };
class X1 : virtual public A1, public B1 { public: int i, j; };
class Y1 : virtual public A1 {};
class C1 : public X1, public Y1 { void f(); };
void C1::f() {
  C1::i = 0;   // should be okay: X1::i hides B1::i; dominates A1::i, Y1::i
  i = 0;       // should be okay
  C1::j = 0;   // should be okay: X1::j dominates A1::j, Y1::j 
  j = 0;       // should be okay
}

class A2 { public: int i, j; };
class B2 { public: int i; };
class X2 : public B2, virtual public A2 { public: int i, j; };
class Y2 : virtual public A2 {};
class C2 : public X2, public Y2 { void f(); };
void C2::f() {
  C2::i = 0;   // should be okay: X2::i hides B2::i; dominates A2::i, Y2::i
  i = 0;       // should be okay
  C2::j = 0;   // should be okay: X2::j dominates A2::j, Y2::j 
  j = 0;       // should be okay
}


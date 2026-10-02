//options_all:-r -x -tused
//options: --strict;cp

//       V1
//     _/||\
//    / / | \
//   A  B / |
//    \ |/  |
//     V2   |
//    / |   |
//   C  D   |
//    \ |   |
//      E   /
//       \ /
//        F
//
struct V1 { int i; };
struct A : public virtual V1 { float i; };
struct B : public virtual V1 {};
struct V2 : public A, public B, public virtual V1 { void f(); };
struct C : public virtual V2 { double i; };
struct D : public virtual V2 {};
struct E : public C, public D { void f(); };
struct F : public E, public virtual V1 { void f(); };
void V2::f() {
  i = 0;     // should be A::i (float)
}
void E::f() {
  B::i = 0;  // should be V1::i (int)
  D::i = 0;  // should be A::i (float)
  i = 0;     // should be C::i (double)
}
void F::f() {
  i = 0;     // C::i (double) or ambiguous? (does C::i dominate V1::i?)
}


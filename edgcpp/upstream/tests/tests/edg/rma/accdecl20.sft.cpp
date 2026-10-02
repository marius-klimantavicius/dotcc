//options_all:-r -x -tused
//options: --strict;fn

//    V   A
//   / \ /
//  B   C
//   \ /
//    D
//
struct V {
  int i;
  int j;
};
struct A {
  int a;
  int b;
};
struct C : public virtual V, public A {
  int i;    // hides V::i
  int a;    // hides A::a
};
struct B : public virtual V {
  int j;    // hides V::j
  int b;    // conflicts with A::b
};
struct D : private B, private C {
public:
  V::i;
  V::j;
  A::a;
  A::b;
};


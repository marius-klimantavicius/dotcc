//options_all:--strict
//type:cp

//     V
//    / \
//   A   B
//    \ /
//     C
struct V { int i; };
struct A : public virtual V { int i; };
struct B : public virtual V { };
struct C : public A, private B {
  B::i;
};

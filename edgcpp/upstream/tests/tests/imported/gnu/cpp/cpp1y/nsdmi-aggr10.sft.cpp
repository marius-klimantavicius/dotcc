//type: fp
//options: --c++14
// PR c++/71638
// { dg-do compile { target c++14 } }

struct {
  int &&a;
  int b{a};
} c[] { { 2 } };

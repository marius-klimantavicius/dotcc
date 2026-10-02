//type: fp
//options: --c++11
// PR c++/58255
// { dg-do compile { target c++11 } }

struct A {
  explicit A() { }
  A(int x) : A() { }
};

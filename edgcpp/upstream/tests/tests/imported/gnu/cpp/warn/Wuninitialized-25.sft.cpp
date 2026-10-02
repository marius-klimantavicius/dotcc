//type: fp
//options: --c++11
// PR c++/19808
// { dg-do compile { target c++11 } }
// { dg-options "-Wall" }

struct A {
  A *a;
};

struct B : A {
  int i;
  B() : A{a} {}
};

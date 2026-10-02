//type: fp
//options: --c++11
// PR c++/66654
// { dg-do compile { target c++11 } }

class A {
  A();
};
class B {
  A r{r};
};

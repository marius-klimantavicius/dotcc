//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M:A }

module M:A;

template <typename T> struct A {
  template <typename U> friend struct B;
private:
  int x = 42;
};

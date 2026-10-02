//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;

export extern "C++" template <typename> struct A {
  template <typename> friend struct B;
};

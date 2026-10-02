//type: fp
//options:  --c++20 --modules
// PR c++/112899
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;

export struct A {
  static constexpr int x = -1;
};

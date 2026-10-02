//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi bla }

export module bla;

export extern "C++" inline void fun() {
  void oops();  // { dg-bogus "block-scope extern declaration" }
  oops();
}

//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi A }

export module A;

extern "C++" int foo ();
extern "C++" char bar ();

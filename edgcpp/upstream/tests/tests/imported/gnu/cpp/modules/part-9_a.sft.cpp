//type: fp
//options:  --c++20 --modules
// PR c++/114950
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M:a }

module M:a;

struct A {};
extern "C++" struct B {};
void f(int) {}
extern "C++" void f(double) {}

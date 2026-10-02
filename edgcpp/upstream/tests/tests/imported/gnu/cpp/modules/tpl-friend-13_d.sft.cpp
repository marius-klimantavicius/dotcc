//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi X }

export module X;
export import M;
A<int> ax;
B<int> bx;

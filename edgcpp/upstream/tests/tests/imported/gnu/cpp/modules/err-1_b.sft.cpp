//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Bar;
// { dg-module-cmi "Bar" }

export int Frob (long, int, int);

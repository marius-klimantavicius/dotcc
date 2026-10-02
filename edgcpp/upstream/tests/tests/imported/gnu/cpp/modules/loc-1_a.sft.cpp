//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module bob;
// { dg-module-cmi bob }

export int frob (int *); // line 5

//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Baz;
// { dg-module-cmi "Baz" }

export void Quux (int, int);

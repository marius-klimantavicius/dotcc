//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

module M;

decltype(f()) g() { return {}; }

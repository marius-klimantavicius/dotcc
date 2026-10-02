//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
module;
module; // { dg-error "expected" }

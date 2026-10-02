//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
int j;
module; // { dg-error "only permitted as" }

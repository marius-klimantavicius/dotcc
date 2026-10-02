//type: fn
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

int k;

module :private; // { dg-error "private module fragment" }
int i;

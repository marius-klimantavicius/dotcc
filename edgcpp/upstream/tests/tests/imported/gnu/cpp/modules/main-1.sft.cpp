//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module M;
int main() {}  // { dg-error "attach" }

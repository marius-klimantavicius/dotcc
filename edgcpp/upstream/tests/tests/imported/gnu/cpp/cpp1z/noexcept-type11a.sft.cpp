//type: fp
//options:  --c++14
// { dg-options "-Wall -Wno-noexcept-type -std=c++14" }

void f(int(*)() noexcept) { }

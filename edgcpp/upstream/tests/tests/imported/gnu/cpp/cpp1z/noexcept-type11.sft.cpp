//type: fp
//options:  --c++14
// { dg-options "-Wall -std=c++14" }

void f(int(*)() noexcept) { }	// { dg-warning "mangled" }

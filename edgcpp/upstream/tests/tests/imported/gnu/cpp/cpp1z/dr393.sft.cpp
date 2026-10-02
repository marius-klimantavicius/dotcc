//type: fp
//options:  --c++14
// DR 393
// { dg-options -Wpedantic }

void f(int (&)[]);  // { dg-warning "unknown bound" "" { target c++14_down } }

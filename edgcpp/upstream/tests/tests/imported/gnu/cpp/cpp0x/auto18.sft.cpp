//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

void f()
{
  auto val = val;  // { dg-error "auto" }
}

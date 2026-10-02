//type: fn
//options: --c++20
// { dg-do compile { target concepts } }

const auto &f(auto)
{
  static int n;
  return n;
}

void foo()
{
  f(5) = 0; // { dg-error "read-only" }
}

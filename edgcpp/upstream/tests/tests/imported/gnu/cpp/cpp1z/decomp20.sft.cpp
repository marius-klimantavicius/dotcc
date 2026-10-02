//type: fp
//options: --c++17
// { dg-do compile { target c++17 } }

struct A { int i,j; };

A f();

int main()
{
  auto [i,j] (f());
}

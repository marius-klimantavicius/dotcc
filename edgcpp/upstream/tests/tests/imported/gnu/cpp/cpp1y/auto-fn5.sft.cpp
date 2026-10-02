//type: rp
//options: --c++14
// { dg-do run { target c++14 } }

int i;
auto& f() { return i; }

int main()
{
  f() = 42;
  return i != 42;
}

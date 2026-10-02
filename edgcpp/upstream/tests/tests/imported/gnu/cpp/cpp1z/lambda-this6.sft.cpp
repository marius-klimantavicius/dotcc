//type: fp
//options: --c++17
// { dg-do compile { target c++17 } }

struct S
{
  int a = [this] { return 6; } ();
};

S
foo()
{
  return {};
}

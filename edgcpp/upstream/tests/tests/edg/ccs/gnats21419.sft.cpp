//type:cp
//options:--c++17;fn:--c++20

struct A {
  int x;
};

int f()
{
  (void)A(29);
  A a(29);
  return a.x;
}

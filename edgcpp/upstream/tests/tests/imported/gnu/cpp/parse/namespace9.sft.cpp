//type: fp
//options: 
namespace A {
  void f();
}
void g()
{
  struct f { };
  using A::f;
}

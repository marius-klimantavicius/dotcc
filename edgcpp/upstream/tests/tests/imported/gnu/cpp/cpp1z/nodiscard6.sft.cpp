//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

struct A
{
  [[nodiscard]] A();
};

void foo()
{
  A();  // { dg-warning "ignoring return value" }
}

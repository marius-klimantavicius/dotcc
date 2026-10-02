//type:fp
//options:--c++11

namespace multiple_base_classes
{
  struct B1
  { };

  struct B2
  { };

  struct D : B1, B2
  { };

  struct E : D
  { };
}

namespace virtual_base_class
{
  struct Base { };
  struct MiddleA : virtual public Base { };
  struct MiddleB : virtual public Base { };
  struct Derived : public MiddleA, public MiddleB { };
}

namespace another_virtual_base_class
{
  struct B { };
  struct C : B { };
  struct D : virtual C { };
  struct E : D { };
}

namespace call_base_class_function
{
  struct A
  {
    void bar();
  };

  struct B : A
  { };

  struct C : B
  {
    void f()
    {
      bar();
    }
  };
}

namespace pointer_to_member_conv_virtual_base
{
  struct A {int i; };
  struct AA : public A { };
  struct V { };
  struct B : public AA, virtual public V {};

  int A::*p = &A::i;
  int B::*q = p;
}

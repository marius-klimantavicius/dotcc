//type:fn
//options:--c++11 --microsoft_version 1700:--ms_c++20 --microsoft_version 1942:--c++20 --gn 140200:--c++20 --clang_version 190200

namespace private_in_base_overloaded_with_using
{
  class A
  {
    static int f();
    static int f(int);
  };

  class B : A
  {
    using A::f;
  };

  struct D : B
  {
    using B::f;
  };

  int i = D::f() + D::f(1);
}

namespace private_in_base_no_overload_with_using
{
  class A
  {
    static int f();
  };

  class B : A
  {
    using A::f;
  };

  struct D : B
  {
    using B::f;
  };

  int i = D::f();
}

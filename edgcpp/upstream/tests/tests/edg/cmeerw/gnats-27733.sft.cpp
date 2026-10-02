//type:fp
//options:--c++11 --microsoft_version 1700:--ms_c++20 --microsoft_version 1942:--c++20 --gn 140200;fn:--c++20 --clang_version 190200;fn

namespace minimal
{
  class A {
    void f();
    void f(int);
  };
  struct B : A { };
  struct D : B {
    using B::f;
  };
}

namespace overloaded
{
  class A
  {
    static int f();
    static int f(int);
  };

  struct B : A
  { };

  struct D : B
  {
    using B::f;
  };

  int i = D::f() + D::f(1);
}

namespace no_overload
{
  class A
  {
    static int f();
  };

  struct B : A
  { };

  struct D : B
  {
    using B::f;
  };

  int i = D::f();
}

namespace overloaded_with_using
{
  class A
  {
  protected:
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

namespace no_overload_with_using
{
  class A
  {
  protected:
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

namespace rmpl_overloaded
{
  class A
  {
    template<typename T = void>
    static int f();
    template<typename T = void>
    static int f(int);
  };

  struct B : A
  { };

  struct D : B
  {
    using B::f;
  };

  int i = D::f() + D::f(1);
}

namespace tmpl_no_overload
{
  class A
  {
    template<typename T = void>
    static int f();
  };

  struct B : A
  { };

  struct D : B
  {
    using B::f;
  };

  int i = D::f();
}

namespace tmpl_overloaded_with_using
{
  class A
  {
  protected:
    template<typename T = void>
    static int f();
    template<typename T = void>
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

namespace tmpl_no_overload_with_using
{
  class A
  {
  protected:
    template<typename T = void>
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

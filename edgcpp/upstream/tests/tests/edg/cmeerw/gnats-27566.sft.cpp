//type:fn
//options:--c++11:--c++14:--c++17:--c++20

namespace minimal
{
  template<int, typename>
  class A {
    A(int);
  };
  template<typename T>
  struct B : A<0, T> {
    using A<0, T>::A;
  };
  B<int> b(1);
}

namespace tmpl_cls
{
  template<int I, class U>
  struct A {
    static int f();

  protected:
    A(int);
  };

  template<class T>
  struct B : A<0, T> {
    using Base = A<0, T>;
    using Base::Base;

  protected:
    using Base::f;
  };

  template<class T>
  struct D : B<T> {
    using Base = B<T>;
    using Base::Base;
  };

  B<double> b(1);
  D<float> d(1);

  int i = B<double>::f() + D<float>::f();
}

namespace tmpl_fn
{
  template<int I, class U>
  struct A {
    template<typename T = void>
    static int f();

  protected:
    template<typename T>
    A(T);
  };

  template<class T>
  struct B : A<0, T> {
    using Base = A<0, T>;
    using Base::Base;

  protected:
    using Base::f;
  };

  template<class T>
  struct D : B<T> {
    using Base = B<T>;
    using Base::Base;
  };

  B<double> b(1);
  D<float> d(1);

  int i = B<double>::f() + D<float>::f();
}

namespace non_tmpl
{
  struct A {
    static int f();

  protected:
    A(int);
  };

  struct B : A {
    using A::A;

  protected:
    using A::f;
  };

  struct D : B {
    using B::B;
  };

  B b(1);
  D d(1);

  int i = B::f() + D::f();
}

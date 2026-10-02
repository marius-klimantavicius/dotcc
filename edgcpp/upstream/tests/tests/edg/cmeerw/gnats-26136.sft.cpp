//type:fp
//options:--c++17:--c++17 --g++:--ms_c++20

namespace minimal
{
  struct A {
    ~A();
  };
  struct B {
    B(A = A());
  };
  template<typename T>
  struct C : B {
    using B::B;
  };
  template<unsigned> struct X {};
  X<sizeof(C<int>)> x;
}

namespace dependent_base
{
  struct A
  {
    ~A();
  };

  struct B
  {
    B(A = A());
  };

  template<typename T>
  struct C : T
  {
    using B::B;
  };

  template<unsigned> struct X {};

  X<sizeof(C<B>)> x;
}

namespace inh_ctor_in_templ_arg
{
  struct A
  {
    ~A();
  };

  struct B
  {
    B(A = A());
  };

  template<typename T>
  struct C : T
  {
    using B::B;
    static constexpr bool v = true;
  };

  template<bool> struct X {};

  X<C<B>::v> x;
}

namespace inh_ctor_after_nested_class
{
  struct A
  {
    ~A();
  };

  struct B
  {
    B(A = A());
  };

  struct C : B
  {
    struct D
    {
      D(int = 1);
    };

    using B::B;
  };
}

namespace pending_class_definition
{
  struct A
  {
    ~A();
  };

  struct B
  {
    B(A = A());
  };

  template<typename T>
  struct C : B
  {
    using B::B;
  };

  template<bool> struct X {};

  template<typename T>
  struct Y : X<__is_constructible(C<T>)>
  { };

  Y<int> y;
}

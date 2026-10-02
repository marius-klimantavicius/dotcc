//type:fp
//options:--c++20 -A:--c++20 --gn 130100:--ms_c++20 --microsoft_version 1926:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T> struct A {
    static constexpr T f() { return true; }
    static int g() requires (f());
  };
  int i = A<bool>::g();
}

namespace fn_requires_unqualified_function
{
  template<typename T> struct A
  {
    static constexpr T f()
    {
      return true;
    }

    static int g() requires (f());
  };

  int i = A<bool>::g();
}

namespace ctor_requires_unqualified_function
{
  template<typename T> struct A
  {
    static constexpr T f()
    {
      return true;
    }

    A(T) requires (f())
    { }
  };

  A a(true);
}

namespace explicit_unqualified_function
{
  template<typename T> struct A
  {
    static constexpr T f()
    {
      return false;
    }

    explicit (f())
    A(T)
    { }
  };

  A a(true);
}

namespace unqualified_variable
{
  template<typename T> struct A
  {
    static constexpr T v = false;

    explicit(v)
    A(T)
    { }
  };

  A a(true);
}

namespace qualified_function
{
  template<typename T> struct A
  {
    static constexpr T f()
    {
      return false;
    }

    explicit (A::f())
    A(T)
    { }
  };

  A a(true);
}

namespace qualified_function_with_template_args
{
  template<typename T> struct A
  {
    static constexpr T f()
    {
      return false;
    }

    explicit (A<T>::f())
    A(T)
    { }
  };

  A a(true);
}

namespace namespace_function
{
  static constexpr bool f()
  {
    return false;
  }

  template<typename T> struct A
  {
    explicit (f())
    A(T)
    { }
  };

  A a(true);
}

namespace qualified_function_in_different_class
{
  struct B
  {
    static constexpr bool f()
    {
      return false;
    }
  };

  template<typename T> struct A
  {
    explicit (B::f())
    A(T)
    { }
  };

  A a(true);
}

namespace qualified_function_in_different_template_class
{
  template<typename T>
  struct B
  {
    static constexpr T f()
    {
      return false;
    }
  };

  template<typename T> struct A
  {
    explicit (B<T>::f())
    A(T)
    { }
  };

  A a(true);
}

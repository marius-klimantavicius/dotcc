//type:fp
//options:--c++ --gn 140200 --no_defer_parse_function_templates:--c++20:--c++20 --no_defer_parse_function_templates:--ms_c++20 --microsoft_version 1936 --no_defer_parse_function_templates
//options_all:-w

namespace explicit_spec
{
  template<typename T> struct B;
  template<> struct B<void> {
    void f();
  };
  template<typename T> struct B : B<void>, T {
    using B<void>::f;
    void g() {
      B::f();
    }
  };
}

namespace namespaced_class
{
  namespace ns {
    struct B {
      void f();
    };
  }
  template<typename T> struct B : ns::B, T {
    using ns::B::f;
    void g(B b) {
      B::f();
    }
  };
}

namespace gpp_mode
{
  template<typename T>
  struct C
  { };

  template<typename T>
  struct B;

  template<>
  struct B<void>
  {
    void f();
  };

  template<typename T>
  struct B
    : B<void>, C<T>
  {
    using B<void>::f;

    void g()
    {
      B b;
      B<void> bv;

      B::f();
      B<void>::f();
    }
  };
}

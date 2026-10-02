//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944

namespace minimal {
  struct A {
    static constexpr bool v = false;
  };
  template<typename T> struct B {
    void g(int) noexcept(A::v);
  };
  struct D : B<bool> {
    void f() {
      g(1);
    }
  };
}

namespace tmpl_base
{
  struct A
  {
    static constexpr bool v = false;
  };

  template<typename T> struct B
  {
    void g(int) noexcept(A::v);
  };

  struct D : B<bool>
  {
    void f()
    {
      g(1);
    }
  };
}

namespace non_tmpl_base
{
  struct A
  {
    static constexpr bool v = false;
  };

  struct B
  {
    void g(int) noexcept(A::v);
  };

  struct D : B
  {
    void f()
    {
      g(1);
    }
  };
}

//type:fp
//options:--c++20
//options_all:-w

#include <typeinfo>

namespace non_tmpl
{
  struct A
  {
    virtual ~A();
  };

  void f(const A a)
  {
    constexpr auto v = &typeid(a);
  }

  const A a;
  auto p = &typeid(a);
}

namespace tmpl
{
  template<typename T>
  struct A
  {
    virtual ~A();
  };

  template<typename T>
  void f(T const t)
  {
    constexpr auto v = &typeid(t);
  }

  template void f(tmpl::A<int>);
}

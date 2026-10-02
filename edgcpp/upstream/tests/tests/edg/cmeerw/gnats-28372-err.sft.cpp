//type:fn
//options:--c++20 --clang_version 210100

namespace ambiguous
{
  int f(int);
  int f(long);

  int i = __builtin_invoke(f, 1);
}

namespace type_cls
{
  struct C
  { };

  C c;
  int i = __builtin_invoke(c, 1);
}

namespace type_int
{
  int i = __builtin_invoke(1, 2);
}

namespace type_obj_pointer
{
  int i = __builtin_invoke("", 2);
}

namespace failed_substitution
{
  template<typename T>
  auto f(T t) -> decltype(__builtin_invoke(t));

  auto v = f(0);
}

namespace failed_substitution_mbr_pointer
{
  struct C
  {
    int f() const;
  };

  template<typename T>
  auto f(T t) -> decltype(__builtin_invoke(&C::f, t));

  auto v = f(0);
}

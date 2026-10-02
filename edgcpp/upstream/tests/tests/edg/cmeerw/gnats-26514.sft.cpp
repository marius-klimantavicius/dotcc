//type:fp
//options:--ms_c++20  --microsoft_version 1934:--c++20;fn:--c++20 --gn 120100;fn:--c++20 --clang_version 160000;fn

namespace minimal
{
  template<typename T, typename U>
  void f(T *, U);
  template<typename T, typename U>
  char f(T, U *) requires true;
  char c = f("", "");
}

namespace template_constraint
{
  template<typename T, typename U>
  void f(T *, U);
  template<typename T, typename U> requires true
  char f(T, U *);

  char c = f("", "");
}

namespace int_param
{
  template<typename T>
  void f(T, int);
  template<typename T> requires true
  char f(int, T);

  char c = f(1, 2);
}

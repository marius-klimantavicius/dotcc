//type:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<typename T> requires true
  int f(T);         // #1
  template<typename T, typename ... U> requires true
  int f(T, U ...);  // #2
  int i = f(1);
}

namespace trailing_requires
{
  template<typename T, typename ... U>
  int f(T, U ...) requires true;
  template<typename T>
  int f(T) requires true;
  int i = f(1);
}

namespace more_constrained
{
  template<typename T, typename ... U>
  int f(T, U ...) requires true = delete;

  template<typename T>
  int f(T);

  int i = f(1);
}

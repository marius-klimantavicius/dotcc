//type:fp
//options:--c++11:--c++17:--c++20:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename T> using A = int;
  template<typename T, T> struct C;
  template<typename R, typename ... Args, R fn(int, Args...)>
  struct C<R(*)(A<R>, Args...), fn> { };
  int f(int, int);
  C<int(*)(int, int), &f> c;
}

namespace alias_template
{
  template<typename T>
  using A = int;

  template<typename T, T>
  struct C;

  template<typename R, typename ... Args, R fn(int, Args...)>
  struct C<R(*)(A<R>, Args...), fn>
  { };

  int f(int, int);

  C<decltype(&f), &f> c;
}

//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<int>
  struct S {
    char c;
  };
  template<typename T, S s> struct N;
  template<typename T, S s> struct N<T *, s> { };
  N<int *, S<0>{'a'}> n;
}

namespace non_placeholder
{
  template<int N>
  struct S
  {
    char c;
  };

  template<typename T, S<0>> struct N;

  template<typename T, S<0> s> struct N<T *, s> { };

  N<int *, S<0>{'a'}> n;
}

namespace constrained
{
  template<int N>
  struct S
  {
    char c;
  };

  template<S> struct N;

  template<S s> requires true struct N<s> { };

  N<S<0>{'a'}> n;
}

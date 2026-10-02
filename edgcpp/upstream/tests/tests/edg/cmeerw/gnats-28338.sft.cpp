//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename T, bool = requires { T::v; }>
  struct A : A<T> { };
}

namespace simple
{
  template<typename T, bool = requires { T::v; }>
  struct A : A<T>
  { };

  template<typename T>
  struct A<T, true>
  { };

  struct C
  {
    static int v;
  };

  A<C> a;
  A<C, false> af;
}

namespace nested
{
  template<typename U>
  struct O
  {
    template<typename T, bool = requires { T::v; }>
    struct A : A<T> {};

    template<typename T>
    struct A<T, true>
    { };
  };

  struct C
  {
    static int v;
  };

  O<void>::A<C> a;
  O<void>::A<C, false> af;
}

//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename T> using A = T;
  template<typename T> struct C;
  template<typename T>
  struct C<T *> {
    template<typename U>
    A<C> (f)(int);
  };
}

namespace template_member
{
  template<typename T>
  using A = T;

  template<typename T>
  struct C {
    template<typename U>
    A<C> (f)(int);
  };
}

namespace partial_spec_member
{
  template<typename T>
  using A = T;

  template<typename T>
  struct C;

  template<typename T>
  struct C<T *> {
    template<typename U>
    A<C> (f)(int);
  };
}

namespace explicit_spec_member
{
  template<typename T>
  using A = T;

  template<typename T>
  struct C;

  template<>
  struct C<int *> {
    template<typename U>
    A<C> (f)(int);
  };
}

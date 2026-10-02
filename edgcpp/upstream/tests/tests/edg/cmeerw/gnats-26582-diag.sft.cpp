//type:fn
//options:--ms_c++20 --microsoft_version 1936:--c++14 --clang_version 30901:--c++17 --gn 110400

namespace specialized_arg_dpdt_type
{
  template<typename T, T>
  struct C
  { };

  template<typename T>
  struct C<T, 1>;               // error

  C<int, 1> c;
}

namespace specialized_arg_dpdt_instantiated_type
{
  template<class T, class U, U u>
  struct C
  { };

  template<class T>
  struct C<T, T, 1>;            // error

  C<int, int, 1> c;
}

namespace specialized_arg_dpdt_instantiated_decltype_type
{
  template<class T, class U, decltype(U()) u>
  struct C
  { };

  template<class T>
  struct C<T, T, 1>;            // error (GCC and MSVC don't diagnose)

  C<int, int, 1> c;
}

namespace dpdt_arg
{
  template<typename, int>
  struct C
  { };

  template<typename T>
  struct C<T, sizeof(T)>;       // error

  C<int, sizeof(int)> c;
}

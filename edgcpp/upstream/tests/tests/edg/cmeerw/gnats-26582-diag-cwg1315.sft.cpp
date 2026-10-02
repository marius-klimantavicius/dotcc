//type:fn
//options:--c++11:--c++11 --gn 130100:--c++11 --clang_version 160000:--ms_c++20 --microsoft_version 1936

namespace error_dpdt_type
{
  template<typename T, T t>
  struct C
  { };

  template<typename T>
  struct C<T, 1>;               // error

  C<int, 1> c;
}

namespace error_dpdt_decltype
{
  template<typename T, decltype(T()) t>
  struct C
  { };

  template<typename T>
  struct C<T, 1>;               // error (but gcc and MSVC won't diagnose)

  C<int, 1> c;
}

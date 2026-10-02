//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename, int, auto>
  concept C = true;
  template<C<1, 2>>
  struct A;
}

namespace use_first_param
{
  template<typename T, typename U, T>
  concept C = true;

  template<C<long, 2> T>
  struct A;
}

namespace use_second_param
{
  template<typename T, typename U, U>
  concept C = true;

  template<C<long, 2> T>
  struct A;
}

//type:fp
//options:--c++11:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190200:--ms_c++20 --microsoft_version 1942

namespace minimal {
  template<typename T>
  struct D {
    using type = T;
  };
  template<typename T, typename U, typename D<T>::type>
  struct C;
  template<typename U>
  struct C<int, U, 0>;
}

namespace non_dpdt_type
{
  template<typename T>
  struct D
  {
    using type = T;
  };

  template<typename T, typename U, typename D<T>::type>
  struct C;

  template<typename U>
  struct C<int, U, 0>;
}

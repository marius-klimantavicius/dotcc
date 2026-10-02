//type:fn
//options:--c++11:--c++20

namespace dependent_type
{
  template<typename T>
  struct D
  {
    using type = T;
  };

  template<typename T, typename U, typename D<U>::type>
  struct C;

  template<typename U>
  struct C<int, U, 0>;
}

namespace failed_substitution
{
  template<typename T>
  struct D
  { };

  template<typename T, typename U, typename D<T>::type>
  struct C;

  template<typename U>
  struct C<int, U, 0>;
}

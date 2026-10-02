//type:fp
//options:--c++11 --g++

namespace integral_value
{
  template<int I> struct C { };
  template<int I> C<I ?: 0> f();
  decltype(f<2>()) c = C<2>{};
}

namespace minimal
{
  enum E { E0, E1 };
  template<E e> struct C { };
  template<E e> C<e ?: E0> f();
  using type = decltype(f<E1>());
}

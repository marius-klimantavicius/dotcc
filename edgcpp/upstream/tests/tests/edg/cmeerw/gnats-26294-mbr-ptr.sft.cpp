//type:fp
//options:--c++20

namespace templated
{
  template<class T> using B = T;
  template<class T, class U> using C = void (B<B<U>>::*)();
}

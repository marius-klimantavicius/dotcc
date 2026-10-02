//type:fp
//options:--c++11:--c++20
//options_all:-tused

namespace decltype_needed
{
  template<typename>
  struct C
  { };

  template<unsigned>
  struct D
  { };

  template<typename T>
  inline int f(T)
  {
    return T::invalid;
  }

  C<decltype(f(1))> *p1;
  D<sizeof(f('a'))> *p2;
  D<noexcept(f('a'))> *p4;
}

//type:fn
//options:--c++20:--ms_c++20 --microsoft_version=1927

namespace partial_deduce_return_result
{
  template<typename T1, typename T2>
  struct C
  {
    C(...);
  };

  template<typename T, typename U>
  C(T, U) -> C<T, U *>;

  template<typename U>
  using A = C<int, U>;

  C c(1, "");
  A ai(1, "");
  A al(1L, "");                 // deduction fails
}

//type:fn
//options:--c++20 --gn 150100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1942

template<bool>
struct A
{ };

template<typename T>
struct B
{
  template<int I>
  struct N
  {
    static_assert(I == 0);
    static constexpr bool v = true;
  };

  struct N1 : N<1> { };
  struct N2 : N<2> { };
  struct N3 : N<3> { };
};

struct C
{
  template<typename T>
  explicit(T::N1::v) C(T, A<T::N2::v>) requires T::N3::v;
};

C c(B<int>{}, A<true>{});

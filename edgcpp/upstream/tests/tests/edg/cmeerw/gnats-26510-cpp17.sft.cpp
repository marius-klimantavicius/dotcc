//type:fp
//options:--c++17 --gn 150100:--c++17 --clang_version 200100

struct C
{
  template<typename T>
  constexpr explicit(sizeof(T) != 0) C(T)
    : i(1)
  { }

  constexpr C(long)
    : i(2)
  { }

  int i;
};

constexpr C c1(0);
static_assert(c1.i == 1);

constexpr C c2 = 0;
static_assert(c2.i == 2);

//type:fn
//options:--c++17:--c++20:--ms_c++17:--ms_c++20
//options_all:-w

namespace minimal
{
  using fn_t = void();
  fn_t [ v ] = 0;
}

namespace valid
{
  struct C
  {
    int i;
    int j;
  };

  auto [ i, j ] = C{ 1, 2 };
}

namespace invalid
{
  struct C
  {
    int i;
    int j;
  };

  using fn_t = void(int);

  fn_t f1 = { };
  fn_t f2;

  fn_t [ v1 ] = { };
  fn_t [ v2 ];

  C [ i, j ] = C{ 1, 2 };
}

//type:fn
//options:--c++:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  struct B {
    operator bool();
  };
  struct D : B;
}

namespace decl_with_base
{
  struct B
  {
    operator bool();
  };

  struct D : B;
}

namespace defn_with_base
{
  struct B
  {
    operator bool();
  };

  struct D : B
  { };
}

namespace decl_with_virtual_base
{
  struct B
  {
    operator bool();
  };

  struct D : virtual B;
}

namespace base_with_no_conv
{
  struct B
  { };

  struct D : B;
}

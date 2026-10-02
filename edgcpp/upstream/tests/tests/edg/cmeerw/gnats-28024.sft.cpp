//type:fp
//options:--c++23:--c++23 --gn 150200:--c++23 --clang_version 210100
//options_all:-w

namespace minimal {
  int i = []<int = 0> -> int { return 1; }();
}

namespace templ_params
{
  int i1 = []<int = 0> { return 1; }();
  int i2 = []<int = 0> -> int { return 1; }();

  int i3 = []<int = 0> noexcept { return 1; }();
  int i4 = []<int = 0> noexcept -> int { return 1; }();

  int i5 = []<int = 0> [[]] { return 1; }();
  int i6 = []<int = 0> [[]] -> int { return 1; }();

  int i7 = []<int = 0> requires true { return 1; }();
  int i8 = []<int = 0> requires true -> int { return 1; }();

  int i9 = []<int = 0> requires true noexcept { return 1; }();
  int i10 = []<int = 0> requires true noexcept -> int { return 1; }();

  int i11 = []<int = 0> requires true [[]] { return 1; }();
  int i12 = []<int = 0> requires true [[]] -> int { return 1; }();
}

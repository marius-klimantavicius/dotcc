//type:fp
//options:--c++14:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w

namespace minimal
{
  int (*p)(int) = [] (auto o) {
    return [] (auto i) -> decltype(o + i) { return 0; };
  } (1);
}

namespace nested_lambda
{
  void foo()
  {
    auto l = [] (auto b) {
      return [] (auto d) -> decltype(b + d) { return 0; };
    };
    int (*fp) (int) = l(1);
  }
}

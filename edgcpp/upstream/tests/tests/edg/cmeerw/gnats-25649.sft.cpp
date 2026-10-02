//type:fp
//options:--c++11:--c++11 --gn 150100:--c++11 --clang_version 210100:--ms_c++20 --microsoft_version 1950
//options_all:-w

namespace minimal
{
  void g() {
    for (int i = decltype(i)();;) {}
  }
}

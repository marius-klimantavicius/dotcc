//type:fp
//options:--c++20:--c++20 --gn 150200:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename T> constexpr int v = 1;
  template<typename T> struct X {
    template<typename V>
    operator V() requires(v<T> == 1);
  };
  auto b = [](auto p) {
    return requires { p + X<int>(); };
  }(1);
}

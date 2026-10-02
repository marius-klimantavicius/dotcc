//type:fp
//options:--c++14 -tused:--c++20 -tused

namespace minimal
{
  template<typename T, T, typename F>
  int g(F f) {
    return f(1);
  }
  int i = g<int, 0>([] (auto) {
    return g<long, 1>([] (auto) {
      return 0; });
  });
}

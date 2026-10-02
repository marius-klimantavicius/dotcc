//type:fn
//options:--c++26

namespace minimal
{
  void f(auto v) {
    auto [... p] = v;           // error
  }
  template void f(int);
}

namespace dependent_container
{
  void f(auto v) {
    auto [... p] = v;           // error
  }
  template void f(int);
}

namespace non_dependent_container
{
  void f(auto, int v) {
    auto [... p] = v;           // error
  }
  template void f(int, int);
}

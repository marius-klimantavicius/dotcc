//type:fn
//options:--c++20 --clang_version 190100:--c++20 --gn 140200;fp

namespace fn_spec
{
  namespace ns
  {
    template <class T> void f(T) {}
  }

  using namespace ns;

  void f();

  struct A
  {
    friend void f<int>(int);
  };
}

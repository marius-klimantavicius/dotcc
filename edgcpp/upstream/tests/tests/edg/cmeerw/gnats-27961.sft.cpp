//type:fp
//options:--c++14:--c++14 --gn 140200:--c++14 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  struct A {
    template<typename T> void g(T);
    void f() {
      [] (auto i) -> decltype(g(i)) { } (1);
    }
  };
}

namespace non_tmpl_function
{
  struct C
  {
    void g(int);
    void f()
    {
      [] (auto i) -> decltype(g(i)) { } (1);
    }
  };
}

namespace tmpl_function
{
  struct D
  {
    template<typename T> void g(T);
    void f()
    {
      [] (auto i) -> decltype(g(i)) { } (1);
    }
  };
}

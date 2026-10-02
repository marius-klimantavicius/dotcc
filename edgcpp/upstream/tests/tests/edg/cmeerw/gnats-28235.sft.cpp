//type:fp
//options:--c++11 -A:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1944
//options_all:-w -tused

namespace minimal
{
  struct B {
    using type = int;
    int i;
  };
  template<int I>
  void f() {
    B b{ I };
    decltype(b)::type t;
  }
  template void f<0>();
}

namespace dependent
{
  template<typename T>
  using A = T;

  struct B
  {
    int i;
    static constexpr int J = 0;
  };

  const char *g(B, long);

  template<int I, typename T>
  void f()
  {
    B b{ I };
    using BB = decltype(b);

#ifndef _MSC_VER
    g(b, decltype(b)::J) == nullptr;
    g(b, BB::J) == nullptr;
    g(b, A<decltype(b)>::J) == nullptr;

    decltype(b) b2;
    g(b2, decltype(b2)::J) == nullptr;

    A<decltype(b)> b3;
    g(b3, decltype(b3)::J) == nullptr;
#endif

    T t{ I };
    using TT = decltype(t);
    g(b, decltype(t)::J) * 1;
    g(b, TT::J) * 1;
    g(b, A<decltype(t)>::J) * 1;

    decltype(t) t2;
    g(t2, decltype(t2)::J) * 1;
  }

  int g(B, int);

  template void f<0, B>();
}

#ifdef __cpp_concepts
namespace abbreviated_template
{
  struct C
  {
    using type = int;
  };

  int f(auto a, typename decltype(a)::type b)
  {
    return b;
  }

  int i = f(C{}, 1);
}
#endif

//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w -tused

namespace minimal
{
  template<int I>
  struct N { };
  template<N L>
  struct S { };
  template<N L>
  int f(S<L>);
  int i = f(S<N<1>{}>{});
}

namespace class_placehoder
{
  template<int I>
  struct N { };

  template<N L>
  struct S { };

  template<N L>
  int f(S<L>);

  int i = f(S<N<1>{}>{});
}

namespace typedef_class_placehoder
{
  template<int I>
  struct N { };

  template<N L>
  struct S { };

  template<typename T>
  using A = T;

  template<N L>
  int f(S<L>);

  int i = f(A<S<A<N<1>>{}>>{});
}

namespace auto_type
{
  template<int I>
  struct N { };

  template<auto L>
  struct S { };

  template<auto L>
  int f(S<L>);

  int i = f(S<N<1>{}>{});
}

namespace decltype_auto_type
{
  template<int I>
  struct N { };

  template<decltype(auto) L>
  struct S { };

  template<decltype(auto) L>
  int f(S<L>);

  int i = f(S<N<1>{}>{});
}

//type:fp
//options:--c++11:--c++20:--c++20 --gn 160100:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<int>
  struct C { };
  C<{}> c0;
}

namespace fn_template
{
  template<int>
  int f();

  int i = f<{}>();
  int j = f<{ 1 }>();
}

namespace class_template
{
  template<int>
  struct C
  {
    int i;
  };

  C<{}> c0;
  C<{ 1 }> c1;
}

#if __cpp_nontype_template_args >= 201911L
namespace aggregate_init
{
  struct B
  {
    int i, j;
  };

  template<B>
  struct C { };

  C<{}> c0;
  C<{1}> c1;
  C<{1, 1}> c11;
}

namespace ctor_init
{
  struct B
  {
    constexpr B()
      : i(), j()
    { }

    constexpr B(int v1)
      : i(v1), j()
    { }

    constexpr B(int v1, int v2)
      : i(v1), j(v2)
    { }

    int i, j;
  };

  template<B>
  struct C { };

  C<{}> c0;
  C<{1}> c1;
  C<{1, 1}> c11;
}

namespace deduce
{
  template<typename T>
  struct B
  {
    constexpr B(T)
    { }
  };

  template<B>
  struct C
  { };

  C<{ 1 }> c1;
}
#endif

//type:fn
//options:--c++20

namespace narrowing
{
  template<signed char>
  struct C
  { };

  C<{ 1024 }> c1024;            // error
}

namespace explicit_ctor_init
{
  struct B
  {
    constexpr explicit B()
      : i(), j()
    { }

    constexpr explicit B(int v1)
      : i(v1), j()
    { }

    constexpr explicit B(int v1, int v2)
      : i(v1), j(v2)
    { }

    int i, j;
  };

  template<B>
  struct C { };

  C<{}> c0;                     // error
  C<{1}> c1;                    // error
  C<{1, 1}> c11;                // error
}

namespace explicit_deduction_guide
{
  template<typename T>
  struct B
  {
    constexpr explicit B(T t)
      : i(t)
    { }

    int i;
  };

  template<B>
  struct C { };

  C<{1}> c1;                    // error
}

namespace decltype_auto
{
  template<decltype(auto)>
  struct C
  { };

  C<{1}> c1;                    // error
}

namespace potential_internal_error
{
  int g();
  struct S { constexpr S(int) { } };
  template<S> int f();
  int i = f<{g()}>();           // error
}

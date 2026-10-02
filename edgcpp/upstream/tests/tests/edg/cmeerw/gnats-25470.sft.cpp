//type:fp
//options:--c++20:--ms_c++20
namespace cls_tmpl
{
  template<int> struct C { };

  template<int I, C<I>>
  struct X
  { static constexpr int value = -1; };

  template<C<0> C0>
  struct X<0, C0>
  { static constexpr int value = 0; };

  template<C<1> C1>
  struct X<1, C1>
  { static constexpr int value = 1; };

  template<C<2> C2>
  struct X<2, C2>
  { static constexpr int value = 2; };

  static_assert(X<0, C<0>{}>::value == 0);
  static_assert(X<1, C<1>{}>::value == 1);
  static_assert(X<2, C<2>{}>::value == 2);

  static_assert(X<-1, C<-1>{}>::value == -1);
}

namespace var_tmpl
{
  template<int> struct C { };

  template<int I, C<I>>
  constexpr int value = -1;

  template<C<0> C0>
  constexpr int value<0, C0> = 0;

  template<C<1> C1>
  constexpr int value<1, C1> = 1;

  template<C<2> C2>
  constexpr int value<2, C2> = 2;

  static_assert(value<0, C<0>{}> == 0);
  static_assert(value<1, C<1>{}> == 1);
  static_assert(value<2, C<2>{}> == 2);

  static_assert(value<-1, C<-1>{}> == -1);
}

namespace requires_expr
{
  template<int> struct C { };

  template<int I, C<I>>
  struct X { };

  template<int I, C<I> CI>
  struct B
  {
    static_assert(requires { typename X<I, CI>; });
  };

  template struct B<0, C<0>{}>;
  template struct B<1, C<1>{}>;
}

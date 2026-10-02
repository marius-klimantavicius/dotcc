//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936
//options_all:-tused

namespace minimal
{
  template<int I, typename... Ts> concept X = sizeof...(Ts) == I;
  template<typename ... Vs>
  struct C {
    template<int J> struct D;
  };
  template<typename ... Us> template<int I>
  struct C<Us ...>::D {
    int f() requires X<I, Us ...>;
  };
  int i = C<int, char>::D<2>().f();
}

namespace in_class_definition
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C
  {
    template<int J>
    struct D
    {
      int f() requires X<J, Vs ...>;
    };
  };

  int i = C<>::D<0>().f() +
          C<int>::D<1>().f() +
          C<int, char>::D<2>().f();
}

namespace outer_non_template
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  struct O
  {
    template<typename ... Vs>
    struct C
    {
      template<int J>
      struct D;
    };
  };

  template<typename ... Us>
  template<int I>
  struct O::C<Us ...>::D
  {
    int f() requires X<I, Us ...>;
  };

  int i = O::C<>::D<0>().f() +
          O::C<int>::D<1>().f() +
          O::C<int, char>::D<2>().f();
}

namespace intermediate_non_template
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C
  {
    struct N
    {
      template<int J>
      struct D;
    };
  };

  template<typename ... Us>
  template<int I>
  struct C<Us ...>::N::D
  {
    int f() requires X<I, Us ...>;
  };

  int i = C<>::N::D<0>().f() +
          C<int>::N::D<1>().f() +
          C<int, char>::N::D<2>().f();
}

namespace inner_non_template
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C
  {
    template<int J>
    struct D;
  };

  template<typename ... Us>
  template<int I>
  struct C<Us ...>::D
  {
    struct N
    {
      int f() requires X<I, Us ...>;
    };
  };

  int i = C<>::D<0>::N().f() +
          C<int>::D<1>::N().f() +
          C<int, char>::D<2>::N().f();
}

namespace inner_pack
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C
  {
    template<int J, typename ... Ys>
    struct D;
  };

  template<typename ... Us>
  template<int I, typename ... Ws>
  struct C<Us ...>::D
  {
    int f() requires X<I, Us ...> && X<I, Ws ...> &&
                     (!X<I + 1, Us ...>) && (!X<I + 1, Ws ...>);
  };

  int i = C<>::D<0>().f() +
          C<int>::D<1, short>().f() +
          C<int, char>::D<2, short, long>().f();
}

namespace outer_explicit_spec
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename T>
  struct O;

  template<>
  struct O<void>
  {
    template<typename ... Vs>
    struct C
    {
      template<int J>
      struct D;
    };
  };

  template<typename ... Us>
  template<int I>
  struct O<void>::C<Us ...>::D
  {
    int f() requires X<I, Us ...>;
  };

  int i = O<void>::C<>::D<0>().f() +
          O<void>::C<int>::D<1>().f() +
          O<void>::C<int, char>::D<2>().f();
}

namespace partial_spec
{
  template<int I, typename... Ts>
  concept X = sizeof...(Ts) == I;

  template<typename ... Vs>
  struct C;

  template<typename ... Vs>
  struct C<Vs * ...>
  {
    template<int J>
    struct D;
  };

  template<typename ... Us>
  template<int I>
  struct C<Us * ...>::D
  {
    int f() requires X<I, Us ...>;
  };

  int i = C<>::D<0>().f() +
          C<int *>::D<1>().f() +
          C<int *, char *>::D<2>().f();
}

//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<int> struct C {
    static constexpr bool v = true;
  };
  template<int I> struct B
  {
    template<int> struct D {
      static int f(auto) requires(C<I>::v);
    };
  };
  int i = B<1>::D<2>::f(3);
}

namespace non_abbreviated
{
  template<int> struct C {
    static constexpr bool v = true;
  };
  template<int I> struct B
  {
    template<int> struct D {
      template<typename V> requires(C<I>::v)
      static int f(V);
    };
  };
  int i = B<1>::D<2>::f(3);
}

namespace constraints_class_member
{
  template<int I>
  struct A
  {
    static constexpr bool v = I != 0;
  };

  template<int I>
  struct B
  {
    template<int J>
    struct D
    {
      template<typename V> requires(A<I>::v)
      static int f1(V);

      template<typename V>
      static int f2(V) requires(A<I>::v);

      template<typename V> requires(A<J>::v)
      static int g1(V);

      template<typename V>
      static int g2(V) requires(A<J>::v);
    };
  };


  int f11 = B<1>::D<2>::f1(1);
  int f12 = B<1>::D<2>::f1(1);

  int f21 = B<1>::D<2>::f2(2);
  int f22 = B<1>::D<2>::f2(2);

  int g11 = B<1>::D<2>::g1(1);
  int g12 = B<1>::D<2>::g1(1);

  int g21 = B<1>::D<2>::g2(2);
  int g22 = B<1>::D<2>::g2(2);
}

namespace constraints_variable_template
{
  template<int I>
  constexpr bool v = I != 0;

  template<int I>
  struct B
  {
    template<int J>
    struct D
    {
      template<typename V> requires(v<I>)
      static int f1(V);

      template<typename V>
      static int f2(V) requires(v<I>);

      template<typename V> requires(v<J>)
      static int g1(V);

      template<typename V>
      static int g2(V) requires(v<J>);
    };
  };


  int f11 = B<1>::D<2>::f1(1);
  int f12 = B<1>::D<2>::f1(1);

  int f21 = B<1>::D<2>::f2(2);
  int f22 = B<1>::D<2>::f2(2);

  int g11 = B<1>::D<2>::g1(1);
  int g12 = B<1>::D<2>::g1(1);

  int g21 = B<1>::D<2>::g2(2);
  int g22 = B<1>::D<2>::g2(2);
}

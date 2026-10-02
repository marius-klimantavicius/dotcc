//type:fp
//options:--c++11:--c++20

#if __cpp_nontype_template_args
namespace minimal
{
  struct B { };
  void f(B &) = delete;
  void f(const B &);
  template<const int I, B V>
  void g(decltype(I) &i) {
    i = 0;
    f(V);
  }
}
#endif

namespace minimal_non_class
{
  template<const int I>
  int *f(decltype(I) *p) {
    return p;
  };
}

template<typename, typename>
struct is_same
{
  static constexpr bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static constexpr bool value = true;
};

namespace non_class_param_type
{
  template<int I, const int J>
  void f()
  {
    static_assert(is_same<int, decltype(I)>::value, "decltype(I)");
    static_assert(is_same<int, decltype(J)>::value, "decltype(J)");

    static_assert(is_same<int, decltype((I))>::value, "decltype((I))");
    static_assert(is_same<int, decltype((J))>::value, "decltype((J))");
  }

  template void f<1, 2>();

  template<int I, const int J>
  struct A
  {
    static_assert(is_same<int, decltype(I)>::value, "decltype(I)");
    static_assert(is_same<int, decltype(J)>::value, "decltype(J)");

    static_assert(is_same<int, decltype((I))>::value, "decltype((I))");
    static_assert(is_same<int, decltype((J))>::value, "decltype((J))");
  };

  template struct A<1, 2>;
}

#if __cpp_nontype_template_args
namespace class_param_type
{
  struct C
  {
    int i;
  };

  void g(C &) = delete;
  constexpr int g(const C &)
  { return 1; }

  template<C I, const C J>
  void f()
  {
    static_assert(g(I) == 1);
    static_assert(g(J) == 1);

    static_assert(is_same<C, decltype(I)>::value, "decltype(I)");
    static_assert(is_same<C, decltype(J)>::value, "decltype(J)");

    static_assert(is_same<const C &, decltype((I))>::value, "decltype((I))");
    static_assert(is_same<const C &, decltype((J))>::value, "decltype((J))");
  }

  template void f<C{1}, C{2}>();

  template<C I, const C J>
  struct A
  {
    static_assert(is_same<C, decltype(I)>::value, "decltype(I)");
    static_assert(is_same<C, decltype(J)>::value, "decltype(J)");

    static_assert(is_same<const C &, decltype((I))>::value, "decltype((I))");
    static_assert(is_same<const C &, decltype((J))>::value, "decltype((J))");
  };

  template struct A<C{1}, C{2}>;
}
#endif

namespace deduction
{
  template<int I>
  struct B
  { };

  template<const int I>
  int f(B<I>);

  int i = f(B<1>{});
}

#if __cpp_deduction_guides
namespace deduction_guides
{
  template<int I>
  struct B
  { };

  template<const int I>
  struct C
  {
    C(B<I>);
  };

  C c(B<1>{});
}
#endif

namespace typedef_int
{
  using INT = int;

  template<INT I>
  int *f(decltype(I) *p)
  {
    return p;
  }
}

namespace typedef_const_int
{
  using CINT = const int;

  template<CINT I>
  int *f(decltype(I) *p)
  {
    return p;
  }
}

namespace const_decltype
{
  const int ci = 0;

  template<decltype(ci) I>
  int *f(decltype(I) *p)
  {
    return p;
  }
}

#if __cpp_deduction_guides
namespace deduced_type
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<C c1, C<int> c2>
  struct X
  {
    decltype(c1) m1;
    decltype(c2) m2;
  };

  void f(X<C{1}, C{2}> x)
  {
    ++x.m1.t;
    ++x.m2.t;
  }
}
#endif

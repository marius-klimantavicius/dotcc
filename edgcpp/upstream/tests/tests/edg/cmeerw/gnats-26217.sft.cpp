//type:fp
//options:--c++17:--c++20:--ms_c++17:--ms_c++20

namespace minimal
{
  struct B { B(); };
  B b;
  template<auto &v = b>
  int f() { return 0; }
  int i = f();
}

namespace non_constexpr_obj
{
  struct B
  {
    B();
  };

  B b;

  template<const auto &b_v = b>
  void f() { }

  void (*p1)() = &f<>;
  void (*p2)() = &f<b>;
}

namespace non_constexpr_obj_ptr
{
  struct B
  {
    B();
  };

  B b;

  template<auto *b_v = &b>
  void f() { }

  void (*p1)() = &f<>;
  void (*p2)() = &f<&b>;
}

namespace non_const_param_type
{
  struct B
  {
    B();
  };

  B b;

  template<auto &b_v = b>
  void f() { }

  void (*p1)() = &f<>;
  void (*p2)() = &f<b>;
}

namespace structural_type
{
  struct B
  { };
  constexpr B b{};

  template<const auto &b_v = b>
  void f() { }

  void (*p1)() = &f<>;
  void (*p2)() = &f<b>;
}

namespace non_structural_type
{
  class C
  {
    int i;
  };

  constexpr C c{};

  template<const auto &c_v = c>
  void f() { }

  void (*p1)() = &f<>;
  void (*p2)() = &f<c>;
}

namespace dpdt_auto_type
{
  template<auto N, auto M = N>
  void f() { }

  void (*p)() = &f<1>;
}

namespace dpdt_auto_type_call
{
  template<int N>
  struct C
  { };

  template<auto N, auto M = N>
  void f(C<N>) { }

  void g()
  {
    f(C<1>{});
  }
}

#ifndef _MSC_VER
// There is an unrelated issue in Microsoft mode
namespace non_dpdt_auto_type_fn_ref
{
  template<typename T>
  struct C
  { };

  template<typename T>
  T x(T);

  template<typename T, typename U, auto (&F)(int) = x<U> >
  void f(C<T>, C<U>) { }

  void g()
  {
    f(C<int>{}, C<int>{});
  }
}

namespace dpdt_auto_type_fn_ref
{
  template<typename T>
  struct C
  { };

  template<typename T>
  T x(T);

  template<typename T, typename U, auto (&F)(T) = x<U> >
  void f(C<T>, C<U>) { }

  void g()
  {
    f(C<int>{}, C<int>{});
  }
}
#endif

namespace dpdt_auto_type_fn_ptr
{
  template<typename T>
  struct C
  { };

  template<typename T>
  T x(T);

  template<typename T, typename U, auto (*F)(U) = x<U> >
  void f(C<T>, C<U>) { }

  void g()
  {
    f(C<int>{}, C<long>{});
  }
}

namespace decltype_auto_deduction_from_paren_init
{
  struct B
  {
    int i = 0;
  };

  constexpr B b;

  struct C
  {
    static constexpr B b{ };
  };

  template<decltype(auto) v = (b)>
  constexpr int f()
  {
    return v.i;
  }

  template<decltype(auto) v = (C::b)>
  constexpr int g()
  {
    return v.i;
  }

  static_assert(f<>() == 0);
  static_assert(g<>() == 0);
}

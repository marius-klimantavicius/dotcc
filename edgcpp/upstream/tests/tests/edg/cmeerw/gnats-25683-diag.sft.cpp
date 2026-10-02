//type:fn
//options:--c++17:--ms_c++17
//options_all:-tused
namespace diag_non_templ
{
  template<int I, int J>
  void bar()
  {
    static_assert(I == J, "fails for I=0,J=-2");
  }

  void foo()
  {
    if constexpr (false) nontmpl_undef_1; // error
    if constexpr (true) nontmpl_undef_2;  // error

    if constexpr (false) bar<0, -1>(); // OK, not ODR-used
    if constexpr (true) bar<0, -2>();  // triggers error
  }
}

namespace diag_templ_defn
{
  template<int I, int J>
  void bar()
  {
    static_assert(I == J, "fails for I=0,j=-5 and I=1,J=-6");
  }

  template<int I>
  void foo()
  {
    if constexpr (false) tmpl_undef_1; // error
    if constexpr (I < 0) tmpl_undef_2; // error
    if constexpr (I > 0) tmpl_undef_3; // error

    if constexpr (false) bar<0, -1>(); // OK, not ODR-used
    if constexpr (false) bar<I, -2>(); // OK, not ODR-used

    if constexpr (I < 0) bar<0, -3>(); // OK, not ODR-used
    if constexpr (I < 0) bar<I, -4>(); // OK, not ODR-used

    if constexpr (I > 0) bar<0, -5>(); // triggers error
    if constexpr (I > 0) bar<I, -6>(); // triggers error
  }

  template void foo<0>();
  template void foo<1>();
}

namespace diag_dpdt_condition
{
  template<bool B>
  struct D
  { };

  template<bool B>
  void foo()
  {
    [] (auto t) {
      if constexpr (sizeof (t) > 0)
      {
        typename D<B>::type t;        // error
        static_assert(B);             // okay with CWG2518
      }
    };
  }

  template void foo<false>();
}

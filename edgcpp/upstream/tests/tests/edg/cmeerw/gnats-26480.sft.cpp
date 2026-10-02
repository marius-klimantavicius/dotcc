//type:fp
//options:--c++20:--c++20 --gn 120100:--ms_c++20

namespace minimal
{
#if __GNUC__ || _MSC_VER
  template<typename T> struct C {
    char g(T);
    int f() requires (sizeof(g(1)) == 1);
  };
  int i = C<int>().f();
#endif
}

namespace minimal_this
{
  template<typename T> struct C {
    void g(T);
    void g(T, int);
    int f() requires requires { g(1); };
  };
  int i = C<int>().f();
}

namespace gnats_testcase
{
#if __GNUC__
  template <int __v> struct integral_constant {
    static constexpr int value = __v;
  };
  struct access {
    void base();
    template <typename> void base();
  };
  template <typename> using void_t = void;
  template <typename, template <class> class, typename...>
  struct detector : integral_constant<false> {};
  template <template <class> class Template, typename... Args>
  struct detector<void_t<Template<Args...>>, Template, Args...>;
  template <typename D> struct iterator_interface {
    D derived();
    using difference_type = int;
    auto operator+=(difference_type) { access::base(derived()); }
    auto operator--() requires requires { access::base(derived()); }
    { derived() += difference_type(); }
  };
  template <template <class> class Template, typename... Args>
  using ill_formed = integral_constant<!detector<void, Template, Args...>::value>;
  template <typename T> using decrementable_t = decltype(--T());
  struct basic_forward_iter : iterator_interface<basic_forward_iter> {};
  static_assert(ill_formed<decrementable_t, basic_forward_iter>::value);
#endif
}

namespace member_fn
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    void nondep(int &);
    D nondep(int);

    void dep(T &);
    D dep(T);

    int g1() requires requires { nondep(0).template f<0>(); };
    int g2() requires requires { dep(0).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2();
}

namespace member_fn_constraints_char
{
#if __GNUC__ || _MSC_VER
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    D dep(T);

    int g() requires requires (char c) { dep(1).template f<0>(); };
    int g() requires requires (char c) { dep(c).template f<0>(); };
  };

  int i = B<char &>().g();
#endif
}

namespace member_tmpl_fn
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    template<typename U>
    void nondep(int &, U);
    template<typename U>
    D nondep(int, U);

    template<typename U>
    void dep(T &, U);
    template<typename U>
    D dep(T, U);

    int g1() requires requires { nondep(0, 0).template f<0>(); };
    int g2() requires requires { dep(0, 0).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2();
}

namespace member_tmpl_fn_constraints
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    template<typename U>
    D nondep(int &, U);

    template<typename U>
    D dep(T &, U);

    int g1() requires requires (T i) { nondep(i, 0).template f<0>(); };
    int g1() requires requires (T i) { nondep(+i, 0).template f<0>(); };

    int g2() requires requires (T i) { dep(i, 0).template f<0>(); };
    int g2() requires requires (T i) { dep(+i, 0).template f<0>(); };

    int h1() requires requires (T i) { nondep<int>(i, 0).template f<0>(); };
    int h1() requires requires (T i) { nondep<int>(+i, 0).template f<0>(); };

    int h2() requires requires (T i) { dep<int>(i, 0).template f<0>(); };
    int h2() requires requires (T i) { dep<int>(+i, 0).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2() + B<int>().h1() + B<int>().h2();
}

namespace non_mbr_tmpl_fn
{
  struct D
  {
    template<int> void f();
  };

  template<typename U>
  void nondep(int &, U);
  template<typename U>
  D nondep(int, U);

  template<typename T, typename U>
  void dep(T &, U);
  template<typename T, typename U>
  D dep(T, U);

  template<typename T>
  struct B
  {
    int g1() requires requires { nondep(0, 0).template f<0>(); };
    int g2() requires requires { dep(0, 0).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2();
}

namespace nonmbr_tmpl_fn_constraints
{
  struct D
  {
    template<int> void f();
  };

  template<typename U>
  D nondep(int &, U);

  template<typename T, typename U>
  D dep(T &, U);

  template<typename T>
  struct B
  {
    int g1() requires requires (T i) { nondep(i, 0).template f<0>(); };
    int g1() requires requires (T i) { nondep(+i, 0).template f<0>(); };

    int g2() requires requires (T i) { dep(i, 0).template f<0>(); };
    int g2() requires requires (T i) { dep(+i, 0).template f<0>(); };

    int h1() requires requires (T i) { nondep<int>(i, 0).template f<0>(); };
    int h1() requires requires (T i) { nondep<int>(+i, 0).template f<0>(); };

    int h2() requires requires (T i) { dep<int>(i, 0).template f<0>(); };
    int h2() requires requires (T i) { dep<int>(+i, 0).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2() + B<int>().h1() + B<int>().h2();
}

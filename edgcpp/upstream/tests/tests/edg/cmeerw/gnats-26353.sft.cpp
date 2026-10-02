//type:fp
//options:--c++11:--ms_c++20
//options_all:-w

namespace minimal
{
  struct B {
    friend void f(B) noexcept(v);  // Previously a spurious 'identifier "v" is
                                   // undefined' error. Now okay.
    static constexpr bool v = true;
  };
}

namespace in_noexcept
{
  template<typename T>
  struct C {
    friend void f(const C & c) noexcept(noexcept(C{}))
    { }
  };

  static_assert(noexcept(f(C<int>())), "noexcept");
  static_assert(noexcept(f(C<long>())), "noexcept");
}

namespace non_tmpl_out_of_class_decl
{
  struct C;

  void f(const C &, int i) noexcept(sizeof i == sizeof(int));

  struct C
  {
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
  };

  static_assert(noexcept(f(C(), 1)), "noexcept");
}

namespace tmpl_out_of_class_decl
{
  template<typename T>
  struct C;

  void f(const C<int> &, int i) noexcept(sizeof i == sizeof(int));

  template<typename T>
  struct C
  {
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
  };

  static_assert(noexcept(f(C<int>(), 1)), "noexcept");
}

namespace non_tmpl
{
  int g;

  struct C
  {
    friend void f(const C & c, int i) noexcept(sizeof g == sizeof(int));
    friend void h(const C & c, int i) noexcept(sizeof g != sizeof(int));
  };

  static_assert(noexcept(f(C(), 1)), "noexcept");
  static_assert(!noexcept(h(C(), 1)), "noexcept");
}

namespace non_tmpl_param_ref
{
  struct C
  {
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
    friend void f(const C & c, int i) noexcept(sizeof i == sizeof(int));
  };

  static_assert(noexcept(f(C(), 1)), "noexcept");
}

namespace tmpl_param_ref
{
  template<typename T>
  struct C
  {
    friend void f(const C & c, T i) noexcept(sizeof i == sizeof(T));
    friend void f(const C & c, T i) noexcept(sizeof i == sizeof(T));
  };

  static_assert(noexcept(f(C<int>(), 1)), "noexcept");
}

namespace friend_typedef_type
{
  using fn_t = void (int);

  struct C
  {
    friend fn_t f;
  };
}

namespace delay_instantiate_of_exception_spec
{
  template<typename T>
  struct C
  {
    friend void f(const C &, T i) noexcept(T::dont_instantiate);
  };

  C<int> c;
}

namespace tmpl_delay_instantiate_of_exception_spec
{
  template<typename T>
  struct C
  {
    template<typename U>
    friend void f(const C &, T i) noexcept(T::dont_instantiate);
  };

  C<int> c;
}

namespace tmpl_friend_member
{
  template<typename T>
  struct C
  {
    friend void f(C const &, int i) noexcept(sizeof i == sizeof(int));
    friend void g(C const &, char c) noexcept(sizeof c == sizeof(char));
  };

  void foo(C<int> c)
  {
    static_assert(noexcept(f(c, 1)), "noexcept");
    static_assert(noexcept(g(c, 'a')), "noexcept");
  }
}

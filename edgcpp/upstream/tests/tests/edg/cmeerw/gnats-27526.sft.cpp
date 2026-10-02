//type:fp
//options:--c++14 --no_exceptions:--c++14 --exceptions:--c++20 --no_exceptions:--c++20 --exceptions

namespace minimal
{
  struct C {
    friend void f(const C &) noexcept(true) {}
  };
}

namespace non_tmpl
{
  struct C
  {
    void g() noexcept(mbr_value)
    { }

    friend void f(const C &) noexcept(mbr_value)
    { }

    template<typename U>
    friend void tf(const C &, U) noexcept(mbr_value)
    { }

    static constexpr bool mbr_value = true;
  };

  void test(C c)
  {
    c.g();
    f(c);
    tf(c, 1);

    static_assert(noexcept(c.g()), "");
    static_assert(noexcept(f(c)), "");
    static_assert(noexcept(tf(c, 1)), "");
  }
}

namespace tmpl
{
  template<typename T>
  struct C
  {
    void g() noexcept(mbr_value)
    { }

    friend void f(const C &) noexcept(mbr_value)
    { }

    template<typename U>
    friend void tf(const C &, U u) noexcept(mbr_value)
    { }

    static constexpr bool mbr_value = true;
  };

  template struct C<int>;

  void test(C<int> c)
  {
    c.g();
    f(c);
    tf(c, 1);

    static_assert(noexcept(c.g()), "");
    static_assert(noexcept(f(c)), "");
    static_assert(noexcept(tf(c, 1)), "");
  }
}

#if __cpp_concepts
namespace non_tmpl_auto
{
  struct C
  {
    void g(auto) noexcept(mbr_value)
    { }

    friend void f(const C &, auto) noexcept(mbr_value)
    { }

    static constexpr bool mbr_value = true;
  };

  void test(C c)
  {
    c.g(1);
    f(c, 1);

    static_assert(noexcept(c.g(1)), "");
    static_assert(noexcept(f(c, 1)), "");
  }
}

namespace tmpl_auto
{
  template<typename T>
  struct C
  {
    void g(auto) noexcept(mbr_value)
    { }

    static constexpr bool mbr_value = true;
  };

  template struct C<int>;

  void test(C<int> c)
  {
    c.g(1);
  }
}
#endif

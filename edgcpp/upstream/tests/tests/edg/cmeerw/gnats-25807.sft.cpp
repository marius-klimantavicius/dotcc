//type:fp
//options:--c++11:--c++11 --clang:--c++11 --g++:--c++20:--c++20 --clang:--c++20 --g++:--ms_c++20:--c++11 --g++ -DFAIL;fn
//options_all:-tused --no_defer

#ifndef __clang__
namespace minimal
{
  struct C {
    int operator () () const;
    template<int I>
    static auto f(const C &c) -> decltype(c());
  };
  int i = C::f<0>(C());
}
#endif

namespace fail
{
#ifdef FAIL
  struct D;
  template<typename> void f(D const &d) {
    d();  // error even in gcc/clang modes with --no_defer
  }
#endif
}

namespace incomplete_type
{
  struct C
  {
    int operator () (int) const;

    struct D
    {
      template<typename T>
      auto g(const C &c, T t) -> decltype(c(t))
      {
        return (c)(t);
      }
    };

    template<typename T>
    auto f(T t) -> decltype((*this)(t))
    {
      return (*this)(t);
    }

#if __cpp_concepts
    template<typename U>
    auto h(U u) requires requires (const C &c, U u) {
      c(u);
    }
    {
      return (*this)(u);
    }
#endif
  };

  void foo()
  {
    auto v1 = C{}.f(0);
    auto v2 = C::D{}.g(C{}, 0);

#if __cpp_concepts
    auto v3 = C{}.h(0);
#endif
  }
}

namespace dependent
{
  struct C
  {
    template<typename T>
    void f();
  };

  struct A;

  template<typename>
  struct B
  {
    C f() const;

    template<typename T>
    void g() const
    {
#if defined(__GNUC__) || defined(__clang__) || defined(_MSC_VER)
      f().template f<int>();
#else
      f().f<int>();
#endif
    }
  };

  void foo()
  {
    B<int>().g<int>();
  }
}

#ifdef __GNUC__
namespace example_from_pr_15619
{
  class A;

  template <class> struct B
  {
    A m_fn1(const char *) const;
    template <class T> void m_fn2(T const &) const;
  };

  template <typename U> template <class T> void B<U>::m_fn2(T const &) const
  {
    m_fn1("")();
  }

  class A
  {
  public:
    void operator () () const;
  };

  void f()
  {
    B<int>().m_fn2(1);
  }
}
#endif

namespace dependent_parameter_type
{
  struct D
  {
    template<int>
    void f();
  };

#define NON_DPDT(x) x.f<0>()
#define DPDT(x) x.m

  template<typename T>
  struct C
  {
    static D s1();
    static D s2(int);
    static D s3(const C &);

    D f() const;

    D ov1(long);
    static D ov1(int);

    static D ov2(long);
    D ov2(int) const;

    D ov3(long) const;
    D ov3(int) const;

    void foo(const C &c, const C *p, D (&fn1)(), D (&fn2)(T))
    {
      // these are always non-dependent
      NON_DPDT(s1());
      NON_DPDT(s2(0));
      NON_DPDT(fn1());

      // these are dependent for clang
#if __clang__
#define TEST(x) DPDT(x)
#else
#define TEST(x) NON_DPDT(x)
#endif
      TEST(this->s1());
      TEST(c.s1());
      TEST(p->s1());

      TEST(this->s2(0));
      TEST(c.s2(0));
      TEST(p->s2(0));
#undef TEST

      // following are all dependent for clang/gcc/msvc
#if __GNUC__ || __clang__ || _MSC_VER
#define TEST(x) DPDT(x)
#else
#define TEST(x) NON_DPDT(x)
#endif

#if __GNUC__ || __clang__ || _MSC_VER
      // still not handled correctly in GCC/MSVC mode
      s3(*this, 2);
      s3(c, 2);
      s3(*p, 2);
      this->f(1);
      p->f(1);
      fn2(1, 2);
#else
      TEST(s3(*this));
      TEST(s3(c));
      TEST(s3(*p));
      TEST(this->f());
      TEST(p->f());
      TEST(fn2(1));
#endif

      TEST(f());
      TEST(f());

      TEST(ov1(0L));
      TEST(this->ov1(0L));
      TEST(c.ov1(0L));
      TEST(p->ov1(0L));
      TEST(ov1(0));
      TEST(this->ov1(0));
      TEST(c.ov1(0));
      TEST(p->ov1(0));

      TEST(ov2(0L));
      TEST(this->ov2(0L));
      TEST(c.ov2(0L));
      TEST(p->ov2(0L));
      TEST(ov2(0));
      TEST(this->ov2(0));
      TEST(c.ov2(0));
      TEST(p->ov2(0));

      TEST(ov3(0L));
      TEST(this->ov3(0L));
      TEST(c.ov3(0L));
      TEST(p->ov3(0L));
      TEST(ov3(0));
      TEST(this->ov3(0));
      TEST(c.ov3(0));
      TEST(p->ov3(0));
#undef TEST
    }
  };

#undef DPDT
#undef NON_DPDT
}

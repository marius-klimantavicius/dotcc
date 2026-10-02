//type:fp
//options:--c++14:--c++20:--c++20 --gn 140200:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1942
//options_all:-tused -w

namespace minimal
{
  template<int> struct C { };
  void g(int i) {
    [] (auto j) -> C<sizeof(i + j)> { return {}; };
  }
}

namespace trailing_return_decltype
{
  template<typename T>
  inline void f(T && fn)
  { fn(1); }

  namespace non_tmpl_fn
  {
    void g()
    {
      int local_var = 1;
      f([] (auto i) -> decltype(i + local_var) { return 1; });
    }
  }

  namespace non_tmpl_fn_constexpr
  {
    void g()
    {
      constexpr int local_var = 1;
      f([] (auto i) -> decltype(i + local_var) { return 1; });
    }
  }

  namespace tmpl_fn
  {
    template<typename T>
    void g()
    {
      T local_var = 1;
      f([] (auto i) -> decltype(i + local_var) { return 1; });
    }

    template void g<int>();
  }

  namespace tmpl_fn_constexpr
  {
    template<typename T>
    void g()
    {
      constexpr T local_var = 1;
      f([] (auto i) -> decltype(i + local_var) { return 1; });
    }

    template void g<int>();
  }
}

#ifdef __GNUC__
namespace trailing_return_typeof
{
  template<typename T>
  inline void f(T && fn)
  { fn(1); }

  namespace non_tmpl_fn
  {
    void g()
    {
      int local_var = 1;
      f([] (auto i) -> typeof(i + local_var) { return 1; });
    }
  }

  namespace non_tmpl_fn_constexpr
  {
    void g()
    {
      constexpr int local_var = 1;
      f([] (auto i) -> typeof(i + local_var) { return 1; });
    }
  }

  namespace tmpl_fn
  {
    template<typename T>
    void g()
    {
      T local_var = 1;
      f([] (auto i) -> typeof(i + local_var) { return 1; });
    }

    template void g<int>();
  }

  namespace tmpl_fn_constexpr
  {
    template<typename T>
    void g()
    {
      constexpr T local_var = 1;
      f([] (auto i) -> typeof(i + local_var) { return 1; });
    }

    template void g<int>();
  }
}
#endif

namespace trailing_return_noexcept
{
  template<typename T>
  inline void f(T && fn)
  { fn(1); }

  template<bool B>
  struct C
  {
    C(int);
  };

  namespace non_tmpl_fn
  {
    void g()
    {
      int local_var = 1;
      f([] (auto i) -> C<noexcept(i + local_var)> { return 1; });
    }
  }

  namespace non_tmpl_fn_constexpr
  {
    void g()
    {
      constexpr int local_var = 1;
      f([] (auto i) -> C<noexcept(i + local_var)> { return 1; });
    }
  }

  namespace tmpl_fn
  {
    template<typename T>
    void g()
    {
      T local_var = 1;
      f([] (auto i) -> C<noexcept(i + local_var)> { return 1; });
    }

    template void g<int>();
  }

  namespace tmpl_fn_constexpr
  {
    template<typename T>
    void g()
    {
      constexpr T local_var = 1;
      f([] (auto i) -> C<noexcept(i + local_var)> { return 1; });
    }

    template void g<int>();
  }
}

namespace trailing_return_sizeof
{
  template<typename T>
  inline void f(T && fn)
  { fn(1); }

  template<int I>
  struct C
  {
    C(int);
  };

  namespace non_tmpl_fn
  {
    void g()
    {
      int local_var = 1;
      f([] (auto i) -> C<sizeof(i + local_var)> { return 1; });
    }
  }

  namespace non_tmpl_fn_constexpr
  {
    void g()
    {
      constexpr int local_var = 1;
      f([] (auto i) -> C<sizeof(i + local_var)> { return 1; });
    }
  }

  namespace tmpl_fn
  {
    template<typename T>
    void g()
    {
      T local_var = 1;
      f([] (auto i) -> C<sizeof(i + local_var)> { return 1; });
    }

    template void g<int>();
  }

  namespace tmpl_fn_constexpr
  {
    template<typename T>
    void g()
    {
      constexpr T local_var = 1;
      f([] (auto i) -> C<sizeof(i + local_var)> { return 1; });
    }

    template void g<int>();
  }
}

namespace trailing_return_alignof
{
  template<typename T>
  inline void f(T && fn)
  { fn(1); }

  template<int I>
  struct C
  {
    C(int);
  };

  namespace non_tmpl_fn
  {
    void g()
    {
      int local_var = 1;
      f([] (auto i) -> C<alignof(i + local_var)> { return 1; });
    }
  }

  namespace non_tmpl_fn_constexpr
  {
    void g()
    {
      constexpr int local_var = 1;
      f([] (auto i) -> C<alignof(i + local_var)> { return 1; });
    }
  }

  namespace tmpl_fn
  {
    template<typename T>
    void g()
    {
      T local_var = 1;
      f([] (auto i) -> C<alignof(i + local_var)> { return 1; });
    }

    template void g<int>();
  }

  namespace tmpl_fn_constexpr
  {
    template<typename T>
    void g()
    {
      constexpr T local_var = 1;
      f([] (auto i) -> C<alignof(i + local_var)> { return 1; });
    }

    template void g<int>();
  }
}

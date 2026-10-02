//type:fp
//options:--c++20 --clang_version 210100

namespace simple_func
{
  constexpr auto f(int i, int j)
  {
    return i + j;
  }

  static_assert(f(1, 2L) == 3);
  static_assert(__builtin_invoke(f, 1, 2L) == 3);
  static_assert(__builtin_invoke(&f, 1, 2L) == 3);

  constexpr int (&r)(int, int) = f;
  static_assert(__builtin_invoke(r, 1, 2L) == 3);
}

namespace fn_ptr
{
  constexpr auto f(int i, int j)
  {
    return i + j;
  }

  constexpr auto f(int i, long j)
  {
    return 0;
  }

  constexpr int (*p)(int, int) = f;

  static_assert(p(1, 2L) == 3);
  static_assert(__builtin_invoke(p, 1, 2L) == 3);
}

namespace functor_single
{
  struct B
  {
    constexpr auto operator () (int i, int j) const
    {
      return i + j;
    }
  };

  constexpr B b;

  static_assert(b(1, 2L) == 3);
  static_assert(__builtin_invoke(b, 1, 2L) == 3);
}

namespace functor_multi
{
  struct B
  {
    constexpr auto operator () (int i, int j) const
    {
      return i + j;
    }

    constexpr auto operator () (void *, int) const
    {
      return 0;
    }
  };

  constexpr B b;

  static_assert(b(1, 2L) == 3);
  static_assert(__builtin_invoke(b, 1, 2L) == 3);
}

namespace mbr_function
{
  struct B
  {
    constexpr auto f(int i, int j) const
    {
      return i + j;
    }
  };

  constexpr B b;
  constexpr auto mfp = &B::f;

  static_assert((b.*mfp)(1, 2L) == 3);
  static_assert(((&b)->*mfp)(1, 2L) == 3);
  static_assert(__builtin_invoke(mfp, b, 1, 2L) == 3);
  static_assert(__builtin_invoke(mfp, &b, 1, 2L) == 3);
}

namespace cls_surrogate
{
  constexpr int f(int i, int j)
  {
    return i + j;
  }

  struct B
  {
    using fn_t = int(int, int);

    constexpr operator fn_t *() const
    {
      return f;
    }
  };

  constexpr B b;

  static_assert(b(1, 2L) == 3);
  static_assert(__builtin_invoke(b, 1, 2L) == 3);
}

namespace cls_functor_and_surrogate
{
  constexpr int f(int i, long j)
  {
    return i + j;
  }

  struct B
  {
    using fn_t = int(int, long);

    constexpr auto operator () (int) const
    {
      return 0;
    }

    constexpr operator fn_t *() const
    {
      return f;
    }
  };

  constexpr B b;

  static_assert(b(1, 2L) == 3);
  static_assert(__builtin_invoke(b, 1, 2L) == 3);
}

namespace dpdt
{
  template<auto V>
  struct C
  {
    static_assert(V(1, 2L) == 3);
    static_assert(__builtin_invoke(V, 1, 2L) == 3);
  };

  constexpr int f(int i, int j)
  {
    return i + j;
  }

  struct B1
  {
    constexpr auto operator () (int i, int j) const
    {
      return i + j;
    }
  };

  struct B2
  {
    constexpr auto operator () (int i, int j) const
    {
      return i + j;
    }

    constexpr auto operator () (void *, int) const
    {
      return 0;
    }
  };

  C<&f> cf;

  constexpr B1 b1;
  C<b1> cb1;

  constexpr B2 b2;
  C<b2> cb2;
}

namespace in_substitution
{
  constexpr int f(int i, int j)
  { return i + j; }

  struct A1
  {
    constexpr int operator () (int i, int j) const
    {
      return i + j;
    }
  };

  struct A2
  {
    using fn_t = int(int, int);

    constexpr operator fn_t * () const
    {
      return f;
    }
  };

  struct A3
  {
    constexpr int f(int i, int j) const
    {
      return i + j;
    }
  };

  template<typename ... T>
  constexpr auto g(T ... t) -> decltype(__builtin_invoke(t ...))
  {
    return __builtin_invoke(t ...);
  }

  static_assert(g(f, 1, 2) == 3);
  static_assert(g(A1(), 1, 2) == 3);
  static_assert(g(A2(), 1, 2) == 3);
  static_assert(g(&A3::f, A3(), 1, 2) == 3);

  constexpr A3 a3;
  static_assert(g(&A3::f, a3, 1, 2) == 3);
  static_assert(g(&A3::f, &a3, 1, 2) == 3);
}

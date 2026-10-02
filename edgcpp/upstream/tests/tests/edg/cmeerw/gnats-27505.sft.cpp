//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<int I>
  struct X {
    static_assert(I != 1);
  };
  template<int I>
  struct C {
    template<int J>
    struct D  {
      static int f() requires (I == J) || X<I>::v || X<J>::v;
    };
  };
  int i = C<1>::D<1>::f();
}

namespace pr
{
  template<typename T>
  struct X
  {
    static_assert(sizeof(T) == 1); // fails with MSVC
  };

  template<typename T>
  struct C
  {
    template<typename U>
    struct D
    {
      static void f(U u) requires (sizeof(U) == 1) && X<T>::value;
      static int f(long);

      static void g(U u) requires (sizeof(T) == 1) && X<U>::value;
      static int g(long);
    };
  };

  int i = C<int>::D<int>::f(1) + C<int>::D<int>::g(2);
}

namespace nested_pack_expansion
{
  template<int ... I>
  struct C
  {
    template<int ... J>
    struct D
    {
      static int nontmpl() requires (((J + I) + ...) == 10);

      template<typename = void>
      static int tmpl() requires (((J + I) + ...) == 10);
    };
  };

  int i = C<1, 2>::D<3, 4>::nontmpl() + C<1, 2>::D<3, 4>::tmpl();
}

namespace member_fn
{
  template<typename T>
  struct B
  {
    constexpr bool nondep(T)
    { return true; }

    int nontmpl1() requires requires { nondep(0); }
    {
      return 1;
    }

    int nontmpl2() requires (!requires { nondep(""); })
    {
      return 1;
    }

    template<typename = void>
    int tmpl1() requires requires { nondep(0); }
    {
      return 1;
    }

    template<typename = void>
    int tmpl2() requires (!requires { nondep(""); })
    {
      return 1;
    }
  };

  B<int> b;
  int i = b.nontmpl1() + b.nontmpl2() + b.tmpl1() + b.tmpl2();
}

namespace data_mbr
{
  template<typename T>
  struct B
  {
    T mbr;

    int nontmpl1() requires requires { mbr + 0; }
    {
      static_assert(requires { mbr + 0; });
      return 1;
    }

    int nontmpl2() requires (!requires { mbr * ""; })
    {
      static_assert(!requires { mbr * ""; });
      return 1;
    }

    template<typename = void>
    int tmpl1() requires requires { mbr + 0; }
    {
      static_assert(requires { mbr + 0; });
      return 1;
    }

    template<typename = void>
    int tmpl2() requires (!requires { mbr * ""; })
    {
      static_assert(!requires { mbr * ""; });
      return 1;
    }
  };

  B<int> b;
  int i = b.nontmpl1() + b.nontmpl2() + b.tmpl1() + b.tmpl2();
}

namespace static_data_mbr
{
  template<typename T>
  struct B
  {
    static T mbr;

    int nontmpl1() requires requires { mbr + 0; }
    {
      static_assert(requires { mbr + 0; });
      return 1;
    }

    int nontmpl2() requires (!requires { mbr * ""; })
    {
      static_assert(!requires { mbr * ""; });
      return 1;
    }

    template<typename = void>
    int tmpl1() requires requires { mbr + 0; }
    {
      static_assert(requires { mbr + 0; });
      return 1;
    }

    template<typename = void>
    int tmpl2() requires (!requires { mbr * ""; })
    {
      static_assert(!requires { mbr * ""; });
      return 1;
    }
  };

  B<int> b;
  int i = b.nontmpl1() + b.nontmpl2() + b.tmpl1() + b.tmpl2();
}

namespace data_mbr_this
{
  template<typename T>
  struct B
  {
    T mbr;

    int nontmpl1() requires requires { this->mbr + 0; }
    {
      static_assert(requires { this->mbr + 0; });
      return 1;
    }

    int nontmpl2() requires (!requires { this->mbr * ""; })
    {
      static_assert(!requires { this->mbr * ""; });
      return 1;
    }

    template<typename = void>
    int tmpl1() requires requires { this->mbr + 0; }
    {
      static_assert(requires { this->mbr + 0; });
      return 1;
    }

    template<typename = void>
    int tmpl2() requires (!requires { this->mbr * ""; })
    {
      static_assert(!requires { this->mbr * ""; });
      return 1;
    }
  };

  B<int> b;
  int i = b.nontmpl1() + b.nontmpl2() + b.tmpl1() + b.tmpl2();
}

namespace static_data_mbr_this
{
  template<typename T>
  struct B
  {
    static T mbr;

    int nontmpl1() requires requires { this->mbr + 0; }
    {
      static_assert(requires { this->mbr + 0; });
      return 1;
    }

    int nontmpl2() requires (!requires { this->mbr * ""; })
    {
      static_assert(!requires { this->mbr * ""; });
      return 1;
    }

    template<typename = void>
    int tmpl1() requires requires { this->mbr + 0; }
    {
      static_assert(requires { this->mbr + 0; });
      return 1;
    }

    template<typename = void>
    int tmpl2() requires (!requires { this->mbr * ""; })
    {
      static_assert(!requires { this->mbr * ""; });
      return 1;
    }
  };

  B<int> b;
  int i = b.nontmpl1() + b.nontmpl2() + b.tmpl1() + b.tmpl2();
}

namespace access_control
{
  struct B;

  class A
  {
    friend struct B;
    int i;
  };

  template<typename T>
  struct C
  {
    static void g() requires true;
  };

  struct B
  {
    int f(A a)
    {
      int i = a.i;
      C<int>::g();
      return i + a.i;
    }
  };
}

namespace friend_pack
{
  template<bool B, typename ... T>
  concept X = true;

  template<typename ... T>
  struct C
  {
    template<bool B>
    struct D
    {
      void f() requires X<B, T ...>;

      friend bool operator ==(D, D) requires X<B, T ...>
      {
        return true;
      }
    };
  };

  bool g(C<int>::D<false> d)
  {
    return d == d;
  }
}

namespace friend_nested_class
{
  template<typename>
  concept X = true;

  template<typename>
  struct V
  {
  private:
    template<bool>
    struct D
    {
      friend bool operator!(const D&) requires X<D>
      {
        return true;
      }
    };

  public:
    static D<false> f()
    {
      return { };
    }
  };

  void f()
  {
    !V<int>::f();
  }
}

namespace constrained_partial_spec
{
  template<typename>
  concept X = true;

  template<typename T1>
  struct C
  {
    template<typename T2>
    struct V
    { };

    template<typename TP2> requires true
    struct V<TP2>
    {
      template<bool B>
      struct D
      {
        bool operator!() const requires X<D>
        {
          return true;
        }
      };
    };
  };

  template<typename T2>
  struct V
  { };

  template<typename TP2> requires true
  struct V<TP2>
  {
    template<typename T1>
    struct C
    {
      template<bool B>
      struct D
      {
        bool operator!() const requires X<D>
        {
          return true;
        }
      };
    };
  };

  void f(C<void>::V<int>::D<false> d1, V<int>::C<void>::D<false> d2)
  {
    !d1;
    !d2;
  }
}

namespace constrained_partial_spec_ptr_1
{
  template<typename>
  concept X = true;

  template<typename U>
  struct V
  { };

  template<typename T> requires true
  struct V<T *>
  {
    template<bool>
    struct D
    {
      bool operator!() const requires X<D>
      {
        return true;
      }
    };
  };

  template<typename T>
  struct V<T *>
  { };

  void f(V<int *>::D<false> d)
  {
    !d;
  }
}

namespace constrained_partial_spec_ptr_2
{
  template<typename>
  concept X = true;

  template<typename U>
  struct V
  { };

  template<typename T>
  struct V<T *>
  { };

  template<typename T> requires true
  struct V<T *>
  {
    template<bool>
    struct D
    {
      bool operator!() const requires X<D>
      {
        return true;
      }
    };
  };

  void f(V<int *>::D<false> d)
  {
    !d;
  }
}

namespace befriending_constrained_function
{
  template<typename T>
  struct C
  {
    static bool f() requires T::v
    {
      return T::v;
    }
  };

  class Y
  {
    template<typename T>
    friend bool C<T>::f() requires T::v; // clang seems to complain here

  private:
    static constexpr bool v = true;
  };

  bool b = C<Y>::f();
}

namespace befriending_constrained_template_function
{
  template<typename T>
  bool f() requires T::v
  {
    return T::v;
  }

  class Y
  {
    template<typename T>
    friend bool f() requires T::v;

  private:
    static constexpr bool v = true;
  };

  bool b = f<Y>();
}

namespace partially_substituted_parent
{
  template<typename T>
  concept X1 = true;

  struct B
  {
    template<typename T>
    struct N { };
  };

  template<typename T>
  concept X2 = sizeof(T) != 0;

  template<typename Y>
  struct C {
    template<typename U> requires X2<typename Y::template N<U>>
    struct N {
      using A = typename Y::template N<U>;

      struct P
      {
        static int f() requires X1<A>;
      };

      template<typename = void>
      struct Q
      {
        static int f() requires X1<A>;
      };
    };
  };

  int i = C<B>::N<int>::P::f();
  int j = C<B>::N<int>::Q<>::f();
}

namespace outer_pack_with_inner_alias
{
  template<typename>
  concept X = true;

  template<typename T, typename ... U>
  struct V
  {
    template<bool B>
    struct D
    {
      using A = D<B>;

      bool operator!() const requires X<A>
      {
        return true;
      }
    };
  };

  void f(V<int *, void>::D<false> d)
  {
    !d;
  }
}

namespace outer_pack_with_inner_class
{
  template<typename>
  concept X = true;

  template<typename T, typename ... U>
  struct V
  {
    template<bool B>
    struct D
    {
      struct A
      { };

      bool operator!() const requires X<A>
      {
        return true;
      }
    };
  };

  void f(V<int *, void>::D<false> d)
  {
    !d;
  }
}

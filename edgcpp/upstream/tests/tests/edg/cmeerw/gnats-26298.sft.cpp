//type:fp
//options:--c++20 -A:--c++20 --gn 130100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T, bool B = true>
  struct A {
    T t;
    int f(int) requires (!requires { t.i; });
    template<typename U> int g(U) requires requires { requires B; };
  };
  int i = A<int>().f(0);  // Previously a spurious error.  Now okay.
  int j = A<int>().g(0);  // Previously a spurious error.  Now okay.
}

namespace minimal_int
{
  template<typename T>
  struct A {
    T t;
    int f() requires (!requires { t.i; });
  };
  int i = A<int>().f();
}

namespace minimal_class
{
  struct C { };
  template<typename T>
  struct A {
    T t;
    int f() requires (!requires { t.i; });
  };
  int i = A<C>().f();
}

namespace minimal_nested
{
  template<bool B>
  struct A {
    template<typename U> int f(U) requires requires { requires B; };
  };
  int i = A<true>().f(0);
}

namespace param_member_access
{
  struct B {
    int i;
  };

  struct D {
    int i;
  };

  template<typename T>
  constexpr bool f(T t)
  {
    return requires { t.D::i; };
  }

  static_assert(!f(B{}));
  static_assert(f(D{}));
}

namespace param_derived_to_base
{
  struct B
  {
    int i;
  };

  struct D : B
  { };

  template<typename T>
  constexpr bool f(T t)
  {
    return requires { t.D::i; };
  }

  static_assert(f(D()));
}

namespace member_access
{
  template<typename T>
  struct C
  {
    constexpr bool f()
    {
      return requires { m->f(); };
    }

    constexpr bool g()
    {
      return requires { m->i; };
    }

    T *m;
  };

  struct B1
  {
    int i;
    void f();
  };

  struct B2 { };

  struct B3
  {
    void i();
    void f(int);
  };

  static_assert(C<B1>().f());
  static_assert(C<B1>().g());
  static_assert(!C<B2>().f());
  static_assert(!C<B2>().g());
  static_assert(!C<B3>().f());
  static_assert(!C<B3>().g());
  static_assert(!C<int>().f());
  static_assert(!C<int>().g());
}

namespace qualified_member_access
{
  template<typename T, typename U>
  struct C
  {
    constexpr bool f()
    {
      return requires { m->U::f(); };
    }

    constexpr bool g()
    {
      return requires { m->U::i; };
    }

    T *m;
  };

  struct B1
  {
    int i;
    void f();
  };

  struct B2 { };

  struct B3
  {
    void i();
    void f(int);
  };

  static_assert(C<B1, B1>().f());
  static_assert(C<B1, B1>().g());
  static_assert(!C<B1, B3>().f());
  static_assert(!C<B1, B3>().g());
  static_assert(!C<B2, B1>().f());
  static_assert(!C<B2, B1>().g());
  static_assert(!C<B3, B3>().f());
  static_assert(!C<B3, B3>().g());
  static_assert(!C<B3, B1>().f());
  static_assert(!C<B3, B1>().g());
  static_assert(!C<int, B1>().f());
  static_assert(!C<int, B1>().g());
}

namespace non_dpdt_qualifier_member_access
{
  struct D
  {
    int i;
    void f();
  };

  template<typename T>
  struct C
  {
    constexpr bool f()
    {
      return requires { m->D::f(); };
    }

    constexpr bool g()
    {
      return requires { m->D::i; };
    }

    T *m;
  };

  struct B
  {
    int i;
    void f();
  };

  static_assert(C<D>().f());
  static_assert(C<D>().g());
  static_assert(!C<B>().f());
  static_assert(!C<B>().g());
}

namespace non_type_template_name
{
  template<bool V, typename T> struct A
  {
    void f()
    {
      static_assert(V == requires { T::template C< int > (); });
    }
  };

  struct B
  {
    template<class T>
    struct C
    { };
  };

  struct D
  {
    template<class T>
    static void C()
    { }
  };

  auto v = (A<false, B>().f(), A<true, D>().f(), 0);
}

namespace ill_formed_ctor
{
  template <class T> struct A
  {
    void f()
    {
      static_assert(!requires { T::template C<int>(); });
    }
  };

  struct B
  {
    template <class T> struct C { };
    struct D { };
  };

  void g()
  {
    A<B>().f();
  }
}

namespace ill_formed_ctor_var
{
  template<typename T>
  struct C
  { };

  template<typename T>
  constexpr bool v = requires (T t) { C(t); };

  static_assert(!v<int>);
}

namespace substitute_operator_new
{
  template <class T> struct A
  {
    static constexpr T f()
    {
      return requires { requires sizeof new T[1] { operator new } == 0; };
    }
  };

  static_assert(!A<int>::f());
}


namespace aggregate_init
{
  template<typename U, class T> constexpr bool f(T args)
  {
    struct A { U x; };
    return requires { A { args } ; } ;
  }

  static_assert(f<int>(1));
  static_assert(!f<int *>(1));
}

namespace aggregate_init_nested
{
  template<typename U, class T> constexpr bool f(T args)
  {
    struct A {
      struct B {
        U x;
      };
    };
    void A();
    return requires { (typename A::B { args }) ; } ;
  }

  static_assert(f<int>(1));
  static_assert(!f<int *>(1));
}

namespace template_fn_result_member
{
  template<typename T>
  void foo()
  {
    T::dont_instantiate;
  }

  template<typename T>
  void f()
  {
    static_assert(!requires { foo<T>().x; });
    noexcept(foo<T>());
  }

  auto v = (f<int>(), 0);
}

namespace nested_requirement_in_nested_template
{
  template<typename T> struct A
  {
    constexpr A(int) { }
    constexpr bool operator == (int) const
    {
      return true;
    }
  };

  template<typename T> struct C
  {
    static int f()
    {
      using type = decltype ( [ ] ( auto x ) {
        return requires { requires A < T > ( int{} ) == int{} ; };
      } ( 1 ) );
      return 0;
    }
  };

  auto value = C<int>::f();
}

namespace nested_requirement_in_lambda
{
  struct S { using blah = void; };

  template<typename, typename>
  static constexpr bool is_same_v = false;

  template<typename T>
  static constexpr bool is_same_v<T, T> = true;

  template <typename T> constexpr bool trait = !is_same_v<T, S>;
  template <typename T> concept C = trait<T>;

  template<typename U>
  auto f1()
  {
    return []<typename T>(T) {
      static_assert(requires { requires C<U> && (C<T> || C<T>); });
      return 0;
    };
  }

  template<typename U>
  auto f2()
  {
    return []<typename T>(T) {
      static_assert(!requires { requires C<U> && (C<T> || C<T>); });
      return 0;
    };
  }

  auto g1 = f1<int>();
  int n = g1(0);

  auto g2 = f2<int>();
  int m = g2(S{});
}

namespace local_types
{
  template<typename U> constexpr bool f(bool v)
  {
    struct A {
      struct B {
        U u;
        U u2;
      };

      U u;
    };
    void A();

    if (v)
    {
      void A();
      struct A {
        struct B { const char *m; };
      };

      return requires {
        (typename A::B{""});
      } && !requires {
        (typename A::B{1, 2});
      };
    }
    else
    {
      return requires {
        (typename A::B{1, 2});
      } && !requires {
        (typename A::B{""});
      };
    }
  }

  static_assert(f<int>(false));
  static_assert(f<int>(true));
}

//type:fn
//options:--c++17:--c++17 --gnu_version 100100:--c++17 --gnu_version 110100

namespace fails_with_gcc_10
{
  namespace no_templ_args
  {
    template<typename ... T>
    struct C
    {
      template<typename ... U>
      static constexpr int f()
      { return 0; }

      template<typename U, int = f()>
      C(U);
    };

    C c(1);                     // deduction failed for gcc 10
  }

  namespace templ_args
  {
    template<typename ... T>
    struct C
    {
      template<typename ... U>
      static constexpr int f()
      { return 0; }

      template<typename U, int = f<T ...>()>
      C(U);
    };

    C c(1);                     // deduction failed for gcc 10
  }

  namespace qualified_name_templ_args
  {
    template<typename ... T>
    struct C
    {
      template<typename ... U>
      static constexpr int f()
      { return 0; }

      template<typename U, int = C<T ...>::f<T ...>()>
      C(U);
    };

    C c(1);                     // deduction failed for gcc 10
  }
}

namespace accepted_by_gcc_10
{
  template<typename ...>
  struct B
  { };

  template<>
  struct B<>;

  template<typename ... T>
  struct C : public B<T ...>    // use of incomplete type for gcc 11
  {
    template<typename ... U>
    static constexpr bool g() { return false; }
    template<typename ... U, bool = g<U ...>()> C(U ...);
  };

  template<typename ... U> C(U ...) -> C<U ...>;

  C c{1, 2};
}

namespace accepted_by_gcc_10_additional_arg
{
  template<typename ...>
  struct B
  { };

  template<>
  struct B<>;

  template<typename... T>
  struct C : public B<T ...>    // use of incomplete type for gcc 11
  {
    template<typename... U> using P = bool;

    template<typename ... U>
    static constexpr bool g() { return false; }
    template<typename ... U, int = g<U ...>(), P<U ...> = true> C(U ...);
  };

  template<typename... U> C(U ...) -> C<U ...>;

  C c{1, 2};
}

namespace not_accepted_by_gcc_10
{
  template<typename... T>
  struct C
  {
    template<typename ... U>
    static constexpr bool g() { return false; }

    template<typename ... U, int V1 = g<U ...>()> C(U ...);
  };

  C c{1, 2};                    // deduction failed for gcc 10
}
